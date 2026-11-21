namespace backend.Infrastructure.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

public sealed record StoredHomeworkFile(string StorageName, string DisplayName, string ContentType, long SizeBytes);

public sealed class InvalidHomeworkFileException : Exception
{
    public InvalidHomeworkFileException(string message) : base(message)
    {
    }
}

public interface IHomeworkFileStorage
{
    Task<StoredHomeworkFile> SaveAsync(IFormFile file, CancellationToken cancellationToken = default);
    Stream OpenRead(string storageName);
    Task DeleteAsync(string storageName);
}

public sealed class HomeworkFileStorage : IHomeworkFileStorage
{
    private static readonly byte[] PngSignature = [137, 80, 78, 71, 13, 10, 26, 10];
    private readonly string _rootPath;
    private readonly HomeworkFileStorageOptions _options;

    public HomeworkFileStorage(IOptions<HomeworkFileStorageOptions> options, IWebHostEnvironment environment)
    {
        _options = options.Value;
        var storagePath = string.IsNullOrWhiteSpace(_options.StoragePath)
            ? Path.Combine(environment.ContentRootPath, "App_Data", "homework")
            : _options.StoragePath;
        _rootPath = Path.GetFullPath(Path.IsPathRooted(storagePath)
            ? storagePath
            : Path.Combine(environment.ContentRootPath, storagePath));
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<StoredHomeworkFile> SaveAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length <= 0 || file.Length > _options.MaxFileSizeBytes)
            throw new InvalidHomeworkFileException($"Each image must be between 1 byte and {_options.MaxFileSizeBytes} bytes.");

        var tempName = $"{Guid.NewGuid():N}.upload";
        var tempPath = GetContainedPath(tempName);
        try
        {
            long totalBytes = 0;
            await using (var output = new FileStream(
                tempPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
            await using (var input = file.OpenReadStream())
            {
                var buffer = new byte[81920];
                int bytesRead;
                while ((bytesRead = await input.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    totalBytes += bytesRead;
                    if (totalBytes > _options.MaxFileSizeBytes)
                        throw new InvalidHomeworkFileException($"Each image must be no larger than {_options.MaxFileSizeBytes} bytes.");
                    await output.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                }
            }

            var (contentType, extension) = await DetectImageTypeAsync(tempPath, cancellationToken);
            if (!_options.AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
                throw new InvalidHomeworkFileException("Only JPEG and PNG images are allowed.");

            var storageName = $"{Guid.NewGuid():N}{extension}";
            var storagePath = GetContainedPath(storageName);
            File.Move(tempPath, storagePath);
            return new StoredHomeworkFile(storageName, GetDisplayName(file.FileName), contentType, totalBytes);
        }
        catch
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
            throw;
        }
    }

    public Stream OpenRead(string storageName)
    {
        return new FileStream(
            GetContainedPath(storageName),
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
    }

    public Task DeleteAsync(string storageName)
    {
        var path = GetContainedPath(storageName);
        if (File.Exists(path))
            File.Delete(path);
        return Task.CompletedTask;
    }

    private string GetContainedPath(string storageName)
    {
        if (string.IsNullOrWhiteSpace(storageName) ||
            storageName != Path.GetFileName(storageName) ||
            !System.Text.RegularExpressions.Regex.IsMatch(storageName, @"\A[0-9a-f]{32}\.(upload|png|jpg)\z"))
            throw new InvalidOperationException("Invalid stored file name.");

        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, storageName));
        var rootPrefix = _rootPath.EndsWith(Path.DirectorySeparatorChar)
            ? _rootPath
            : _rootPath + Path.DirectorySeparatorChar;
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!fullPath.StartsWith(rootPrefix, comparison))
            throw new InvalidOperationException("Stored file path escaped its configured directory.");
        return fullPath;
    }

    private static async Task<(string ContentType, string Extension)> DetectImageTypeAsync(
        string path,
        CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, true);
        if (stream.Length >= 45)
        {
            var header = new byte[24];
            await stream.ReadExactlyAsync(header, cancellationToken);
            if (header.AsSpan(0, PngSignature.Length).SequenceEqual(PngSignature) &&
                header.AsSpan(8, 4).SequenceEqual(new byte[] { 0, 0, 0, 13 }) &&
                header.AsSpan(12, 4).SequenceEqual("IHDR"u8) &&
                System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(16, 4)) > 0 &&
                System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(20, 4)) > 0)
            {
                stream.Seek(-8, SeekOrigin.End);
                var trailer = new byte[8];
                await stream.ReadExactlyAsync(trailer, cancellationToken);
                if (trailer.AsSpan(0, 4).SequenceEqual(new byte[] { 0, 0, 0, 0 }) &&
                    trailer.AsSpan(4, 4).SequenceEqual("IEND"u8))
                    return ("image/png", ".png");
            }
        }

        if (stream.Length >= 4)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var header = new byte[3];
            await stream.ReadExactlyAsync(header, cancellationToken);
            if (header[0] == 0xff && header[1] == 0xd8 && header[2] == 0xff)
            {
                stream.Seek(-2, SeekOrigin.End);
                var trailer = new byte[2];
                await stream.ReadExactlyAsync(trailer, cancellationToken);
                if (trailer[0] == 0xff && trailer[1] == 0xd9)
                    return ("image/jpeg", ".jpg");
            }
        }

        throw new InvalidHomeworkFileException("The uploaded file is not a valid PNG or JPEG image.");
    }

    private static string GetDisplayName(string? fileName)
    {
        var name = Path.GetFileName((fileName ?? string.Empty).Replace('\\', '/'));
        var safeName = new string(name.Where(character => !char.IsControl(character)).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(safeName) ? "image" : safeName[..Math.Min(safeName.Length, 255)];
    }
}
