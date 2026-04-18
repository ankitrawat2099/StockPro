using AuthService.Entities;

namespace AuthService.Repositories;

public interface IAuthRepository
{
    Task<AppUser?> FindByEmailAsync(string email);
    Task<AppUser?> FindByUserIdAsync(Guid userId);
    Task<bool> ExistsByEmailAsync(string email);
    Task<List<AppUser>> FindAllByRoleAsync(string role);
    Task<List<AppUser>> FindByDepartmentAsync(string department);
    Task<List<AppUser>> FindByIsActiveAsync(bool isActive);
    Task DeleteByUserIdAsync(Guid userId);
}