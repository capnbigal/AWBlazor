# ServiceStack-Style Patterns for Blazor + ASP.NET Core Minimal APIs

> Note on sources: during authoring, direct web access to `docs.servicestack.net`
> and live search results was unavailable in the research environment. This
> document is written from existing knowledge of ServiceStack's published
> architecture (Architecture Overview, API Design, Auto-Mapping, Validation,
> Request/Response Filters pages) and current ASP.NET Core minimal-API +
> FluentValidation idioms. Treat citations as general rather than line-quoted.

A reference for extracting the useful ideas from ServiceStack's
message-based, DTO-first architecture and translating them to the
idiomatic .NET 10 / Blazor stack already in use in this repo
(`src/AWBlazorApp/Endpoints`, `Models/`, `Validators/`, `Services/`).
This is **not** a proposal to adopt ServiceStack; it is a
cherry-picking guide.

---

## 1. Why look at ServiceStack at all?

ServiceStack's architecture predates minimal APIs by a decade but ended
up converging on many of the same principles:

- **Message-based, DTO-first design.** Each endpoint is defined by a
  Request DTO (the "message") and a Response DTO. The service is a thin
  handler.
- **One service per operation.** No 12-method `UserController` god-class.
- **Cross-cutting concerns as a pipeline** (filters), not inheritance.
- **Validation as a first-class pluggable stage**, not ad-hoc `if`-checks
  in the handler.
- **Auto-mapping** to reduce the entity-DTO boilerplate tax.

The ideas are sound. The *implementation* (a parallel HTTP host, custom
IoC, bespoke serializers, heavy reflection) is what you do not want to
reintroduce — see `memory/project_migration_status.md`.

---

## 2. Core ideas worth porting

### 2.1 Request / Response DTOs are the API contract

In ServiceStack:

```csharp
public class GetProduct : IReturn<GetProductResponse> { public int Id { get; set; } }
public class GetProductResponse { public Product Result { get; set; } public ResponseStatus ResponseStatus { get; set; } }
public class ProductService : Service {
    public object Get(GetProduct req) => new GetProductResponse { Result = Db.SingleById<Product>(req.Id) };
}
```

The `IReturn<T>` marker binds request-to-response on the **type**,
enabling typed clients and documentation without separate metadata.

**Minimal API translation:**

```csharp
// Models/Products/GetProduct.cs
public sealed record GetProductRequest(int Id);
public sealed record GetProductResponse(ProductDto Result);

// Endpoints/ProductEndpoints.cs
group.MapGet("/products/{id:int}", async (int id, IProductService svc, CancellationToken ct) =>
        Results.Ok(new GetProductResponse(await svc.GetAsync(id, ct))))
    .WithName("GetProduct")
    .Produces<GetProductResponse>();
```

You give up `IReturn<T>` but gain OpenAPI / Swagger metadata that
generates the same typed clients.

| ServiceStack pattern | Blazor / .NET 10 equivalent |
|---|---|
| `class GetProduct : IReturn<GetProductResponse>` | `record GetProductRequest(...)` + `.Produces<GetProductResponse>()` on the endpoint |
| `ResponseStatus` envelope on every response | `ProblemDetails` for errors; plain response body for success |
| Attribute-based routing (`[Route]` on DTO) | `MapGet/MapPost` in an `*Endpoints.cs` extension method |
| One DTO class per operation | One `record` per operation (file-scoped pair of Request/Response) |
| `[Authenticate]`, `[RequiredRole]` on DTO | `.RequireAuthorization("PolicyName")` on the endpoint |

### 2.2 Single-responsibility services / handlers

ServiceStack's `Service` base class holds one operation per method, and
best practice is **one class per verb** — e.g. `CreateProductService`,
`UpdateProductService`. The .NET-idiomatic version is either:

- **Inline lambda handler** inside `MapX(...)` when logic is under ~10
  lines and the query is trivial.
- **Injected handler class** (`IProductService.CreateAsync`) when the
  operation has branching, multiple DB reads, or needs to be reused
  from Blazor components.

