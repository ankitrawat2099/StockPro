using AuthService.Data;
using AuthService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly AuthDbContext _context;

    public AuthRepository(AuthDbContext context)
    {
        _context = context;
    }

    //Find by Email
    public async Task<AppUser?> FindByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    // Find by UserId
    public async Task<AppUser?> FindByUserIdAsync(Guid userId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
    }

    // Exists by Email
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    // Find all by Role
    public async Task<List<AppUser>> FindAllByRoleAsync(string role)
    {
        return await _context.Users.Where(u => u.Role == role).ToListAsync();
    }

    // Find by Department
    public async Task<List<AppUser>> FindByDepartmentAsync(string department)
    {
        return await _context.Users.Where(u => u.Department == department).ToListAsync();
    }

    // Find by IsActive
    public async Task<List<AppUser>> FindByIsActiveAsync(bool isActive)
    {
        return await _context.Users.Where(u => u.IsActive == isActive).ToListAsync();
    }

    // Delete by UserId (Hard delete as per diagram)
    public async Task DeleteByUserIdAsync(Guid userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}