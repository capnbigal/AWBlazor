# ADR-0001: Blazor Server over WebAssembly

**Status:** Accepted  
**Date:** 2026-04-12  
**Context:** Needed server-side DB access, real-time SignalR, no WASM download penalty. App is internal/low-latency.  
**Decision:** Blazor Server (Interactive Server render mode).  
**Consequences:** +No WASM payload, +direct DB access, +real-time via SignalR. -Requires persistent connection, -server memory per user.