This repo already has examples of both (`HelloEndpoints.cs` = inline;
`LookupService.cs`, `PermissionService.cs`, `ToolSlotAuditService.cs`
= handler classes). Keep that split.

### 2.3 Validation pipeline (FluentValidation)

ServiceStack popularised the pattern where a `FluentValidation` validator
runs **automatically** before the service, and a failure short-circuits
with a structured `ValidationException` → 400 payload.

**Minimal API translation** (use an endpoint filter so this happens
automatically per-endpoint rather than in each handler):

```csharp
// Endpoints/ValidationExtensions.cs
public static RouteHandlerBuilder WithValidation<T>(this RouteHandlerBuilder b) where T : class
    => b.AddEndpointFilter(async (ctx, next) =>
    {
        var model = ctx.Arguments.OfType<T>().FirstOrDefault();
        if (model is null) return await next(ctx);
        var validator = ctx.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator is null) return await next(ctx);
        var result = await validator.ValidateAsync(model, ctx.HttpContext.RequestAborted);
        if (!result.IsValid)
            return Results.ValidationProblem(result.ToDictionary());
        return await next(ctx);
    });

// Usage
group.MapPost("/products", async (CreateProductRequest req, IProductService s) => ...)
     .WithValidation<CreateProductRequest>();
```

This repo already has `Endpoints/ValidationExtensions.cs` — extend it
rather than re-inventing.

**Rulesets** (ServiceStack `"Create"` vs `"Update"`) translate cleanly:

```csharp
public sealed class ProductRequestValidator : AbstractValidator<ProductRequest> {
    public ProductRequestValidator() {
        RuleSet("Create", () => { RuleFor(x => x.Id).Equal(0); });
        RuleSet("Update", () => { RuleFor(x => x.Id).GreaterThan(0); });
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
    }
}
```

Select the ruleset at the endpoint: `validator.Validate(req, options => options.IncludeRuleSets("Create"))`.

### 2.4 Auto-mapping (without ServiceStack.Text)

ServiceStack's `ConvertTo<T>()` and `PopulateWith()` are attractive
*until* they hide a bug. For a Blazor codebase, pick **one** of:

| Approach | Use when |
|---|---|
| **Explicit static mappers** (`ProductMapper.ToDto(entity)`) | You want compile-time safety, zero reflection, and diff-able mapping logic. Default choice for small/medium apps. |
| **Mapperly** (source generator) | You want AutoMapper-style terseness without runtime cost. Recommended for .NET 10. |
| **AutoMapper** | You already have a large profile library or the team is fluent in it. New projects should prefer Mapperly. |

Do **not** ship runtime-reflection `ConvertTo` clones — they cost
startup memory and hide property-name typos until production.

**Recommended structure:**

```
Models/
  AdventureWorks/
    ProductDtos.cs              // record ProductDto, record CreateProductRequest, ...
    ProductMappings.cs          // static class ProductMappings { ToDto, ToEntity, ApplyUpdate }
```

### 2.5 Response envelopes: `PagedResult<T>`, `ApiResponse<T>`

ServiceStack's `ResponseStatus` pattern (every response has an
`ErrorCode`/`Message`/`Errors[]` block) is *over*-prescriptive for a
mostly-cookie-authed Blazor app. Use the split the platform already
favours:

- **Errors → `ProblemDetails` / `ValidationProblemDetails`** (RFC 7807,
  built into minimal APIs via `Results.Problem` / `Results.ValidationProblem`).
- **Success lists → `PagedResult<T>`** envelope:

```csharp
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount) {
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
```

One canonical `PagedResult<T>` in `Models/Shared/PagedResult.cs` used
by *every* list endpoint. That is the single most useful ServiceStack
idea to adopt wholesale — it standardises pagination across your API
without committing to a universal envelope.

### 2.6 Error contracts

