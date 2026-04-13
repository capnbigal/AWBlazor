using ElementaryApp.Components;
using ElementaryApp.Components.Account;
using ElementaryApp.Data;
using ElementaryApp.Endpoints;
using ElementaryApp.Services;
using FluentValidation;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.OpenApi;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using MudBlazor.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Replace default logging with Serilog. The MSSqlServer sink is gated on RequestLogs:Enabled
// so tests (which run against the dev SQL Server) can disable it.
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .Enrich.WithClientIp()
        .WriteTo.Console();

    var requestLogsEnabled = context.Configuration.GetValue("RequestLogs:Enabled", defaultValue: true);
    var connStr = context.Configuration.GetConnectionString("DefaultConnection");
    if (requestLogsEnabled && !string.IsNullOrWhiteSpace(connStr))
    {
        var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
        columnOptions.Store.Remove(Serilog.Sinks.MSSqlServer.StandardColumn.Properties);
        columnOptions.Store.Add(Serilog.Sinks.MSSqlServer.StandardColumn.LogEvent);
        columnOptions.AdditionalColumns = new List<Serilog.Sinks.MSSqlServer.SqlColumn>
        {
            new() { ColumnName = "UserName", DataType = System.Data.SqlDbType.NVarChar, DataLength = 256, AllowNull = true },
            new() { ColumnName = "RequestPath", DataType = System.Data.SqlDbType.NVarChar, DataLength = 512, AllowNull = true },
            new() { ColumnName = "RequestMethod", DataType = System.Data.SqlDbType.NVarChar, DataLength = 10, AllowNull = true },
            new() { ColumnName = "RemoteIp", DataType = System.Data.SqlDbType.NVarChar, DataLength = 50, AllowNull = true },
            new() { ColumnName = "MachineName", DataType = System.Data.SqlDbType.NVarChar, DataLength = 100, AllowNull = true },
        };

        configuration.WriteTo.MSSqlServer(
            connectionString: connStr,
            sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions
            {
                TableName = "RequestLogs",
                SchemaName = "dbo",
                AutoCreateSqlDatabase = false,
                AutoCreateSqlTable = true,
                BatchPostingLimit = 50,
                BatchPeriod = TimeSpan.FromSeconds(5),
            },
            columnOptions: columnOptions,
            restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information);
    }
});

var services = builder.Services;
var configuration = builder.Configuration;

// --- Database ---------------------------------------------------------------------------------
// App_Data is still used for ASP.NET DataProtection key persistence.
var appDataPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(appDataPath);

// Production points at SQL Server ELITE / AdventureWorks2022. Integration tests override
// this DbContext registration via ConfigureTestServices to use a SQLite in-memory database.
var connectionString = configuration.GetConnectionString("DefaultConnection")
                       ?? "Server=ELITE;Database=AdventureWorks2022;Trusted_Connection=True;TrustServerCertificate=True";

services.AddHttpContextAccessor();
services.AddSingleton<AuditingInterceptor>();

services.AddDbContextFactory<ApplicationDbContext>((sp, options) =>
{
    options.UseSqlServer(connectionString, sql =>
    {
        sql.MigrationsAssembly(typeof(Program).Assembly.GetName().Name);
        sql.UseHierarchyId();
    });
    options.AddInterceptors(sp.GetRequiredService<AuditingInterceptor>());
    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
});

// AddDbContextFactory does not register a scoped DbContext, but Identity (UserStore) and the
// existing minimal-API endpoints expect one — register a scoped resolver from the factory.
services.AddScoped<ApplicationDbContext>(sp =>
    sp.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());

services.AddDatabaseDeveloperPageExceptionFilter();

// --- Health checks ---------------------------------------------------------------------------
services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database", tags: ["ready"])
    .AddSqlServer(connectionString, name: "sqlserver", tags: ["ready"]);

// --- Identity / Auth --------------------------------------------------------------------------
services.AddCascadingAuthenticationState();
services.AddScoped<IdentityUserAccessor>();
services.AddScoped<IdentityRedirectManager>();
services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

