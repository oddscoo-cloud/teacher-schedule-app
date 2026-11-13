namespace backend.Core.Interfaces.Repositories;

using backend.Core.Entities;

public interface ITeacherRepository
{
    Task<TeacherEntity?> GetByIdAsync(int id);
    Task<TeacherEntity?> GetByUserIdAsync(int userId);
    Task<IEnumerable<TeacherEntity>> GetBySchoolIdAsync(int schoolId);
    Task<TeacherEntity> CreateAsync(TeacherEntity teacher);
    Task<TeacherEntity> UpdateAsync(TeacherEntity teacher);
}
