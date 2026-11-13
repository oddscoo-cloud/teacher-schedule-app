namespace backend.Infrastructure.Repositories;

using backend.Core.Entities;
using backend.Core.Interfaces.Repositories;
using backend.Data;
using Microsoft.EntityFrameworkCore;

public class SchoolRepository : ISchoolRepository
{
    private readonly AppDbContext _context;

    public SchoolRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SchoolEntity?> GetByIdAsync(int id)
    {
        return await _context.Schools.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<SchoolEntity?> GetDefaultSchoolAsync()
    {
        return await _context.Schools.FirstOrDefaultAsync();
    }

    public async Task<SchoolEntity> CreateAsync(SchoolEntity school)
    {
        school.CreatedAt = DateTime.UtcNow;
        _context.Schools.Add(school);
        await _context.SaveChangesAsync();
        return school;
    }

    public async Task<SchoolEntity> UpdateAsync(SchoolEntity school)
    {
        school.UpdatedAt = DateTime.UtcNow;
        _context.Schools.Update(school);
        await _context.SaveChangesAsync();
        return school;
    }
}
