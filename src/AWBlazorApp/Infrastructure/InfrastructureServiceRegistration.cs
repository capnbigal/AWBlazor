using AWBlazorApp.Features.Identity.Application.Services;
using AWBlazorApp.Features.Identity.Domain;
using AWBlazorApp.Infrastructure.Email;
using AWBlazorApp.Infrastructure.Jobs;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace AWBlazorApp.Infrastructure;

/// <summary>
/// Registers Infrastructure-owned services: background jobs, Hangfire (and its dependent
/// <see cref="IEmailSender{TUser}"/> wiring), and related cross-cutting pieces.
/// </summary>
public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString)
    {
        services.Configure<SmtpConfig>(configuration.GetSection("Smtp"));
        services.AddTransient<SmtpEmailJob>();
        services.AddTransient<RequestLogCleanupJob>();
        services.AddTransient<AuditLogCleanupJob>();
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
            services.AddHangfireServer(options =>
            {
                options.WorkerCount = Environment.ProcessorCount * 2;
            });

            Func<IServiceProvider, IEmailSender<ApplicationUser>> emailSenderFactory =
                !string.IsNullOrWhiteSpace(smtpHost)
                    ? sp => ActivatorUtilities.CreateInstance<HangfireSmtpEmailSender>(sp)
                    : sp => ActivatorUtilities.CreateInstance<IdentityNoOpEmailSender>(sp);
            services.AddSingleton<IEmailSender<ApplicationUser>>(emailSenderFactory);
        }
        else
        {
            services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
        }

        return services;
    }
}
