import type { AuthUser } from "../types";

function base64UrlDecode(input: string): string {
  const base64 = input.replace(/-/g, "+").replace(/_/g, "/");
  const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), "=");
  return decodeURIComponent(
    atob(padded)
      .split("")
      .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
      .join("")
  );
}

export function decodeToken(token: string): AuthUser | null {
  try {
    const payloadSegment = token.split(".")[1];
    const payload = JSON.parse(base64UrlDecode(payloadSegment));

    const rawPermission =
      payload["Permission"] ?? payload["permission"] ?? [];
    const permissions: string[] = Array.isArray(rawPermission)
      ? rawPermission
      : rawPermission
      ? [rawPermission]
      : [];

    return {
      userId: Number(
        payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] ??
          payload["nameid"] ??
          payload["sub"]
      ),
      fullName:
        payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname"] ??
        payload["given_name"] ??
        "",
      username:
        payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] ??
        payload["unique_name"] ??
        payload["name"] ??
        "",
      role:
        payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ??
        payload["role"] ??
        "Unassigned",
      permissions,
    };
  } catch (e) {
    return null;
  }
}

export function isTokenExpired(token: string): boolean {
  try {
    const payloadSegment = token.split(".")[1];
    const payload = JSON.parse(base64UrlDecode(payloadSegment));
    if (!payload.exp) return false;
    return Date.now() >= payload.exp * 1000;
  } catch {
    return true;
  }
}