| Concern | ServiceStack | Minimal-API translation |
|---|---|---|
| Validation failure | `ResponseStatus.Errors[]` | `Results.ValidationProblem(dict)` → 400 with `ValidationProblemDetails` |
| Domain error (e.g. "insufficient stock") | `throw new HttpError(400, "StockError", "...")` | `Results.Problem(title, statusCode: 409, type: "https://...")` |
| Not found | `throw HttpError.NotFound(...)` | `Results.NotFound()` or `TypedResults.NotFound<ProblemDetails>(...)` |
| Unauthorized | `[Authenticate]` on DTO | `.RequireAuthorization()` on endpoint |
| Crash | `ResponseStatus` with stack trace (dev only) | `UseExceptionHandler` + `AddProblemDetails()` |

Use a centralised `ExceptionHandling` startup module (`Startup/`
folder already exists) that maps custom exceptions to `ProblemDetails`,
so handlers can just throw.

---

## 3. Cross-cutting concerns

ServiceStack expresses these as Request/Response Filters (global +
typed + attribute). The minimal-API equivalents are:

| ServiceStack concept | .NET 10 equivalent |
|---|---|
| Global Request Filter | Middleware (`app.Use(...)`) |
| Global Response Filter | Middleware with `context.Response.OnStarting(...)` |
| Typed Request Filter `(IRequest, IResponse, TDto)` | Endpoint filter (`.AddEndpointFilter<T>`) |
| `[ValidateRequest]` attribute | `.WithValidation<T>()` extension (repo already has this) |
| `[CacheResponse]` attribute | `.CacheOutput(policy: "ByUser5Min")` (ASP.NET Core OutputCache) |
| `[RequiredRole]` attribute | `.RequireAuthorization(policy)` + `AuthorizationPolicyBuilder` |
| Global exception mapper | `app.UseExceptionHandler()` + `IExceptionHandler` |

**Ordering rule of thumb:** Authentication → Authorization → Rate
limiter → Output cache → Validation → Handler. This matches the default
pipeline order in `Program.cs`; keep endpoint filters for things that
need the bound model (validation) and middleware for things that need
the raw `HttpContext` (headers, rate limits, logging).

---

## 4. Recommended folder / code structure

Aligns with the existing repo and just sharpens the conventions:

```
src/AWBlazorApp/
  Models/                         # DTOs (records) - the API contract
    Shared/
      PagedResult.cs
      ApiError.cs                 # optional thin wrapper over ProblemDetails
    AdventureWorks/
      ProductDtos.cs              # record ProductDto, GetProductRequest, CreateProductRequest, UpdateProductRequest
      ProductMappings.cs          # static ToDto / ToEntity / ApplyUpdate
  Validators/
    AdventureWorks/
      ProductValidators.cs        # one AbstractValidator per Request DTO (or shared with RuleSets)
  Services/                       # reusable domain logic (injected, used by endpoints AND Blazor components)
    LookupService.cs
    PermissionService.cs
    ProductService.cs             # IProductService / ProductService
  Endpoints/                      # minimal-API wiring only; no business logic
    ProductEndpoints.cs           # static MapProductEndpoints(this IEndpointRouteBuilder app)
    ValidationExtensions.cs       # .WithValidation<T>() filter
    EndpointMappingExtensions.cs  # single MapAllEndpoints() call used by Program.cs
  Startup/
    ExceptionHandling.cs          # UseExceptionHandler + ProblemDetails mapping
    AuthorizationPolicies.cs
  Components/                     # Blazor - consumes IProductService directly, NOT the HTTP endpoint
```

Key rule: **Blazor components call services, not endpoints.**
Endpoints are the *external* (API-key) surface. If the Razor page and
the API both need the same logic, the logic lives in a service in
`Services/` and both callers depend on it. This avoids the
HTTP-self-call antipattern.

---

## 5. Decision matrix: service class vs inline endpoint

| Signal | Inline lambda in endpoint | Service class (`IFooService`) |
|---|---|---|
| Single DB query + project to DTO | yes | no |
| Used by both a Blazor page and the API | no | **yes** |
| Multiple DB round-trips or transaction | no | **yes** |
| Requires domain rules beyond validation | no | **yes** |
| Needs unit testing without an HTTP host | no | **yes** |
| Purely CRUD over one entity | yes | only if reused |
| Calls another service or external API | no | **yes** |
| Will grow (you know it will) | no | **yes** |

