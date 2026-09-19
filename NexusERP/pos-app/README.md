# POS App (Expo / React Native)

A phone/tablet/iPad point-of-sale app for NexusERP: login, sidebar navigation,
a configurable fixed-grid sales screen (image or shape display), an
always-visible cart on wide screens, checkout with cash/card + change
calculation, current-shift receipts, and shift management (open/close, cash
in/out, shift history).

## 1. Apply the backend patches first

This app depends on 5 small additive endpoints that don't exist in the
backend yet. They're in `../backend-patches/` (sibling folder to this one),
numbered in the order to apply them:

1. `1_PosDtos_ADDITIONS.cs` — new DTOs, paste into `PosDtos.cs`
2. `2_IPosService_ADDITIONS.cs` — new interface methods
3. `3_PosService_ADDITIONS.cs` — implementations (needs `using NexusERP.Application.DTOs;`)
4. `4_PosController_ADDITIONS.cs` — new endpoints on `PosController`
5. `5_PinLogin_ADDITIONS.cs` — user-switch PIN login (touches `IUserRepository`,
   `UserRepository`, `IAuthService`, `AuthService`, `AuthController`, and adds
   one small endpoint to `PosController`)

None of these change or remove any existing code — they're pure additions.
Build and run your API, then confirm `POST /api/auth/pin-login` and
`GET /api/pos/shift/current?storeId=1` respond before moving on.

## 2. Install dependencies

```bash
cd pos-app
npm install
```

## 3. Point the app at your backend

Open `src/api/client.ts` and set `API_BASE_URL` to your API's real address.
`localhost` will **not** work from a physical phone/tablet/emulator — use
your computer's LAN IP (e.g. `http://192.168.1.50:7001/api`) or a deployed
HTTPS URL. If your API uses a self-signed dev certificate, you'll also need
to trust it on the device or temporarily point at HTTP during development.

## 4. Run it

```bash
npx expo start
```

Scan the QR code with Expo Go (fastest way to test on a real phone/tablet),
or press `i` / `a` for a simulator, or `w` for a web preview.

## What's included

- **Login** — plain username/password, no registration (matches your spec)
- **Sidebar** — current user card + "Switch User" (PIN pad, no full re-login
  needed), permanently visible on tablet/desktop width, slide-out drawer on
  phone
- **Sales** — fixed-size grid (12 boxes on wide screens, 9 on phone —
  intentionally fixed per device class so the layout never reflows), tap
  "Edit Layout" to assign/clear a product per box via search, tap a box in
  normal mode to add to cart. Cart sits permanently beside the grid on wide
  screens; on phone it's a tab (with a badge showing item count) since there
  isn't room for both side by side
- **Cart** — tap an item to edit quantity/discount, double-tap to +1 quantity,
  swipe left to remove, one button for a whole-receipt discount
- **Checkout** — full receipt preview, Cash/Card toggle (Cash default),
  amount-tendered input with live change calculation, blocked until tendered
  ≥ total for cash sales
- **Receipts** — current shift's receipts, tap through to a clean full-detail
  view
- **Shift** — open/close shift, Cash In / Cash Out with comment, running
  totals (cash/card/voucher breakdown, pay-ins/outs, profit), shift history
  via the top-right clock-icon button

## Known limitations / good next steps

- **Grid layout is stored per-device** (`AsyncStorage`), not synced across
  multiple POS terminals in the same store. If you want every till at a store
  to show the same board, add one more backend endpoint (a JSON blob keyed by
  store, similar to the existing `SystemSettings` table) and swap
  `src/store/posLayoutStore.ts` to read/write through it instead of
  `AsyncStorage`.
- **`getByIds` in `productService.ts`** fetches a page of up to 100 products
  and filters client-side, since there's no bulk-by-id endpoint yet. Fine at
  small-to-medium catalog sizes; add a real bulk endpoint if your catalog
  grows large.
- **Barcode scanning isn't included** — wasn't in the spec, but `expo-camera`
  + `expo-barcode-scanner` would slot in cleanly next to the product grid if
  wanted later.
- No offline/queueing support — checkout requires a live connection.
