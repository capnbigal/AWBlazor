## Description

Briefly describe what this PR does and why.

## Type of change

- [ ] Bug fix (non-breaking change that fixes an issue)
- [ ] New feature (non-breaking change that adds functionality)
- [ ] Refactoring (no functional changes)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update
- [ ] Infrastructure / CI / build change

## Related issues

Closes #(issue number)

## Changes made

- 
- 
- 

## Testing

- [ ] All existing tests pass (`dotnet test ElementaryApp.slnx`)
- [ ] New tests added for new functionality
- [ ] Form POST tests added for any new Identity/SSR form pages
- [ ] Tested manually against SQL Server (ELITE/AdventureWorks2022_dev)

## Checklist

- [ ] Code follows the project code style (4 spaces, file-scoped namespaces)
- [ ] `IDbContextFactory` used in Blazor components (not scoped DbContext)
- [ ] `[ExcludeFromInteractiveRouting]` added to any new Identity pages
- [ ] No MudBlazor input components used in static SSR forms
- [ ] `[SupplyParameterFromForm]` properties have `OnInitialized` null-coalescing guard
- [ ] EF migration added if data model changed
- [ ] `MigrationMarkers` updated in `DatabaseInitializer` if migration added
- [ ] No SQLite, ServiceStack, Vue, Tailwind, or NPM dependencies introduced

## Screenshots

If this PR includes UI changes, add before/after screenshots.
