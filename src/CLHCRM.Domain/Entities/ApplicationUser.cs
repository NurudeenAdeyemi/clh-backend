using Microsoft.AspNetCore.Identity;

namespace CLHCRM.Domain.Entities;

/// <summary>
/// Represents a user in the application, extending IdentityUser for authentication and authorization.
/// </summary>
public class ApplicationUser : IdentityUser
{
    // Add any additional profile properties here
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
