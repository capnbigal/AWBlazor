using ElementaryApp.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace ElementaryApp.Data;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
    public string? ProfileUrl { get; set; }

    public ICollection<UserAreaPermission> AreaPermissions { get; set; } = [];
}
