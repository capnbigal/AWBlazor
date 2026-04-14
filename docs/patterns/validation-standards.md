# Validation Standards

Where validation lives, how it runs, and what it should cover.

## Layers of validation (in order of execution)

| Layer | Tech | Catches |
|---|---|---|
| 1. Client-side (interactive only) | `<DataAnnotationsValidator />` + MudForm + FluentValidation | Type errors, required fields — instant UX feedback |
| 2. Server-side (always) | FluentValidation in endpoint handler | All rules, including those that need DB lookups |
| 3. Database | EF constraints, FK, CHECK | Data integrity — last line of defense |

**Never trust client-side validation alone.** Layer 2 is the security boundary; layer 1 is UX polish.

## Where validators live

`AWBlazorApp/Validators/` and `AWBlazorApp/Validators/AdventureWorks/`

One file per entity, conventionally named `{Entity}Validators.cs`. Contains both `Create{Entity}Validator` and `Update{Entity}Validator` as `sealed` classes inheriting `AbstractValidator<T>`.

## Validator structure

```csharp
public sealed class CreateAddressValidator : AbstractValidator<CreateAddressRequest>
{
    public CreateAddressValidator()
    {
        RuleFor(x => x.AddressLine1)
            .NotEmpty().WithMessage("Address line 1 is required.")
            .MaximumLength(60);

        RuleFor(x => x.City)
            .NotEmpty().MaximumLength(30);

        RuleFor(x => x.PostalCode)
            .NotEmpty().MaximumLength(15)
            .Matches(@"^[A-Za-z0-9\- ]+$").WithMessage("Postal code has invalid characters.");

        RuleFor(x => x.StateProvinceId)
            .GreaterThan(0).WithMessage("State/province is required.");
    }
}

public sealed class UpdateAddressValidator : AbstractValidator<UpdateAddressRequest>
{
    public UpdateAddressValidator()
    {
        // Patch semantics: only validate fields that are actually being patched.
        When(x => x.AddressLine1 is not null, () =>
            RuleFor(x => x.AddressLine1!).NotEmpty().MaximumLength(60));

        When(x => x.City is not null, () =>
            RuleFor(x => x.City!).NotEmpty().MaximumLength(30));
        // ...
    }
}
```

## Standard rule patterns

### Required string with length

```csharp
RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
```

### Optional string with max length

```csharp
RuleFor(x => x.Notes).MaximumLength(500); // null is OK
```

### Numeric range

```csharp
RuleFor(x => x.Quantity).InclusiveBetween(0, 10_000);
RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
```

### Email

```csharp
RuleFor(x => x.Email).NotEmpty().EmailAddress();
```

### Custom regex

```csharp
RuleFor(x => x.Sku).NotEmpty().Matches(@"^[A-Z0-9\-]+$").WithMessage("SKU must be uppercase alphanumeric with dashes.");
```

### Conditional rules

```csharp
RuleFor(x => x.DiscountPercent).NotNull()
    .When(x => x.Status == ProductStatus.OnSale);
```

### "At least one of these required"

```csharp
RuleFor(x => x).Must(x =>
    !string.IsNullOrWhiteSpace(x.Family)
    || !string.IsNullOrWhiteSpace(x.MtCode)
    || !string.IsNullOrWhiteSpace(x.Destination))
    .WithMessage("At least one of Family, MtCode, or Destination must be set.");
```

### DB-dependent validation

If a rule requires a database lookup (e.g. "this email isn't already used"), do it in the endpoint AFTER FluentValidation, not inside the validator. Validators should be DI-free where possible to keep them fast and unit-testable.

```csharp
private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
    CreateUserRequest req, IValidator<CreateUserRequest> v, ApplicationDbContext db, CancellationToken ct)
{
    var validation = await v.ValidateAsync(req, ct);
    if (!validation.IsValid) return TypedResults.ValidationProblem(validation.ToDictionary());

    if (await db.Users.AnyAsync(u => u.Email == req.Email, ct))
        return TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            ["Email"] = ["A user with that email already exists."]
        });

    // ... create ...
}
```

## Wiring up

### Endpoint handler

```csharp
private static async Task<Results<Created<IdResponse>, ValidationProblem>> CreateAsync(
    CreateAddressRequest request,
    IValidator<CreateAddressRequest> validator,
    ApplicationDbContext db,
    ClaimsPrincipal user,
    CancellationToken ct)
{
    var v = await validator.ValidateAsync(request, ct);
    if (!v.IsValid) return TypedResults.ValidationProblem(v.ToDictionary());
    // ...
}
```

`AddValidatorsFromAssemblyContaining<Program>()` in `ServiceRegistration.cs` auto-discovers all validators by interface (`IValidator<T>`).

### MudForm (interactive)

```razor
@inject MudFormValidator<CreateAddressRequest> Validator

<MudForm @ref="form" Model="@request" Validation="@Validator.ValidateField" ValidationDelay="0">
    <MudTextField @bind-Value="request.AddressLine1" Label="Line 1" />
</MudForm>

@code {
    private async Task SaveAsync()
    {
        if (!await Validator.ValidateAllAsync(request))
        {
            Snackbar.Add("Please fix the errors.", Severity.Error);
            return;
        }
        // ...
    }
}
```

`MudFormValidator<T>` is in `Validators/MudFormValidator.cs`. Open generic registration in `ServiceRegistration.cs`:
```csharp
services.AddTransient(typeof(MudFormValidator<>));
```

### Static SSR EditForm

DataAnnotations on the InputModel:

```csharp
private sealed class InputModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";
}
```

Plus FluentValidation if needed (run manually in `OnValidSubmit`).

## Error message style

- Use sentences. Capital first letter, period at end.
- Be specific. ❌ "Invalid input"  ✅ "Email must be in the format name@example.com"
- Don't expose internals. ❌ "FOREIGN KEY constraint violated"  ✅ "Selected state/province does not exist"
- Don't accuse. ❌ "You entered an invalid date"  ✅ "Date must be in the future"

## Anti-patterns

- ❌ Validating in the entity setter (entities are anemic — validation is its own layer)
- ❌ Validating in the endpoint handler instead of the validator (validators are testable in isolation)
- ❌ Skipping server-side validation because the form has client-side validation
- ❌ Returning raw exception messages to clients (wrap in friendly messages)
- ❌ Validators that hit the DB (move DB checks to the endpoint after validation passes)

## Testing validators

```csharp
[Test]
public void CreateAddressValidator_Empty_AddressLine1_Fails()
{
    var validator = new CreateAddressValidator();
    var result = validator.Validate(new CreateAddressRequest { AddressLine1 = "", City = "X", PostalCode = "1" });
    Assert.That(result.IsValid, Is.False);
    Assert.That(result.Errors, Has.Some.Property("PropertyName").EqualTo("AddressLine1"));
}
```

Validators are pure functions of input — easy to test, no setup needed.