// Add the API-key scheme alongside the Identity cookies.
services.AddAuthentication()
    .AddScheme<ElementaryApp.Authentication.ApiKeyAuthenticationOptions, ElementaryApp.Authentication.ApiKeyAuthenticationHandler>(
        ElementaryApp.Authentication.ApiKeyAuthenticationOptions.Scheme, _ => { });

services.AddAuthorization(options =>
{
    options.AddPolicy("ApiOrCookie", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddAuthenticationSchemes(
            IdentityConstants.ApplicationScheme,
            ElementaryApp.Authentication.ApiKeyAuthenticationOptions.Scheme);
    });
});

services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("App_Data"));

services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// --- SMTP / email + Hangfire ------------------------------------------------------------------
services.Configure<SmtpConfig>(configuration.GetSection("Smtp"));
services.AddTransient<SmtpEmailJob>();
services.AddTransient<RequestLogCleanupJob>();
services.AddTransient<ApiKeyHashMigrationJob>();

var hangfireEnabled = configuration.GetValue("Features:Hangfire", defaultValue: true);
var smtpHost = configuration["Smtp:Host"];

if (hangfireEnabled)
{
    services.AddHangfire(cfg => cfg
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true,
            SchemaName = "HangFire",
        }));
    services.AddHangfireServer();

    if (!string.IsNullOrWhiteSpace(smtpHost))
    {
        services.AddSingleton<IEmailSender<ApplicationUser>, HangfireSmtpEmailSender>();
    }
    else
    {
        services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
    }
}
else
{
    // Test / dev path: skip Hangfire entirely. Use the no-op email sender so registration
    // and forgot-password flows still complete (the email body is logged but not delivered).
    services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
}

// --- Caching ----------------------------------------------------------------------------------
services.AddMemoryCache();
services.AddSingleton<AnalyticsCacheService>();
services.AddSingleton<NotificationService>();

// --- Forecasting services ----------------------------------------------------------------
services.AddScoped<ElementaryApp.Services.Forecasting.IForecastDataSourceProvider, ElementaryApp.Services.Forecasting.ForecastDataSourceProvider>();
services.AddScoped<ElementaryApp.Services.Forecasting.IForecastComputationService, ElementaryApp.Services.Forecasting.ForecastComputationService>();
services.AddScoped<ElementaryApp.Services.Forecasting.IForecastAlgorithm, ElementaryApp.Services.Forecasting.SimpleMovingAverageAlgorithm>();
services.AddScoped<ElementaryApp.Services.Forecasting.IForecastAlgorithm, ElementaryApp.Services.Forecasting.WeightedMovingAverageAlgorithm>();
services.AddScoped<ElementaryApp.Services.Forecasting.IForecastAlgorithm, ElementaryApp.Services.Forecasting.ExponentialSmoothingAlgorithm>();
services.AddScoped<ElementaryApp.Services.Forecasting.IForecastAlgorithm, ElementaryApp.Services.Forecasting.LinearRegressionAlgorithm>();
services.AddTransient<ElementaryApp.Services.Forecasting.ForecastEvaluationJob>();

// --- Rate limiting -----------------------------------------------------------------------
services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("api", limiter =>
    {
        limiter.PermitLimit = 100;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("auth", limiter =>
    {
        limiter.PermitLimit = 5;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
});

// --- Blazor + MudBlazor -----------------------------------------------------------------------
services.AddRazorComponents()
    .AddInteractiveServerComponents();

services.AddMudServices();

// User guide (loads _posts/*.md at startup, DB-backed read tracking).
services.AddSingleton<UserGuideService>();

// --- API: validators + Swagger ----------------------------------------------------------------
services.AddValidatorsFromAssemblyContaining<Program>();
services.AddTransient(typeof(ElementaryApp.Validators.MudFormValidator<>));

services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ElementaryApp API",
        Version = "v1",
        Description = "REST API for the ElementaryApp Blazor host (post-ServiceStack migration).",
    });
});

var app = builder.Build();

// --- HTTP pipeline ----------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// --- Security headers ------------------------------------------------------------------------
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; connect-src 'self' ws: wss:; font-src 'self' data:;";
    await next();
});

