# Immich API Key Bootstrap

## Problem

Running against a real Immich instance (`USE_STUB_IMMICH=false`) currently requires an
operator to manually create an Immich admin account, log into the Immich UI, and hand-craft
an API key, then paste it into the `IMMICH_API_KEY` secret. There's no automated way to get a
working key for a fresh Immich database (e.g. a freshly provisioned self-hosted instance or an
ephemeral E2E instance).

## Approach

Automate the bootstrap via Immich's own REST API instead of seeding a key directly into its
database (fragile — would have to match Immich's internal key hashing and breaks on upgrades):

1. Wait for Immich to respond on `GET /server/ping`.
2. `POST /auth/admin-sign-up` to create the admin account. Treat `400 Bad Request` as
   "admin already exists" (Immich's own behavior) and continue — sign-up is one-time per DB.
3. `POST /auth/login` with the same admin credentials to get an `accessToken`.
4. `POST /api-keys` (Bearer-authenticated with the access token) to create an API key with
   `all` permissions.
5. Cache the returned secret to a local file so restarts don't spam Immich with new keys.

## Implementation

- `Infrastructure/Immich/ImmichBootstrapper.cs` (new): plain `HttpClient` implementation of
  the flow above (no dependency on the generated Kiota client — keeps this isolated and easy
  to reason about). Idempotent: reuses the cached key file if present, tolerates a
  pre-existing admin account.
- `Program.cs`: when not using the stub and no `IMMICH_API_KEY` is supplied, run the
  bootstrapper once at startup (same place `DbSeeder` runs) and populate the env var that
  `AddImmichClient` already reads.
- `appsettings.json`: add non-secret bootstrap defaults (`Immich:Bootstrap:AdminEmail`,
  `AdminName`, `ApiKeyName`, `ApiKeyCacheFile`). The bootstrap admin password comes from
  `IMMICH_ADMIN_PASSWORD` only — never checked in.
- `VideoManager.AppHost/Program.cs`: stop demanding an `immich-api-key` secret parameter up
  front; pass through `IMMICH_API_KEY`/`IMMICH_ADMIN_PASSWORD` if configured, otherwise leave
  empty so the backend bootstraps itself.
- `StubImmichService` is unchanged — it's still the fast, no-Immich-required dev/test mode via
  `USE_STUB_IMMICH=true`. This work only replaces how the *real* mode obtains its API key.