Default to inline until a second caller appears or the handler crosses
~15 lines — then extract.

---

## 6. Naming conventions

Stick to these across the codebase; they read like a checklist.

| Artifact | Convention | Example |
|---|---|---|
| Request DTO | `<Verb><Entity>Request` | `CreateProductRequest`, `GetProductRequest`, `ListProductsRequest` |
| Response DTO (single) | `<Entity>Dto` | `ProductDto` |
| Response DTO (wrapper) | `<Verb><Entity>Response` *only if* shape differs from `Dto` | `LoginResponse` |
| List / paged response | `PagedResult<TDto>` | `PagedResult<ProductDto>` |
| Endpoint group class | `<Entity>Endpoints` | `ProductEndpoints` |
| Endpoint mapping method | `Map<Entity>Endpoints` | `MapProductEndpoints(this IEndpointRouteBuilder)` |
| Validator | `<RequestDto>Validator` | `CreateProductRequestValidator` |
| Service interface / impl | `I<Entity>Service` / `<Entity>Service` | `IProductService` |
| Service method | `<Verb>Async` | `CreateAsync`, `ListAsync`, `GetAsync`, `UpdateAsync`, `DeleteAsync` |
| Mapper | `<Entity>Mappings` (static) | `ProductMappings.ToDto(entity)` |
| Authorization policy | `PascalCase` constant string in `AuthorizationPolicies` | `"ProductAdmin"` |
| OpenAPI operation id | matches request DTO minus "Request" | `CreateProduct` |

Avoid `Controller`, `Manager`, `Helper`, `Util` — all four are weak-noun
smells that signal fat classes. `Service` is acceptable for domain
logic but **not** for one-liners that could be a static mapper.

---

## 7. Anti-patterns to avoid

1. **Fat service god-class.** `IProductService` with 18 methods including
   `ExportCsv`, `SyncFromLegacy`, `GeneratePdf`. Split by feature, not entity.
2. **Anaemic service pass-through.** `public Task<ProductDto> GetAsync(int id)
   => _db.Products.FindAsync(id).ToDto();` — if there's no added logic and
   the endpoint is the only caller, inline it.
3. **Leaky entity in the API contract.** Never return EF entities directly
   from an endpoint. Always map to a DTO. (Lazy-loading, circular refs,
   and unintended field exposure are real.)
4. **Entity-as-request-DTO.** Accepting `Product` as the POST body means
   clients can set `RowVersion`, `CreatedDate`, `ModifiedBy`. Always use
   a purpose-built `CreateProductRequest`.
5. **Validation in the handler.** If you find yourself writing
   `if (req.Name is null) return BadRequest(...)`, that belongs in a
   FluentValidation rule.
6. **Mapper sprawl.** Defining a mapping inline in three different
   endpoints. Centralise in `<Entity>Mappings`.
7. **HTTP self-call from Blazor.** `_httpClient.GetAsync("/api/products")`
   from a Razor page when both live in the same process. Inject the
   service directly.
8. **Global response envelope everywhere.** Don't force every endpoint to
   return `{ result, error, meta }`. Use `PagedResult<T>` for lists,
   plain DTO for singles, `ProblemDetails` for errors. Three shapes, not one.
9. **Reflection-heavy auto-mappers** with no compile-time check. Prefer
   source-generated (Mapperly) or explicit mappers.
10. **Mixing MudBlazor input components into SSR endpoint-form flows** —
    see `CLAUDE.md` §2. Applies when Razor pages POST to endpoint-shaped
    handlers.

---

## 8. Putting it together: a worked sketch

A full `Product` CRUD feature in the recommended style, condensed:

