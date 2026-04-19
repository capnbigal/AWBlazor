using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;

namespace AWBlazorApp.Shared.Dtos;

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
