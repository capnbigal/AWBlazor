namespace AWBlazorApp.Data;

/// <summary>
/// Wraps <see cref="DatabaseInitializer.InitializeAsync"/> so it runs as part of the host's
/// startup pipeline. Doing this in a hosted service (rather than as a line in <c>Main</c> after
/// <c>app.Build()</c>) means <c>WebApplicationFactory</c>-based integration tests honour it
/// without requiring the test to manually invoke the initializer.
/// </summary>
public sealed class DatabaseInitializerHostedService(IServiceProvider services) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) =>
        DatabaseInitializer.InitializeAsync(services, cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