app.UseRateLimiter();
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("UserName", httpContext.User.Identity?.Name ?? "anonymous");
        diagnosticContext.Set("UserId", httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "");
        diagnosticContext.Set("RemoteIp", httpContext.Connection.RemoteIpAddress?.ToString() ?? "");
        diagnosticContext.Set("RequestPath", httpContext.Request.Path.Value ?? "");
        diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
    };
});

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

// Swagger / OpenAPI UI is now always mounted (was Development-only before). In non-Development
// environments a branch middleware checks the Admin role before serving anything under /swagger,
// mirroring the Hangfire dashboard's auth model. The swagger JSON and the Swagger-UI HTML both
// live under /swagger, so a single prefix-match predicate guards them together.
if (!app.Environment.IsDevelopment())
{
    app.UseWhen(
        ctx => ctx.Request.Path.StartsWithSegments("/swagger"),
        branch => branch.Use(async (ctx, next) =>
        {
            if (ctx.User.Identity?.IsAuthenticated != true || !ctx.User.IsInRole(AppRoles.Admin))
            {
                var returnUrl = Uri.EscapeDataString(ctx.Request.Path + ctx.Request.QueryString);
                ctx.Response.Redirect($"/Account/Login?ReturnUrl={returnUrl}");
                return;
            }
            await next();
        }));
}
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ElementaryApp API v1");
    c.RoutePrefix = "swagger";
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

// Dark mode toggle — sets a cookie and redirects back. No JS needed.
app.MapGet("/api/toggle-dark-mode", (HttpContext ctx) =>
{
    var current = ctx.Request.Cookies["darkMode"] == "true";
    var newValue = !current;
    ctx.Response.Cookies.Append("darkMode", newValue.ToString().ToLowerInvariant(), new CookieOptions
    {
        HttpOnly = false,
        SameSite = SameSiteMode.Strict,
        MaxAge = TimeSpan.FromDays(365),
        IsEssential = true,
    });
    var returnUrl = ctx.Request.Query["returnUrl"].FirstOrDefault() ?? "/";
    return Results.LocalRedirect($"~{returnUrl}");
});
app.MapApiEndpoints();
app.MapChartExportEndpoints();
app.MapHub<ElementaryApp.Hubs.NotificationHub>(ElementaryApp.Hubs.NotificationHub.HubUrl);

// Health check endpoints — /healthz for liveness (anonymous), /healthz/ready for readiness (Admin-only).
app.MapHealthChecks("/healthz", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false, // liveness: just "is the process running?"
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"status\":\"Healthy\"}");
    }
});

app.MapHealthChecks("/healthz/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds,
                description = e.Value.Description,
                exception = e.Value.Exception?.Message,
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds,
        });
        await context.Response.WriteAsync(result);
    }
}).RequireAuthorization(p => p.RequireRole(AppRoles.Admin));

// Hangfire dashboard — Admin-only via the custom authorization filter. Only mapped when
// the Hangfire feature flag is enabled (it's disabled in tests).
if (hangfireEnabled)
{
    app.MapHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [new ElementaryApp.Authentication.HangfireDashboardAuthFilter()],
    });

    RecurringJob.AddOrUpdate<RequestLogCleanupJob>(
        "request-log-cleanup",
        job => job.ExecuteAsync(),
        Cron.Daily(3, 0)); // Run at 3 AM UTC daily

    RecurringJob.AddOrUpdate<ElementaryApp.Services.Forecasting.ForecastEvaluationJob>(
        "forecast-evaluation",
        job => job.ExecuteAsync(),
        Cron.Daily(4, 0)); // Run at 4 AM UTC daily, after cleanup at 3 AM

    // One-time job to hash any remaining plain-text API keys. Safe to re-run.
    BackgroundJob.Enqueue<ApiKeyHashMigrationJob>(job => job.ExecuteAsync());
}

// Database migration + seed runs synchronously on host start, before the pipeline begins
// processing requests. (Tests use SQLite + InitializeAsync detects the non-SQL-Server provider
// and falls back to EnsureCreated.)
await DatabaseInitializer.InitializeAsync(app.Services);

app.Run();

// Expose the implicit Program class so WebApplicationFactory<Program> can host it from the test project.
public partial class Program;
