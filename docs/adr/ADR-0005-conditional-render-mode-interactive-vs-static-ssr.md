# ADR-0005: Conditional render mode — Interactive Server vs Static SSR

**Status:** Accepted  
**Date:** 2026-04-12  
**Context:** Identity pages need static SSR for cookie writes. All other pages need Interactive Server for MudBlazor interactivity.  
**Decision:** App.razor uses AcceptsInteractiveRouting() to choose. Identity pages use [ExcludeFromInteractiveRouting].  
**Consequences:** +Both modes coexist, +Identity cookie auth works, +MudBlazor interactive features work. -Every new Identity page must remember the attribute, -antiforgery tokens can desync (logout endpoint uses DisableAntiforgery).
