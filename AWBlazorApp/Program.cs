using AWBlazorApp.Data;
using AWBlazorApp.Startup;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// --- Serilog ----------------------------------------------------------------------------------
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

// --- Service registration (see Startup/ServiceRegistration.cs) --------------------------------
var services = builder.Services;
var configuration = builder.Configuration;
// Connection pool tuning: if needed, append "Min Pool Size=10;Max Pool Size=200;" to the
// connection string in appsettings.json (environment-specific). The defaults are Min=0, Max=100.
var connectionString = configuration.GetConnectionString("DefaultConnection")
                       ?? "Server=ELITE;Database=AdventureWorks2022;Trusted_Connection=True;TrustServerCertificate=True";

// Ensure App_Data exists for DataProtection key persistence.
Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "App_Data"));

services.AddApplicationDatabase(connectionString);
services.AddApplicationIdentity(configuration);
services.AddHangfireServices(configuration, connectionString);
services.AddForecastingServices();
services.AddApplicationRateLimiting();
services.AddBlazorAndServices();

var app = builder.Build();

// --- Middleware pipeline (see Startup/MiddlewarePipeline.cs) -----------------------------------
app.UseApplicationMiddleware();
app.MapApplicationEndpoints(configuration);

// --- Database initialization ------------------------------------------------------------------
await DatabaseInitializer.InitializeAsync(app.Services);

app.Run();

// Expose the implicit Program class so WebApplicationFactory<Program> can host it from the test project.
public partial class Program;
