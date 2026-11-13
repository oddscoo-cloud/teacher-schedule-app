namespace backend.Infrastructure.Repositories;

using backend.Core.Entities;
using backend.Core.Interfaces.Repositories;
using backend.Data;
using Microsoft.EntityFrameworkCore;

public class WorkspaceMembershipRepository : IWorkspaceMembershipRepository
{
    private readonly AppDbContext _context;

    public WorkspaceMembershipRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<WorkspaceMembershipEntity?> GetByIdAsync(int id)
    {
        return await _context.WorkspaceMemberships
            .Include(wm => wm.School)
            .FirstOrDefaultAsync(wm => wm.Id == id);
    }

    public async Task<IEnumerable<WorkspaceMembershipEntity>> GetByUserIdAsync(int userId)
    {
        return await _context.WorkspaceMemberships
            .Include(wm => wm.School)
            .Where(wm => wm.UserId == userId)
            .ToListAsync();
    }

    public async Task<WorkspaceMembershipEntity?> GetUserSchoolMembershipAsync(int userId, int schoolId)
    {
        return await _context.WorkspaceMemberships
            .Include(wm => wm.School)
            .FirstOrDefaultAsync(wm => wm.UserId == userId && wm.SchoolId == schoolId);
    }

    public async Task<WorkspaceMembershipEntity?> GetDefaultMembershipAsync(int userId)
    {
        return await _context.WorkspaceMemberships
            .Include(wm => wm.School)
            .FirstOrDefaultAsync(wm => wm.UserId == userId && wm.IsDefault);
    }

    public async Task<WorkspaceMembershipEntity> CreateAsync(WorkspaceMembershipEntity membership)
    {
        membership.JoinedAt = DateTime.UtcNow;
        _context.WorkspaceMemberships.Add(membership);
        await _context.SaveChangesAsync();
        return membership;
    }

    public async Task<WorkspaceMembershipEntity> UpdateAsync(WorkspaceMembershipEntity membership)
    {
        _context.WorkspaceMemberships.Update(membership);
        await _context.SaveChangesAsync();
        return membership;
    }

    public async Task DeleteAsync(int id)
    {
        var membership = await GetByIdAsync(id);
        if (membership != null)
        {
            _context.WorkspaceMemberships.Remove(membership);
            await _context.SaveChangesAsync();
        }
    }
}
