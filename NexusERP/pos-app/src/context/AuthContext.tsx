import React, { createContext, useContext, useEffect, useState, useCallback, useRef } from "react";
import { AppState, DeviceEventEmitter } from "react-native";
import { useQueryClient } from "@tanstack/react-query";
import { authService, verifyPosActivation, type PosCompany } from "../api/authService";
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

  const [posUserCount, setPosUserCount] = useState<number | null>(null);
  const sessionVersion = useRef(0);
  const wasBackgrounded = useRef(false);

  const resetCashierData = useCallback(() => {
    queryClient.clear();
    clearCart();
    usePosLayoutStore.getState().reset();
  }, [queryClient, clearCart]);

  const lockCashier = useCallback(
    async (failedToken?: string) => {
      const current = await posSession.getActive();

      // Ignore errors from a previously active cashier.
      if (failedToken && current !== failedToken) {
        return;
      }

      sessionVersion.current += 1;

      await posSession.clearActive();
      resetCashierData();
      setUser(null);

      // Do not change accountAuthenticated or companyId.
      // Do not clear the selected store.
    },
    [resetCashierData]
  );

  useEffect(() => {
    let cancelled = false;

    (async () => {
      try {
        await posSession.accountAccess();

        if (cancelled) return;
        setAccountAuthenticated(true);

        const storedId = await posSession.getCompanyId();

        if (storedId) {
          try {
            const owner = await posSession.ownerCompanyAccess();

            // Check that the saved activator is still authorized.
            // getPosStatus requires POS.Activate.
            const status = await authService.getPosStatus();

            // Never restore an old PIN session automatically.
            await posSession.clearActive();

            if (cancelled) return;

            setCompanyId(storedId);
            setPosUserCount(status.posUserCount);

            const ownerUser = decodeToken(owner);

            if (
              status.posUserCount === 1 &&
              ownerUser?.userId === status.soleUserId
            ) {
              // Exactly one user: no PIN necessary.
              await posSession.setActive(owner);
              if (!cancelled) setUser(ownerUser);
            } else {
              // Multiple users: PIN required.
              setUser(null);
            }
          } catch {
            // Saved company activation is no longer valid.
            // Preserve email login; allow company selection.
            await posSession.clearCompany();

            if (!cancelled) {
              setCompanyId(null);
              setUser(null);
              setPosUserCount(null);
            }
          }
        }
      } catch {
        await posSession.clearAll();

        if (!cancelled) {
          setAccountAuthenticated(false);
          setCompanyId(null);
          setUser(null);
        }
      } finally {
        if (!cancelled) setIsLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  useEffect(() => {
    const subscription = AppState.addEventListener(
      "change",
      state => {
        if (state === "background") {
          wasBackgrounded.current = true;

          if (companyId && posUserCount !== 1) {
            void lockCashier();
          }
        }

        if (
          state === "active" &&
          wasBackgrounded.current
        ) {
          wasBackgrounded.current = false;

          // If the company previously had only one user,
          // check whether another POS user has been added.
          if (companyId && posUserCount === 1) {
            void authService.getPosStatus()
              .then(status => {
                setPosUserCount(status.posUserCount);

                if (status.posUserCount !== 1) {
                  void lockCashier();
                }
              })
              .catch(() => {
                void lockCashier();
              });
          }
        }
      }
    );

    return () => subscription.remove();
  }, [companyId, posUserCount, lockCashier]);

  useEffect(() => {
    const subscription = DeviceEventEmitter.addListener(
      "tenexerp-pos-cashier-locked",
      (failedToken?: string) => {
        void lockCashier(failedToken);
      }
    );

    return () => subscription.remove();
  }, [lockCashier]);

  const login = useCallback(async (email: string, password: string) => {
    await authService.loginAccount(email, password);
    resetCashierData();
    setAccountAuthenticated(true);
    setCompanyId(null);
    setUser(null);
  }, [resetCashierData]);

  const selectCompany = useCallback(async (id: number) => {
    const token = await authService.selectCompany(id);
    const status = await authService.getPosStatus();

    resetCashierData();
    await useTerminalStore.getState().clearStore(id);
    useTerminalStore.getState().reset();

    setCompanyId(id);
    setPosUserCount(status.posUserCount);
    setUser(decodeToken(token));
  }, [resetCashierData]);

  const getCompanies = useCallback(() => authService.getCompanies(), []);

  const switchUser = useCallback(async (userId: number, pin: string) => {
    const version = sessionVersion.current;

    const token = await authService.switchCashier(userId, pin);

    if (
      version !== sessionVersion.current ||
      AppState.currentState === "background"
    ) {
      if ((await posSession.getActive()) === token) {
        await posSession.clearActive();
      }
      throw new Error("POS was locked. Enter the PIN again.");
    }

    const cashier = decodeToken(token);

    if (!cashier) {
      await posSession.clearActive();
      throw new Error("Invalid cashier session.");
    }

    resetCashierData();
    setUser(cashier);
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
      await useTerminalStore.getState().clearStore();
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
