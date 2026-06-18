using AWBlazorApp.Features.Admin;
using AWBlazorApp.Features.Dashboard;
using AWBlazorApp.Features.Engineering;
using AWBlazorApp.Features.Forecasting;
using AWBlazorApp.Features.Identity.Application;
using AWBlazorApp.Features.Identity.Application.Services;
using AWBlazorApp.Features.Identity.Domain;
using AWBlazorApp.Features.Insights;
using AWBlazorApp.Features.Inventory;
using AWBlazorApp.Features.Logistics;
using AWBlazorApp.Features.Maintenance;
using AWBlazorApp.Features.Mes;
using AWBlazorApp.Features.Performance;
using AWBlazorApp.Features.ProcessManagement;
using AWBlazorApp.Features.Processes.Timelines;
using AWBlazorApp.Features.Quality;
using AWBlazorApp.Features.UserGuide;
using AWBlazorApp.Features.Workforce;
using AWBlazorApp.Infrastructure.Authentication;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.OpenApi;
using MudBlazor.Services;
using System.Threading.RateLimiting;

namespace AWBlazorApp.App.Extensions;

/// <summary>
/// Platform-level registration extensions. Feature services are owned by per-feature
/// registration classes and composed through <see cref="AddFeatureServices"/>.
/// </summary>
public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<AuditingInterceptor>();
        services.AddSingleton<AuditLogInterceptor>();

        services.AddDbContextFactory<ApplicationDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(Program).Assembly.GetName().Name);
                sql.UseHierarchyId();
            });
            options.AddInterceptors(
                sp.GetRequiredService<AuditingInterceptor>(),
                sp.GetRequiredService<AuditLogInterceptor>());
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        // Identity (UserStore) and minimal-API endpoints expect a scoped DbContext.
        services.AddScoped<ApplicationDbContext>(sp =>
            sp.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());

        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>("database", tags: ["ready"])
            .AddSqlServer(connectionString, name: "sqlserver", tags: ["ready"]);

        return services;
    }

    public static IServiceCollection AddApplicationIdentity(this IServiceCollection services, IConfiguration configuration)
    {
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

        services.AddAuthentication()
            .AddScheme<AWBlazorApp.Infrastructure.Authentication.ApiKeyAuthenticationOptions, AWBlazorApp.Infrastructure.Authentication.ApiKeyAuthenticationHandler>(
                AWBlazorApp.Infrastructure.Authentication.ApiKeyAuthenticationOptions.Scheme, _ => { });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("ApiOrCookie", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddAuthenticationSchemes(
                    IdentityConstants.ApplicationScheme,
                    AWBlazorApp.Infrastructure.Authentication.ApiKeyAuthenticationOptions.Scheme);
            });
        });

        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo("App_Data"));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        // External authentication providers — configure via user secrets:
        //   dotnet user-secrets set "Authentication:Google:ClientId" "your-id"
        //   dotnet user-secrets set "Authentication:Google:ClientSecret" "your-secret"
        var googleClientId = configuration["Authentication:Google:ClientId"];
        var microsoftClientId = configuration["Authentication:Microsoft:ClientId"];

        if (!string.IsNullOrEmpty(googleClientId))
        {
            services.AddAuthentication().AddGoogle(options =>
            {
                options.ClientId = googleClientId;
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"] ?? "";
            });
        }

        if (!string.IsNullOrEmpty(microsoftClientId))
        {
            services.AddAuthentication().AddMicrosoftAccount(options =>
            {
                options.ClientId = microsoftClientId;
                options.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"] ?? "";
            });
        }

        return services;
    }

    /// <summary>
    /// Composes every feature- and shared-owned service registration. Each feature owns its
    /// own <c>AddXxxServices</c> extension next to the feature; this method is only responsible
    /// for the invocation order.
    /// </summary>
    public static IServiceCollection AddFeatureServices(this IServiceCollection services)
    {
        services.AddSharedServices();

        // Security-event auditing (login success/failure, lockout, password change, API-key/role
        // changes). Stateless + DbContextFactory-backed, so a singleton is safe. Writing "LoginFailed"
        // rows here lights up the previously-dead FailedLoginsLast24h notification metric.
        services.AddSingleton<ISecurityAuditService, SecurityAuditService>();

        services.AddAdminServices();
        services.AddDashboardServices();
        services.AddInventoryServices();
        services.AddLogisticsServices();
        services.AddMesServices();

        // Quality must be registered before Workforce so that the existing
        // IEnumerable<IPostingTriggerHook> order (Inspection, then QualificationCheck) is preserved.
        services.AddQualityServices();
        services.AddWorkforceServices();

        services.AddEngineeringServices();
        services.AddMaintenanceServices();
        services.AddPerformanceServices();
        services.AddInsightsServices();
        services.AddForecastingServices();
        services.AddProcessManagementServices();
        services.AddProcessTimelineServices();
        services.AddUserGuideServices();

        return services;
    }

    public static IServiceCollection AddApplicationRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Global limiter: throttle every /api/* caller to 100 req/min, partitioned by API key
            // (falling back to client IP). This runs before authentication, so the authenticated
            // user name isn't available yet — API key + IP is the right granularity here. Non-/api
            // traffic (the Blazor circuit, static assets, SignalR) is deliberately not limited.
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                if (!context.Request.Path.StartsWithSegments("/api"))
                    return RateLimitPartition.GetNoLimiter("non-api");

                var key = context.Request.Headers["X-Api-Key"].FirstOrDefault()
                          ?? context.Connection.RemoteIpAddress?.ToString()
                          ?? "anonymous";

                return RateLimitPartition.GetFixedWindowLimiter("api:" + key, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                });
            });

            // Retained named policy for any endpoint that wants to opt in explicitly.
            options.AddFixedWindowLimiter("api", limiter =>
            {
                limiter.PermitLimit = 100;
                limiter.Window = TimeSpan.FromMinutes(1);
                limiter.QueueLimit = 0;
            });

            // "auth" (login / 2fa / external-login) is now partitioned by client IP so a single
            // source IP gets its own 5/min budget — previously it was one global bucket shared by
            // every user, which both under-protected brute force and could starve legitimate logins.
            options.AddPolicy("auth", context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter("auth:" + ip, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                });
            });
        });
        return services;
    }

    public static IServiceCollection AddApplicationHsts(this IServiceCollection services)
    {
        // 1-year HSTS. Browsers cache this and refuse plain HTTP for the duration.
        services.AddHsts(options =>
        {
            options.MaxAge = TimeSpan.FromDays(365);
            options.IncludeSubDomains = true;
            options.Preload = true;
        });
        return services;
    }

    public static IServiceCollection AddApplicationCookieHardening(this IServiceCollection services)
    {
        // Tighten Identity application cookie: 60-min sliding expiration with absolute cap.
        services.ConfigureApplicationCookie(options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            options.SlidingExpiration = true;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
        });
        return services;
    }

    /// <summary>
    /// Blazor host + MudBlazor + in-memory cache + OpenAPI. Pure platform wiring; feature
    /// services live in per-feature registration extensions composed by <see cref="AddFeatureServices"/>.
    /// </summary>
    public static IServiceCollection AddBlazorAndServices(this IServiceCollection services)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        services.AddMudServices();
        services.AddMemoryCache();

        // RFC-7807 problem+json infrastructure. Consumed by ApiProblemDetailsMiddleware to turn
        // unhandled /api/* exceptions into structured error responses (DbUpdateException -> 409, etc.).
        services.AddProblemDetails();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "AWBlazorApp API",
                Version = "v1",
                Description = "REST API for the AWBlazorApp Blazor host.",
            });
        });

        return services;
    }
}
