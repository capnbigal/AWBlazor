namespace AWBlazorApp.Data;

/// <summary>
/// Configurable areas for data-level permissions. Each area maps to a set of
/// AdventureWorks entities or app feature pages. See <see cref="PermissionAreaMapping"/>
/// for the route-to-area resolution logic.
/// </summary>
public enum PermissionArea
{
    // AdventureWorks department/schema areas
    HumanResources = 1,
    Production = 2,
    Sales = 3,
    Purchasing = 4,
    Person = 5,

    // App feature areas (gap at 100+ for clean separation)
    Forecasts = 100,
    Analytics = 101,
    ToolSlots = 102,
    Admin = 103,
    UserGuide = 104,
    Processes = 105,
    Enterprise = 106,
    Inventory = 107,
    Logistics = 108,
    Mes = 109,
    Quality = 110,
    Workforce = 111,
}
