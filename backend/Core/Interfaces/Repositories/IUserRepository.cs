namespace backend.Core.Interfaces.Repositories;

using backend.Core.Entities;

public interface IUserRepository
{
    Task<UserEntity?> GetByIdAsync(int id);
    Task<UserEntity?> GetByEmailAsync(string email);
    Task<UserEntity> CreateAsync(UserEntity user);
    Task<UserEntity> UpdateAsync(UserEntity user);
    Task DeleteAsync(int id);
}
