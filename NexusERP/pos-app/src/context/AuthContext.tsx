import React, { createContext, useContext, useEffect, useState, useCallback } from "react";
import { DeviceEventEmitter } from "react-native";
import { useQueryClient } from "@tanstack/react-query";
import { authService, type PosCompany } from "../api/authService";
import { posSession, tokenExpiresSoon } from "../api/posSession";
import { decodeToken } from "../utils/jwt";
import type { AuthUser } from "../types";
import { useCartStore } from "../store/cartStore";
import { useTerminalStore } from "../store/terminalStore";
import { usePosLayoutStore } from "../store/posLayoutStore";

interface AuthContextValue {
  user: AuthUser | null;
  isLoading: boolean;
  accountAuthenticated: boolean;
  companyId: number | null;
  login: (email: string, password: string) => Promise<void>;
  getCompanies: () => Promise<PosCompany[]>;
  selectCompany: (companyId: number) => Promise<void>;
  switchUser: (userId: number, pin: string) => Promise<void>;
  switchCompany: () => Promise<void>;
  logout: () => Promise<void>;
  hasPermission: (permission: string) => boolean;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [companyId, setCompanyId] = useState<number | null>(null);
  const [accountAuthenticated, setAccountAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const clearCart = useCartStore(s => s.clear);
  const queryClient = useQueryClient();

  const resetCashierData = useCallback(() => {
    queryClient.clear();
    clearCart();
    usePosLayoutStore.getState().reset();
  }, [queryClient, clearCart]);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        await posSession.accountAccess(); // refresh and validate Account session
        if (cancelled) return;
        setAccountAuthenticated(true);
        const storedId = await posSession.getCompanyId();
        if (storedId) {
          const owner = await posSession.ownerCompanyAccess();
          let active = await posSession.getActive();
          if (!active || tokenExpiresSoon(active, 0)) {
            active = owner; // stale PIN session never silently re-authenticates as that cashier
            await posSession.setActive(owner);
          }
          if (cancelled) return;
          setCompanyId(storedId);
          setUser(decodeToken(active));
        }
      } catch {
        await posSession.clearAll();
      } finally { if (!cancelled) setIsLoading(false); }
    })();
    return () => { cancelled = true; };
  }, []);

  useEffect(() => {
    const subscription = DeviceEventEmitter.addListener("tenexerp-pos-session-lost", () => {
      void (async () => {
        await posSession.clearAll();
        resetCashierData();
        useTerminalStore.getState().reset();
        setAccountAuthenticated(false);
        setCompanyId(null);
        setUser(null);
      })();
    });
    return () => subscription.remove();
  }, [resetCashierData]);

  const login = useCallback(async (email: string, password: string) => {
    await authService.loginAccount(email, password);
    resetCashierData();
    setAccountAuthenticated(true);
    setCompanyId(null);
    setUser(null);
  }, [resetCashierData]);

  const selectCompany = useCallback(async (id: number) => {
    const token = await authService.selectCompany(id);
    resetCashierData();
    useTerminalStore.getState().reset();
    setCompanyId(id);
    setUser(decodeToken(token));
  }, [resetCashierData]);

  const getCompanies = useCallback(() => authService.getCompanies(), []);

  const switchUser = useCallback(async (userId: number, pin: string) => {
    const token = await authService.switchCashier(userId, pin);
    resetCashierData();
    setUser(decodeToken(token));
  }, [resetCashierData]);

  const switchCompany = useCallback(async () => {
    await posSession.clearCompany();
    resetCashierData();
    useTerminalStore.getState().reset();
    setCompanyId(null);
    setUser(null);
  }, [resetCashierData]);

  const logout = useCallback(async () => {
    try { await posSession.logout(); }
    finally {
      resetCashierData();
      useTerminalStore.getState().reset();
      setUser(null);
      setCompanyId(null);
      setAccountAuthenticated(false);
    }
  }, [resetCashierData]);

  const hasPermission = useCallback((permission: string) => user?.permissions.includes(permission) ?? false, [user]);

  return <AuthContext.Provider value={{ user, companyId, isLoading, accountAuthenticated, login, getCompanies, selectCompany, switchUser, switchCompany, logout, hasPermission }}>
    {children}
  </AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
