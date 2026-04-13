# ADR-0003: API key storage — plain text to SHA-256 hash migration

**Status:** Accepted  
**Date:** 2026-04-12  
**Context:** Phase 4 stored API keys as plain text for simplicity. Phase 7 introduced SHA-256 hashing via ApiKeyHasher.  
**Decision:** New keys stored as SHA-256 hashes. Auth handler checks both plain-text and hashed for backwards compatibility. Hash migration job converts remaining plain-text keys.  
**Consequences:** +Keys not recoverable from DB breach, +backwards compatible. -One-time migration job needed for existing keys.
