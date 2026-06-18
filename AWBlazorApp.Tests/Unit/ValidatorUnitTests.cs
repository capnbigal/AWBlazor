using AWBlazorApp.Features.Production.WorkOrders.Application.Validators;
using AWBlazorApp.Features.Production.WorkOrders.Dtos;
using AWBlazorApp.Features.Purchasing.PurchaseOrderHeaders.Application.Validators;
using AWBlazorApp.Features.Purchasing.PurchaseOrderHeaders.Dtos;
using AWBlazorApp.Features.Sales.Customers.Application.Validators;
using AWBlazorApp.Features.Sales.Customers.Dtos;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace AWBlazorApp.Tests.Unit;

/// <summary>
/// Pure FluentValidation unit tests — no SQL Server, no <c>WebApplicationFactory</c>. They run in
/// milliseconds and form the fast <c>[Category("Unit")]</c> tier that can gate every push, in
/// contrast to the integration suite which needs ELITE / AdventureWorks2022_dev. Run just this tier
/// with <c>dotnet test --filter "Category=Unit"</c>.
/// </summary>
[TestFixture]
[Category("Unit")]
public class ValidatorUnitTests
{
    // ---- CreateCustomerValidator: a customer is a Person XOR a Store ----

    [Test]
    public void Customer_person_only_is_valid() =>
        new CreateCustomerValidator()
            .TestValidate(new CreateCustomerRequest { PersonId = 5 })
            .ShouldNotHaveAnyValidationErrors();

    [Test]
    public void Customer_store_only_is_valid() =>
        new CreateCustomerValidator()
            .TestValidate(new CreateCustomerRequest { StoreId = 7 })
            .ShouldNotHaveAnyValidationErrors();

    [Test]
    public void Customer_both_person_and_store_fails()
    {
        var result = new CreateCustomerValidator().TestValidate(new CreateCustomerRequest { PersonId = 5, StoreId = 7 });
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Customer_neither_person_nor_store_fails()
    {
        var result = new CreateCustomerValidator().TestValidate(new CreateCustomerRequest());
        Assert.That(result.IsValid, Is.False);
    }

    // ---- CreateWorkOrderValidator: positivity + date ordering ----

    [Test]
    public void WorkOrder_valid_when_due_after_start()
    {
        var start = new DateTime(2026, 1, 1);
        new CreateWorkOrderValidator()
            .TestValidate(new CreateWorkOrderRequest { ProductId = 1, OrderQty = 10, StartDate = start, DueDate = start.AddDays(7) })
            .ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void WorkOrder_due_before_start_fails()
    {
        var start = new DateTime(2026, 1, 10);
        new CreateWorkOrderValidator()
            .TestValidate(new CreateWorkOrderRequest { ProductId = 1, OrderQty = 10, StartDate = start, DueDate = start.AddDays(-3) })
            .ShouldHaveValidationErrorFor(x => x.DueDate);
    }

    [Test]
    public void WorkOrder_zero_product_and_qty_fail()
    {
        var start = new DateTime(2026, 1, 1);
        var result = new CreateWorkOrderValidator()
            .TestValidate(new CreateWorkOrderRequest { ProductId = 0, OrderQty = 0, StartDate = start, DueDate = start });
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
        result.ShouldHaveValidationErrorFor(x => x.OrderQty);
    }

    [Test]
    public void WorkOrder_end_before_start_fails()
    {
        var start = new DateTime(2026, 1, 10);
        var result = new CreateWorkOrderValidator()
            .TestValidate(new CreateWorkOrderRequest { ProductId = 1, OrderQty = 10, StartDate = start, DueDate = start.AddDays(5), EndDate = start.AddDays(-1) });
        Assert.That(result.IsValid, Is.False);
    }

    // ---- CreatePurchaseOrderHeaderValidator: status range + positivity ----

    [Test]
    public void PurchaseOrder_valid_minimal() =>
        new CreatePurchaseOrderHeaderValidator()
            .TestValidate(new CreatePurchaseOrderHeaderRequest
            {
                Status = 1, EmployeeId = 1, VendorId = 1, ShipMethodId = 1,
                OrderDate = new DateTime(2026, 1, 1), SubTotal = 10, TaxAmt = 1, Freight = 1,
            })
            .ShouldNotHaveAnyValidationErrors();

    [Test]
    public void PurchaseOrder_status_out_of_range_fails() =>
        new CreatePurchaseOrderHeaderValidator()
            .TestValidate(new CreatePurchaseOrderHeaderRequest
            {
                Status = 9, EmployeeId = 1, VendorId = 1, ShipMethodId = 1, OrderDate = new DateTime(2026, 1, 1),
            })
            .ShouldHaveValidationErrorFor(x => x.Status);

    [Test]
    public void PurchaseOrder_zero_fks_and_negative_amount_fail()
    {
        var result = new CreatePurchaseOrderHeaderValidator()
            .TestValidate(new CreatePurchaseOrderHeaderRequest
            {
                Status = 1, EmployeeId = 0, VendorId = 0, ShipMethodId = 0,
                OrderDate = new DateTime(2026, 1, 1), SubTotal = -1,
            });
        result.ShouldHaveValidationErrorFor(x => x.EmployeeId);
        result.ShouldHaveValidationErrorFor(x => x.VendorId);
        result.ShouldHaveValidationErrorFor(x => x.ShipMethodId);
        result.ShouldHaveValidationErrorFor(x => x.SubTotal);
    }
}
