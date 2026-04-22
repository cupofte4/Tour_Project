using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tour_Project.Models
{
    public class AdminUser
    {
            public int Id { get; set; }
            [Required, MaxLength(100)]
            public string Username { get; set; } = string.Empty;
            [Required, MaxLength(256)]
            public string PasswordHash { get; set; } = string.Empty;
            [MaxLength(200)]
            public string FullName { get; set; } = string.Empty;
            [MaxLength(30)]
            public string? Phone { get; set; }
            [MaxLength(50)]
            public string Role { get; set; } = "admin";
            public bool IsLocked { get; set; } = false;
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
 