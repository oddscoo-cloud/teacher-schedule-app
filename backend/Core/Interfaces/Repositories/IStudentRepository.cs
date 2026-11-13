namespace backend.Core.Interfaces.Repositories;

using backend.Core.Entities;

public interface IStudentRepository
{
    Task<StudentEntity?> GetByIdAsync(int id);
    Task<StudentEntity?> GetByUserIdAsync(int userId);
    Task<IEnumerable<StudentEntity>> GetBySchoolIdAsync(int schoolId);
    Task<IEnumerable<StudentEntity>> GetByTeacherIdAsync(int teacherId);
    Task<StudentEntity> CreateAsync(StudentEntity student);
    Task<StudentEntity> UpdateAsync(StudentEntity student);
}
