# ADR-0006: InputText over MudTextField in SSR forms

**Status:** Accepted  
**Date:** 2026-04-12  
**Context:** MudBlazor input components don't emit HTML name attributes. Static SSR form binding requires name attributes to populate [SupplyParameterFromForm] properties.  
**Decision:** Use Blazor's built-in InputText/InputCheckbox/InputSelect in all [ExcludeFromInteractiveRouting] pages. Style with account-forms.css.  
**Consequences:** +Forms work correctly, +testable via FormPostHelper. -Visual inconsistency between Identity pages and the rest of the app, -developers must know the rule.
