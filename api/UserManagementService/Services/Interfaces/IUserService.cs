using UserManagementService.DTOs;
using UserManagementService.Models;

namespace UserManagementService.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDTO?> GetByIdAsync(Guid id);
        Task<List<UserDTO>> GetByRoleAsync(UserRole role);
        Task<List<UserDTO>> GetAllAsync();
        Task<UserDTO?> UpdateAsync(Guid id, UpdateUserRequest request);
        Task<bool> DeactivateAsync(Guid id);
    }
}
