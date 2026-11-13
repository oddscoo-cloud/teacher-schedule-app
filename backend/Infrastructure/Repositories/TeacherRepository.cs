namespace backend.Infrastructure.Repositories;

using backend.Core.Entities;
using backend.Core.Interfaces.Repositories;
using backend.Data;
using Microsoft.EntityFrameworkCore;

public class TeacherRepository : ITeacherRepository
{
    private readonly AppDbContext _context;

    public TeacherRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TeacherEntity?> GetByIdAsync(int id)
    {
        return await _context.TeacherEntities
            .Include(t => t.StudentAssignments)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<TeacherEntity?> GetByUserIdAsync(int userId)
    {
        return await _context.TeacherEntities
            .Include(t => t.StudentAssignments)
            .FirstOrDefaultAsync(t => t.UserId == userId);
    }

    public async Task<IEnumerable<TeacherEntity>> GetBySchoolIdAsync(int schoolId)
    {
        return await _context.TeacherEntities
            .Include(t => t.StudentAssignments)
            .Where(t => t.SchoolId == schoolId)
            .ToListAsync();
    }

    public async Task<TeacherEntity> CreateAsync(TeacherEntity teacher)
    {
        teacher.CreatedAt = DateTime.UtcNow;
        _context.TeacherEntities.Add(teacher);
        await _context.SaveChangesAsync();
        return teacher;
    }

    public async Task<TeacherEntity> UpdateAsync(TeacherEntity teacher)
    {
        teacher.UpdatedAt = DateTime.UtcNow;
        _context.TeacherEntities.Update(teacher);
        await _context.SaveChangesAsync();
        return teacher;
    }
}
