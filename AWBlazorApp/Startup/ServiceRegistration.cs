using AWBlazorApp.Components.Account;
using AWBlazorApp.Data;
using AWBlazorApp.Services;
using AWBlazorApp.Services.Forecasting;
using FluentValidation;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.OpenApi;
using MudBlazor.Services;

namespace AWBlazorApp.Startup;

/// <summary>
/// Extension methods that register services in the DI container.
/// Extracted from Program.cs for readability and testability.
/// </summary>
public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationDatabase(this IServiceCollection services, string connectionString)
    {
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
            .AddScheme<Authentication.ApiKeyAuthenticationOptions, Authentication.ApiKeyAuthenticationHandler>(
                Authentication.ApiKeyAuthenticationOptions.Scheme, _ => { });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("ApiOrCookie", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddAuthenticationSchemes(
                    IdentityConstants.ApplicationScheme,
                    Authentication.ApiKeyAuthenticationOptions.Scheme);
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

    public static IServiceCollection AddHangfireServices(
        this IServiceCollection services, IConfiguration configuration, string connectionString)
    {
        services.Configure<SmtpConfig>(configuration.GetSection("Smtp"));
        services.AddTransient<SmtpEmailJob>();
        services.AddTransient<RequestLogCleanupJob>();
        services.AddTransient<ApiKeyHashMigrationJob>();
        services.AddTransient<ProcessSchedulerJob>();

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
            services.AddHangfireServer(options =>
            {
                options.WorkerCount = Environment.ProcessorCount * 2;
            });

            services.AddSingleton<IEmailSender<ApplicationUser>>(
                !string.IsNullOrWhiteSpace(smtpHost)
                    ? sp => ActivatorUtilities.CreateInstance<HangfireSmtpEmailSender>(sp)
                    : sp => ActivatorUtilities.CreateInstance<IdentityNoOpEmailSender>(sp));
        }
        else
        {
            services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
        }

        return services;
    }

    public static IServiceCollection AddForecastingServices(this IServiceCollection services)
    {
        services.AddScoped<IForecastDataSourceProvider, ForecastDataSourceProvider>();
        services.AddScoped<IForecastComputationService, ForecastComputationService>();
        services.AddScoped<IForecastAlgorithm, SimpleMovingAverageAlgorithm>();
        services.AddScoped<IForecastAlgorithm, WeightedMovingAverageAlgorithm>();
        services.AddScoped<IForecastAlgorithm, ExponentialSmoothingAlgorithm>();
        services.AddScoped<IForecastAlgorithm, LinearRegressionAlgorithm>();
        services.AddTransient<ForecastEvaluationJob>();
        return services;
    }

    public static IServiceCollection AddApplicationRateLimiting(this IServiceCollection services)
    {
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

    public static IServiceCollection AddBlazorAndServices(this IServiceCollection services)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        services.AddMudServices();
        services.AddMemoryCache();
        services.AddSingleton<AnalyticsCacheService>();
        services.AddSingleton<NotificationService>();
        services.AddSingleton<UserGuideService>();
        services.AddSingleton<LookupService>();
        services.AddScoped<IPermissionService, PermissionService>();

        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddTransient(typeof(AWBlazorApp.Validators.MudFormValidator<>));

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
