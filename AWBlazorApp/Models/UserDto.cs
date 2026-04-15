using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;

namespace AWBlazorApp.Models;

public sealed record UserDto(
    string Id,
    string? UserName,
    string? Email,
    string? FirstName,
    string? LastName,
    string? DisplayName,
    string? ProfileUrl);

public static class UserMappings
{
    public static UserDto ToDto(this ApplicationUser u) => new(
        u.Id, u.UserName, u.Email, u.FirstName, u.LastName, u.DisplayName, u.ProfileUrl);
}