```csharp
// Models/AdventureWorks/ProductDtos.cs
public sealed record ProductDto(int Id, string Name, decimal ListPrice, string? Color);
public sealed record ListProductsRequest(int Page = 1, int PageSize = 25, string? Search = null);
public sealed record CreateProductRequest(string Name, decimal ListPrice, string? Color);
public sealed record UpdateProductRequest(int Id, string Name, decimal ListPrice, string? Color);

// Models/AdventureWorks/ProductMappings.cs
public static class ProductMappings {
    public static ProductDto ToDto(this Product p) => new(p.ProductID, p.Name, p.ListPrice, p.Color);
    public static Product ToEntity(this CreateProductRequest r) =>
        new() { Name = r.Name, ListPrice = r.ListPrice, Color = r.Color };
    public static void ApplyTo(this UpdateProductRequest r, Product p) {
        p.Name = r.Name; p.ListPrice = r.ListPrice; p.Color = r.Color;
    }
}

// Validators/AdventureWorks/ProductValidators.cs
public sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest> {
    public CreateProductRequestValidator() {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ListPrice).GreaterThanOrEqualTo(0);
    }
}

// Services/ProductService.cs
public interface IProductService {
    Task<PagedResult<ProductDto>> ListAsync(ListProductsRequest req, CancellationToken ct);
    Task<ProductDto?> GetAsync(int id, CancellationToken ct);
    Task<ProductDto> CreateAsync(CreateProductRequest req, CancellationToken ct);
    // ...
}

// Endpoints/ProductEndpoints.cs
public static class ProductEndpoints {
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app) {
        var g = app.MapGroup("/api/products").RequireAuthorization("ApiOrCookie").WithTags("Products");

        g.MapGet("", (ListProductsRequest req, IProductService s, CancellationToken ct)
            => s.ListAsync(req, ct));

        g.MapGet("{id:int}", async (int id, IProductService s, CancellationToken ct)
            => await s.GetAsync(id, ct) is { } p ? Results.Ok(p) : Results.NotFound());

        g.MapPost("", async (CreateProductRequest req, IProductService s, CancellationToken ct)
            => Results.Created($"/api/products", await s.CreateAsync(req, ct)))
          .WithValidation<CreateProductRequest>();

        return app;
    }
}
```

Every bullet in section 2 is visible here and the file count is low.

---

## 9. What NOT to port from ServiceStack

- The **universal `ResponseStatus` envelope**. It adds noise for every
  successful response.
- **`IRequiresRequest`-style service base classes** — .NET's DI + minimal
  API parameter binding covers the same need without inheritance.
- **Custom routing via DTO attributes** (`[Route("/products/{Id}")]` on
  the DTO). Keep routing in the endpoint file; it reads better.
- **`ConvertTo<T>()` reflection mapping.** Use Mapperly or explicit.
- **`IReturn<T>` typed clients.** OpenAPI + NSwag / Kiota give the same
  thing with industry-standard tooling.

---

## 10. Quick checklist for a new feature

- [ ] `Models/<Feature>/<Feature>Dtos.cs` — Request/Response records.
- [ ] `Models/<Feature>/<Feature>Mappings.cs` — static mappers.
- [ ] `Validators/<Feature>/<Feature>Validators.cs` — one `AbstractValidator`
      per Request DTO.
- [ ] `Services/<Feature>Service.cs` (interface + impl) — only if reused or
      non-trivial.
- [ ] `Endpoints/<Feature>Endpoints.cs` — `MapXEndpoints()` extension.
- [ ] `EndpointMappingExtensions.cs` updated to call the new `MapX...`.
- [ ] `.WithValidation<TRequest>()` on every POST / PUT / PATCH.
- [ ] `.RequireAuthorization("ApiOrCookie")` (or stricter policy) on the group.
- [ ] Uses `PagedResult<TDto>` if it returns a list.
- [ ] Never exposes EF entities or accepts them as the request body.
- [ ] Blazor components inject the service, not an `HttpClient` to the
      own endpoint.

---

*Last updated: 2026-04-13. Cross-references: `docs/phase-plan.md`,
`docs/adr/`, `CLAUDE.md` sections 1-7.*
