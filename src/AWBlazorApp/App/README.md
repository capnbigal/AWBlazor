# App

The composition root. `Program.cs` should be thin — a few extension method
calls — and the wiring lives here:

- `Extensions/ServiceCollectionExtensions.cs` — `builder.Services.Add*` chains.
- `Extensions/ApplicationBuilderExtensions.cs` — middleware pipeline.
- `Middleware/` — custom middleware components.
- `Routing/` — `EndpointMappingExtensions` that compose per-feature endpoint
  groups.

Nothing with business logic in here. If a class ends up touching SQL or an
external service, it belongs in `Infrastructure/`; if it's feature-specific
behavior, it belongs in `Features/<Name>/`.
