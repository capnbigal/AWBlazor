using AWBlazorApp.Shared.Domain;
using AWBlazorApp.Features.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace AWBlazorApp.Features.Identity.Domain;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
    public string? ProfileUrl { get; set; }

    public ICollection<UserAreaPermission> AreaPermissions { get; set; } = [];
}
