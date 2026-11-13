namespace backend.Core.Interfaces.Repositories;

using backend.Core.Entities;

public interface ISchoolRepository
{
    Task<SchoolEntity?> GetByIdAsync(int id);
    Task<SchoolEntity?> GetDefaultSchoolAsync();
    Task<SchoolEntity> CreateAsync(SchoolEntity school);
    Task<SchoolEntity> UpdateAsync(SchoolEntity school);
}
