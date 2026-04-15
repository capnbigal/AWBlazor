using AWBlazorApp.Endpoints.AdventureWorks;
using AWBlazorApp.Endpoints;
using AWBlazorApp.Endpoints.AdventureWorks;
using AWBlazorApp.Endpoints.Admin;
using AWBlazorApp.Endpoints.Admin;

namespace AWBlazorApp.App.Routing;

public static class EndpointMappingExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapHelloEndpoints();
        app.MapAdminEndpoints();
        app.MapForecastEndpoints();
        app.MapProcessEndpoints();
        app.MapToolSlotConfigurationEndpoints();
        app.MapUserEndpoints();

        // AdventureWorks reference-data endpoints — one group per table, each with its own
        // audit history endpoint.
        app.MapAddressTypeEndpoints();
        app.MapContactTypeEndpoints();
        app.MapCountryRegionEndpoints();
        app.MapPhoneNumberTypeEndpoints();
        app.MapCultureEndpoints();
        app.MapProductCategoryEndpoints();
        app.MapScrapReasonEndpoints();
        app.MapUnitMeasureEndpoints();
        app.MapCurrencyEndpoints();
        app.MapSalesReasonEndpoints();
        app.MapDepartmentEndpoints();
        app.MapShiftEndpoints();

        // Batch 2.
        app.MapLocationEndpoints();
        app.MapShipMethodEndpoints();
        app.MapProductSubcategoryEndpoints();
        app.MapProductDescriptionEndpoints();
        app.MapSpecialOfferEndpoints();
        app.MapStateProvinceEndpoints();
        app.MapSalesTerritoryEndpoints();
        app.MapSalesTaxRateEndpoints();
        app.MapShoppingCartItemEndpoints();

        // Batch 3 — business entities + composite-key tables.
        app.MapCustomerEndpoints();
        app.MapSalesPersonEndpoints();
        app.MapWorkOrderEndpoints();
        app.MapBillOfMaterialsEndpoints();
        app.MapCurrencyRateEndpoints();
        app.MapSalesPersonQuotaHistoryEndpoints();
        app.MapSalesOrderHeaderSalesReasonEndpoints();
        app.MapProductCostHistoryEndpoints();
        app.MapCountryRegionCurrencyEndpoints();
        app.MapEmployeeDepartmentHistoryEndpoints();

        // Batch 4 — Person hierarchy (Address, BusinessEntity, Person + dependents).
        app.MapAddressEndpoints();
        app.MapBusinessEntityEndpoints();
        app.MapPersonEndpoints();
        app.MapEmailAddressEndpoints();
        app.MapPersonPhoneEndpoints();
        app.MapBusinessEntityAddressEndpoints();
        app.MapBusinessEntityContactEndpoints();

        // Batch 5 — Production / Product hierarchy.
        app.MapProductEndpoints();
        app.MapProductModelEndpoints();
        app.MapIllustrationEndpoints();
        app.MapProductPhotoEndpoints();
        app.MapProductReviewEndpoints();
        app.MapProductInventoryEndpoints();
        app.MapProductListPriceHistoryEndpoints();
        app.MapProductProductPhotoEndpoints();

        // Batch 6 — Production junction + transaction tables.
        app.MapProductModelIllustrationEndpoints();
        app.MapProductModelProductDescriptionCultureEndpoints();
        app.MapWorkOrderRoutingEndpoints();
        app.MapTransactionHistoryEndpoints();
        app.MapTransactionHistoryArchiveEndpoints();

        // Batch 7 — Sales: CreditCard, PersonCreditCard, SalesOrderHeader/Detail, SalesTerritoryHistory, SpecialOfferProduct, Store.
        app.MapCreditCardEndpoints();
        app.MapPersonCreditCardEndpoints();
        app.MapSalesOrderHeaderEndpoints();
        app.MapSalesOrderDetailEndpoints();
        app.MapSalesTerritoryHistoryEndpoints();
        app.MapSpecialOfferProductEndpoints();
        app.MapStoreEndpoints();

        // Batch 8 — Purchasing + HR entities.
        app.MapVendorEndpoints();
        app.MapProductVendorEndpoints();
        app.MapPurchaseOrderHeaderEndpoints();
        app.MapPurchaseOrderDetailEndpoints();
        app.MapEmployeeEndpoints();
        app.MapEmployeePayHistoryEndpoints();
        app.MapJobCandidateEndpoints();

        // Batch 9 — Production: Document + ProductDocument (hierarchyid PK support).
        app.MapDocumentEndpoints();
        app.MapProductDocumentEndpoints();

        // CSV export endpoint (Admin only).
        app.MapExportEndpoints();

        return app;
    }
}
