namespace CLHCRM.Application.Interfaces;

/// <summary>
/// Service for accessing current user information
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
}
