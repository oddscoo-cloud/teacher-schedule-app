namespace backend.Infrastructure.Repositories;

using backend.Core.Entities;
using backend.Core.Interfaces.Repositories;
using backend.Data;
using Microsoft.EntityFrameworkCore;

public class StudentRepository : IStudentRepository
{
    private readonly AppDbContext _context;

    public StudentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<StudentEntity?> GetByIdAsync(int id)
    {
        return await _context.StudentEntities
            .Include(s => s.TeacherAssignments)
            .ThenInclude(sta => sta.Teacher)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<StudentEntity?> GetByUserIdAsync(int userId)
    {
        return await _context.StudentEntities
            .Include(s => s.TeacherAssignments)
            .ThenInclude(sta => sta.Teacher)
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<IEnumerable<StudentEntity>> GetBySchoolIdAsync(int schoolId)
    {
        return await _context.StudentEntities
            .Include(s => s.TeacherAssignments)
            .ThenInclude(sta => sta.Teacher)
            .Where(s => s.SchoolId == schoolId)
            .ToListAsync();
    }

    public async Task<IEnumerable<StudentEntity>> GetByTeacherIdAsync(int teacherId)
    {
        return await _context.StudentEntities
            .Include(s => s.TeacherAssignments)
            .ThenInclude(sta => sta.Teacher)
            .Where(s => s.TeacherAssignments.Any(ta => ta.TeacherId == teacherId))
            .ToListAsync();
    }

    public async Task<StudentEntity> CreateAsync(StudentEntity student)
    {
        student.CreatedAt = DateTime.UtcNow;
        _context.StudentEntities.Add(student);
        await _context.SaveChangesAsync();
        return student;
    }

    public async Task<StudentEntity> UpdateAsync(StudentEntity student)
    {
        student.UpdatedAt = DateTime.UtcNow;
        _context.StudentEntities.Update(student);
        await _context.SaveChangesAsync();
        return student;
    }
}
