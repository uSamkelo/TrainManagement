using Microsoft.EntityFrameworkCore;
using UserManagementService.Data;
using UserManagementService.DTOs;
using UserManagementService.Models;
using UserManagementService.Services.Interfaces;

namespace UserManagementService.Services
{
    public class UserService : IUserService
    {
        private readonly UserManagementContext _context;

        public UserService(UserManagementContext context)
        {
            _context = context;
        }

        public async Task<UserDTO?> GetByIdAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            return user == null ? null : MapToDTO(user);
        }

        public async Task<List<UserDTO>> GetByRoleAsync(UserRole role)
        {
            return await _context.Users
                .Where(u => u.Role == role && u.IsActive)
                .Select(u => MapToDTO(u))
                .ToListAsync();
        }

        public async Task<List<UserDTO>> GetAllAsync()
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .Select(u => MapToDTO(u))
                .ToListAsync();
        }

        public async Task<UserDTO?> UpdateAsync(Guid id, UpdateUserRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            if (request.FirstName != null) user.FirstName = request.FirstName;
            if (request.LastName != null) user.LastName = request.LastName;
            if (request.PhoneNumber != null) user.PhoneNumber = request.PhoneNumber;
            if (request.AssignedStationId != null) user.AssignedStationId = request.AssignedStationId;
            if (request.AssignedTrainId != null) user.AssignedTrainId = request.AssignedTrainId;

            await _context.SaveChangesAsync();
            return MapToDTO(user);
        }

        public async Task<bool> DeactivateAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;
            user.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        private static UserDTO MapToDTO(User user) => new()
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString(),
            PhoneNumber = user.PhoneNumber,
            EmployeeId = user.EmployeeId,
            AssignedStationId = user.AssignedStationId,
            AssignedTrainId = user.AssignedTrainId,
            IsActive = user.IsActive
        };
    }
}
