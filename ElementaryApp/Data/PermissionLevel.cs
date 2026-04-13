namespace ElementaryApp.Data;

/// <summary>
/// Permission level for a user on a given <see cref="PermissionArea"/>.
/// Higher numeric value = more access. Effective permission = Max(roleDefault, explicitGrant).
/// </summary>
public enum PermissionLevel
{
    None = 0,
    Read = 1,
    Write = 2,
    Admin = 3,
}
