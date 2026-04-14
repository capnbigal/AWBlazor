using AWBlazorApp.Components;
using AWBlazorApp.Data;
using AWBlazorApp.Endpoints;
using AWBlazorApp.Services;
using Hangfire;
using Serilog;

namespace AWBlazorApp.Startup;

/// <summary>
/// Extension methods that configure the HTTP middleware pipeline and map endpoints.
/// Extracted from Program.cs for readability.
/// </summary>
public static class MiddlewarePipeline
{
    public static WebApplication UseApplicationMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            // HSTS: 1-year max-age + includeSubDomains. Tells browsers to only ever connect
            // via HTTPS, mitigating downgrade attacks. Default .NET value is only 30 days.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseMiddleware<SecurityHeadersMiddleware>();
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
        app.UseMiddleware<AreaPermissionMiddleware>();

        // Swagger — Admin-gated in non-Development environments.
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
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "AWBlazorApp API v1");
            c.RoutePrefix = "swagger";
        });

        return app;
    }

    public static WebApplication MapApplicationEndpoints(this WebApplication app, IConfiguration configuration)
    {
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.MapAdditionalIdentityEndpoints();
        app.MapApiEndpoints();
        app.MapChartExportEndpoints();
        app.MapPreferencesEndpoints();
        app.MapPermissionEndpoints();
        app.MapHub<AWBlazorApp.Hubs.NotificationHub>(AWBlazorApp.Hubs.NotificationHub.HubUrl);

        // Health checks — /healthz (anonymous liveness), /healthz/ready (Admin readiness).
        app.MapHealthChecks("/healthz", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = async (context, _) =>
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

        // Hangfire dashboard + recurring jobs — only when feature flag is enabled.
        var hangfireEnabled = configuration.GetValue("Features:Hangfire", defaultValue: true);
        if (hangfireEnabled)
        {
            app.MapHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = [new AWBlazorApp.Authentication.HangfireDashboardAuthFilter()],
            });

            RecurringJob.AddOrUpdate<RequestLogCleanupJob>(
                "request-log-cleanup",
                job => job.ExecuteAsync(),
                Cron.Daily(3, 0));

            RecurringJob.AddOrUpdate<AWBlazorApp.Services.Forecasting.ForecastEvaluationJob>(
                "forecast-evaluation",
                job => job.ExecuteAsync(),
                Cron.Daily(4, 0));

            RecurringJob.AddOrUpdate<ProcessSchedulerJob>(
                "process-scheduler",
                job => job.ExecuteAsync(),
                "*/15 * * * *");

            BackgroundJob.Enqueue<ApiKeyHashMigrationJob>(job => job.ExecuteAsync());
        }

        return app;
    }
}
