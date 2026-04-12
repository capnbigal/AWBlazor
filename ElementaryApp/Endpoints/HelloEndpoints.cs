using ElementaryApp.Models;

namespace ElementaryApp.Endpoints;

public static class HelloEndpoints
{
    public static IEndpointRouteBuilder MapHelloEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/hello").WithTags("Hello");

        group.MapGet("/{name}", (string name) =>
                Results.Ok(new HelloResponse($"Hello, {name}!")))
            .WithName("Hello")
            .WithSummary("Returns a greeting for the supplied name.")
            .Produces<HelloResponse>();

        return app;
    }
}
