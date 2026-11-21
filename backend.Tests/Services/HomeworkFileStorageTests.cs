using backend.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;

namespace backend.Tests.Services;

public sealed class HomeworkFileStorageTests : IDisposable
{
    private readonly string _storagePath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "TestData",
        "HomeworkFileStorage",
        Guid.NewGuid().ToString("N"));
    private readonly HomeworkFileStorage _storage;

    public HomeworkFileStorageTests()
    {
        var options = new HomeworkFileStorageOptions
        {
            StoragePath = _storagePath,
            MaxFileSizeBytes = 32,
            MaxFilesPerSubmission = 2
        };
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(item => item.ContentRootPath).Returns(Directory.GetCurrentDirectory());
        _storage = new HomeworkFileStorage(Options.Create(options), environment.Object);
    }

    [Fact]
    public async Task SaveAsync_UsesImageSignatureInsteadOfSubmittedNameAndContentType()
    {
        var file = CreateFile(new byte[] { 0xff, 0xd8, 0xff, 0x00, 0xff, 0xd9 }, "not-an-image.txt", "text/plain");

        var stored = await _storage.SaveAsync(file);

        Assert.Matches("^[0-9a-f]{32}\\.jpg$", stored.StorageName);
        Assert.Equal("image/jpeg", stored.ContentType);
        Assert.Equal("not-an-image.txt", stored.DisplayName);
        Assert.Equal(6, stored.SizeBytes);
        Assert.Equal(6, new FileInfo(Path.Combine(_storagePath, stored.StorageName)).Length);
    }

    [Fact]
    public async Task SaveAsync_RejectsSpoofedPngAndRemovesPartialFile()
    {
        var spoofed = new byte[45];
        new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }.CopyTo(spoofed, 0);
        new byte[] { 0, 0, 0, 0, (byte)'I', (byte)'E', (byte)'N', (byte)'D' }.CopyTo(spoofed, spoofed.Length - 8);

        await Assert.ThrowsAsync<InvalidHomeworkFileException>(() =>
            _storage.SaveAsync(CreateFile(spoofed, "photo.png", "image/png")));

        Assert.Empty(Directory.GetFiles(_storagePath));
    }

    [Fact]
    public async Task SaveAsync_RejectsStreamThatExceedsLimitAndCleansUp()
    {
        var file = new InconsistentLengthFormFile(
            Enumerable.Repeat((byte)1, 64).ToArray(),
            reportedLength: 4);

        await Assert.ThrowsAsync<InvalidHomeworkFileException>(() => _storage.SaveAsync(file));

        Assert.Empty(Directory.GetFiles(_storagePath));
    }

    [Fact]
    public void OpenRead_RejectsNamesOutsideGeneratedStorageNameFormat()
    {
        Assert.Throws<InvalidOperationException>(() => _storage.OpenRead("..\\outside.jpg"));
        Assert.Throws<InvalidOperationException>(() => _storage.OpenRead("folder\\00000000000000000000000000000000.jpg"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_storagePath))
            Directory.Delete(_storagePath, recursive: true);
    }

    private static IFormFile CreateFile(byte[] bytes, string fileName, string contentType)
    {
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, stream.Length, "Files", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private sealed class InconsistentLengthFormFile : IFormFile
    {
        private readonly byte[] _bytes;

        public InconsistentLengthFormFile(byte[] bytes, long reportedLength)
        {
            _bytes = bytes;
            Length = reportedLength;
        }

        public string ContentType => "image/png";
        public string ContentDisposition => string.Empty;
        public IHeaderDictionary Headers { get; } = new HeaderDictionary();
        public string Name => "Files";
        public string FileName => "oversized.png";
        public long Length { get; }
        public void CopyTo(Stream target) => target.Write(_bytes);
        public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default) =>
            target.WriteAsync(_bytes, cancellationToken).AsTask();
        public Stream OpenReadStream() => new MemoryStream(_bytes);
    }
}
