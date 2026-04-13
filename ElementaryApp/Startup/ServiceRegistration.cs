using ElementaryApp.Components.Account;
using ElementaryApp.Data;
using ElementaryApp.Services;
using ElementaryApp.Services.Forecasting;
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

namespace ElementaryApp.Startup;

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

    public static IServiceCollection AddApplicationIdentity(this IServiceCollection services)
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
        services.AddTransient(typeof(ElementaryApp.Validators.MudFormValidator<>));

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ElementaryApp API",
                Version = "v1",
                Description = "REST API for the ElementaryApp Blazor host.",
            });
        });

        return services;
    }
}
