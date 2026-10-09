import React, { createContext, useContext, useEffect, useState, useCallback } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { authService } from "../api/authService";
import {
  getStoredToken,
  setStoredToken,
  deleteStoredToken,
} from "../api/client";
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
  const queryClient = useQueryClient();

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
    queryClient.removeQueries({ queryKey: ["pos"] });
    await persistTokenAndSetUser(token, setUser);
  }, [queryClient]);

  // Switching users mid-session: the previous cashier's cart shouldn't leak
  // into the next cashier's session, so we clear it here.
  const switchUser = useCallback(
    async (userId: number, pin: string) => {
      const token = await authService.pinLogin(userId, pin);
      queryClient.removeQueries({ queryKey: ["pos"] });
      await persistTokenAndSetUser(token, setUser);
      clearCart();
    },
    [clearCart, queryClient]
  );

  const logout = useCallback(async () => {
    await deleteStoredToken();
    queryClient.removeQueries({ queryKey: ["pos"] });
    setUser(null);
    clearCart();
  }, [clearCart, queryClient]);

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
