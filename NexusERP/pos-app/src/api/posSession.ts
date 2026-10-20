import { Platform } from "react-native";
import * as SecureStore from "expo-secure-store";
import axios from "axios";

const BASE_URL = process.env.EXPO_PUBLIC_API_BASE_URL || "http://192.168.100.3:5116/api";
const accountHttp = axios.create({ baseURL: BASE_URL, timeout: 15000 });
const KEYS = {
  access: "pos_account_access_v2",
  refresh: "pos_account_refresh_v2",
  owner: "pos_owner_company_v2",
  active: "pos_active_cashier_v2",
  company: "pos_company_id_v2",
};

async function get(key: string): Promise<string | null> {
  if (Platform.OS === "web") return sessionStorage.getItem(key);
  return SecureStore.getItemAsync(key);
}
async function put(key: string, value: string | null): Promise<void> {
  if (Platform.OS === "web") {
    if (value == null) sessionStorage.removeItem(key);
    else sessionStorage.setItem(key, value);
    return;
  }
  if (value == null) await SecureStore.deleteItemAsync(key);
  else await SecureStore.setItemAsync(key, value);
}
function decodeClaims(token: string): { exp?: number; token_type?: string } {
  const part = token.split(".")[1];
  const raw = part.replace(/-/g, "+").replace(/_/g, "/");
  const padded = raw.padEnd(Math.ceil(raw.length / 4) * 4, "=");
  return JSON.parse(atob(padded));
}
export function tokenExpiresSoon(token: string, seconds = 45): boolean {
  try {
    const decoded = decodeClaims(token);
    return !decoded.exp || decoded.exp * 1000 <= Date.now() + seconds * 1000;
  } catch { return true; }
}
export function tokenType(token: string): string {
  try { return decodeClaims(token).token_type ?? ""; }
  catch { return ""; }
}

let refreshPromise: Promise<string> | null = null;
let companyPromise: Promise<string> | null = null;

export const posSession = {
  getActive: () => get(KEYS.active),
  getCompanyId: async () => {
    const raw = await get(KEYS.company);
    const parsed = Number(raw);
    return raw && Number.isInteger(parsed) && parsed > 0 ? parsed : null;
  },
  setActive: (token: string) => put(KEYS.active, token),
  clearActive: () => put(KEYS.active, null),
  setAccount: async (accessToken: string, refreshToken: string) => {
    await put(KEYS.access, accessToken);
    await put(KEYS.refresh, refreshToken);
  },
  clearCompany: async () => {
    await Promise.all([put(KEYS.owner, null), put(KEYS.active, null), put(KEYS.company, null)]);
  },
  clearAll: async () => {
    await Promise.all(Object.values(KEYS).map(k => put(k, null)));
    if (Platform.OS !== "web") await SecureStore.deleteItemAsync("pos_auth_token").catch(() => undefined);
  },
  refreshAccount: async (): Promise<string> => {
    if (refreshPromise) return refreshPromise;
    refreshPromise = (async () => {
      const refreshToken = await get(KEYS.refresh);
      if (!refreshToken) throw new Error("Sign in again to activate this terminal.");
      const { data } = await accountHttp.post<{ accessToken: string; refreshToken: string }>("/account-auth/refresh", { refreshToken });
      await posSession.setAccount(data.accessToken, data.refreshToken);
      return data.accessToken;
    })().finally(() => { refreshPromise = null; });
    return refreshPromise;
  },
  accountAccess: async (): Promise<string> => {
    const token = await get(KEYS.access);
    return token && !tokenExpiresSoon(token) ? token : posSession.refreshAccount();
  },
  selectCompany: async (companyId: number): Promise<string> => {
    const accountToken = await posSession.accountAccess();
    const { data } = await accountHttp.post<{ accessToken: string }>(`/companies/${companyId}/select`, {}, {
      headers: { Authorization: `Bearer ${accountToken}` },
    });
    await put(KEYS.owner, data.accessToken);
    await put(KEYS.company, String(companyId));
    return data.accessToken;
  },
  ownerCompanyAccess: async (): Promise<string> => {
    const existing = await get(KEYS.owner);
    if (existing && !tokenExpiresSoon(existing)) return existing;
    if (companyPromise) return companyPromise;
    companyPromise = (async () => {
      const companyId = await posSession.getCompanyId();
      if (!companyId) throw new Error("Select a company first.");
      const refreshed = await posSession.selectCompany(companyId);
      const active = await get(KEYS.active);
      if (!active || tokenType(active) === "company") await put(KEYS.active, refreshed);
      return refreshed;
    })().finally(() => { companyPromise = null; });
    return companyPromise;
  },
  logout: async () => {
    try {
      const token = await posSession.accountAccess();
      await accountHttp.post("/account-auth/logout", {}, { headers: { Authorization: `Bearer ${token}` } });
    } finally { await posSession.clearAll(); }
  },
};

export const posAccountHttp = accountHttp;
