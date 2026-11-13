namespace backend.Infrastructure.Repositories;

using backend.Core.Entities;
using backend.Core.Interfaces.Repositories;
using backend.Data;
using Microsoft.EntityFrameworkCore;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserEntity?> GetByIdAsync(int id)
    {
        return await _context.UserEntities.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<UserEntity?> GetByEmailAsync(string email)
    {
        return await _context.UserEntities.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<UserEntity> CreateAsync(UserEntity user)
    {
        user.CreatedAt = DateTime.UtcNow;
        _context.UserEntities.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<UserEntity> UpdateAsync(UserEntity user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.UserEntities.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task DeleteAsync(int id)
    {
        var user = await GetByIdAsync(id);
        if (user != null)
        {
            _context.UserEntities.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
