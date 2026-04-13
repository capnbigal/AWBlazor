# ADR-0002: MudBlazor as the UI component library

**Status:** Accepted  
**Date:** 2026-04-12  
**Context:** Needed a mature Blazor component library with data grids, dialogs, charts, theming.  
**Decision:** MudBlazor 9. Locked-in — do not replace.  
**Consequences:** +Rich component set, +Material Design consistency, +active community. -Inline styles prevent strict CSP, -SSR form inputs don't emit name attributes (documented gotcha).
