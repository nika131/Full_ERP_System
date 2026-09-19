import React, { createContext, useContext, useEffect, useState, useCallback } from "react";
import * as SecureStore from "expo-secure-store";
import { authService } from "../api/authService";
import { TOKEN_KEY } from "../api/client";
import { decodeToken, isTokenExpired } from "../utils/jwt";
import type { AuthUser } from "../types";
import { useCartStore } from "../store/cartStore";

interface AuthContextValue {
  user: AuthUser | null;
  isLoading: boolean;
  login: (username: string, password: string) => Promise<void>;
  switchUser: (userId: number, pin: string) => Promise<void>;
  logout: () => Promise<void>;
  hasPermission: (permission: string) => boolean;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

async function getStoredToken(): Promise<string | null> {
  if (typeof window !== "undefined") {
    return window.localStorage.getItem(TOKEN_KEY);
  }

  return SecureStore.getItemAsync(TOKEN_KEY);
}

async function setStoredToken(token: string): Promise<void> {
  if (typeof window !== "undefined") {
    window.localStorage.setItem(TOKEN_KEY, token);
    return;
  }

  await SecureStore.setItemAsync(TOKEN_KEY, token);
}

async function deleteStoredToken(): Promise<void> {
  if (typeof window !== "undefined") {
    window.localStorage.removeItem(TOKEN_KEY);
    return;
  }

  await SecureStore.deleteItemAsync(TOKEN_KEY);
}

async function persistTokenAndSetUser(
  token: string,
  setUser: (u: AuthUser | null) => void
) {
  await setStoredToken(token);
  setUser(decodeToken(token));
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const clearCart = useCartStore((s) => s.clear);

  useEffect(() => {
    (async () => {
      const token = await getStoredToken();

      if (token && !isTokenExpired(token)) {
        setUser(decodeToken(token));
      } else if (token) {
        await deleteStoredToken();
      }

      setIsLoading(false);
    })();
  }, []);

  const login = useCallback(async (username: string, password: string) => {
    const token = await authService.login(username, password);
    await persistTokenAndSetUser(token, setUser);
  }, []);

  // Switching users mid-session: the previous cashier's cart shouldn't leak
  // into the next cashier's session, so we clear it here.
  const switchUser = useCallback(
    async (userId: number, pin: string) => {
      const token = await authService.pinLogin(userId, pin);
      await persistTokenAndSetUser(token, setUser);
      clearCart();
    },
    [clearCart]
  );

  const logout = useCallback(async () => {
    await deleteStoredToken();
    setUser(null);
    clearCart();
  }, [clearCart]);

  const hasPermission = useCallback(
    (permission: string) => user?.permissions.includes(permission) ?? false,
    [user]
  );

  return (
    <AuthContext.Provider
      value={{ user, isLoading, login, switchUser, logout, hasPermission }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}