using System.ComponentModel.DataAnnotations;

namespace UserManagementService.DTOs
{
    public class RegisterRequest
    {
        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = null!;

        [Required, MinLength(8), MaxLength(128)]
        public string Password { get; set; } = null!;

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = null!;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = null!;

        [Required]
        public string Role { get; set; } = "Passenger";

        public string? PhoneNumber { get; set; }
        public string? EmployeeId { get; set; }
        public string? AssignedStationId { get; set; }
        public string? AssignedTrainId { get; set; }
    }

    public class LoginRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public UserDTO User { get; set; } = null!;
    }

    public class UserDTO
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? EmployeeId { get; set; }
        public string? AssignedStationId { get; set; }
        public string? AssignedTrainId { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateUserRequest
    {
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        public string? PhoneNumber { get; set; }
        public string? AssignedStationId { get; set; }
        public string? AssignedTrainId { get; set; }
    }

    public class UpdateTrainStatusRequest
    {
        [Required]
        public string TrainId { get; set; } = null!;

        [Required]
        public string Status { get; set; } = null!;
    }
}
