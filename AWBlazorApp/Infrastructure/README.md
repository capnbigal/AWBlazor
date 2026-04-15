# Infrastructure

Everything that talks to the outside world or persists state.

- `Persistence/` — `ApplicationDbContext`, `DatabaseInitializer`,
  `AuditingInterceptor`, `Migrations/`, EF `IEntityTypeConfiguration<T>`
  files grouped by domain in `Configurations/<Domain>/`.
- `Authentication/` — custom `ApiKeyAuthenticationHandler`, `ApiKeyHasher`.
- `Email/` — SMTP config and delivery jobs.
- `Hangfire/` — dashboard auth filter + server wiring.
- `SignalR/` — `NotificationHub` and friends.
- `Logging/` — Serilog enrichers / sink configuration that's not done
  inline in Program.cs.

When the day comes to extract an `AWBlazorApp.Infrastructure` project, this
folder lifts out as-is.
