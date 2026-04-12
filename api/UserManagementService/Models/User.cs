namespace UserManagementService.Models
{
    public enum UserRole
    {
        Passenger,
        Driver,
        StationStaff,
        Admin
    }

    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public UserRole Role { get; set; }
        public string? PhoneNumber { get; set; }
        public string? EmployeeId { get; set; } // For staff/driver
        public string? AssignedStationId { get; set; } // For station staff
        public string? AssignedTrainId { get; set; } // For drivers
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
    }
}
