# TenexERP website: Account + company onboarding update

This is a **file overlay** for branch `feature/company-auth`.

## Install

Extract this ZIP into the **repository root** (the folder containing `NexusERP/`), allowing the files under `NexusERP/nexus-erp-client/src/` to replace their existing versions.

Or manually copy the `src` files to matching locations in `NexusERP/nexus-erp-client/src/`.

Then, from `NexusERP/nexus-erp-client`:

```powershell
npm install
npm run build
npm run dev
```

Make sure `VITE_API_URL` points to the backend's **`/api` base URL**, e.g. `https://localhost:7001/api` for local development. The API must be running with the local database migrated.

## Included features

- Email/password Account signup, 6-digit verification, resend, and login.
- 30-day Account refresh token rotation; automatic renewal of short-lived Account JWTs.
- Company list, create-company form, employee join form, and join request statuses.
- Company selection issuing company-scoped ERP JWT.
- Existing ERP screens/permissions preserved; ERP JWT auto-renewed by selecting the current company again when needed.
- Admin company-access page: pending requests, role selection, approve/reject, join-code rotation.
- Sign out from Account and switch-company navigation.
- Consistent server-provided error and success messages for onboarding.

## Backend expected

The existing backend must include `/api/account-auth` (register, verify-email, resend-code, login, refresh, logout), `/api/companies` (mine, create, select), join request endpoints and `/api/companies/join-code/rotate` from the current development work.

The pending join request and rotate code endpoints may exist **locally but not yet pushed** to the remote branch. Apply this overlay only after you have those backend changes.

## Important before production

Account refresh tokens are temporarily held in **sessionStorage** to match the current JSON-body API. This is suitable for local integration but exposes refresh tokens to malicious JavaScript if an XSS bug exists. Before production, move refresh token management to Secure, HttpOnly, SameSite cookies, implement CSRF protection if needed, and audit legacy employee login/POS PIN security. The API is also currently set up for local development. Do not publish to production without reviewing and safely applying database migrations.

## Notes

This ZIP contains full copies of changed/new frontend files, **not** the whole React application. I checked the TypeScript/TSX source files for parse errors, but did **not** run the real Vite/TypeScript project build here (npm project dependencies aren't available in this environment). Run `npm run build` in your project and share any error output.
