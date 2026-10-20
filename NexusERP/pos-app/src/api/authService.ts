import type { UserLookup } from "../types";
import apiClient from "./client";
import { posAccountHttp, posSession } from "./posSession";

export type PosCompany = { companyId: number; companyName: string; roleName: string; userId: number; username: string };

export const authService = {
  loginAccount: async (email: string, password: string): Promise<void> => {
    const { data } = await posAccountHttp.post<{ accessToken: string; refreshToken: string }>(
      "/account-auth/login", { email, password, clientType: "POS" });
    await posSession.clearAll();
    await posSession.setAccount(data.accessToken, data.refreshToken);
  },
  getCompanies: async (): Promise<PosCompany[]> => {
    const access = await posSession.accountAccess();
    const { data } = await posAccountHttp.get<{ companies: PosCompany[] }>("/companies/mine", {
      headers: { Authorization: `Bearer ${access}` },
    });
    return data.companies;
  },
  selectCompany: async (id: number): Promise<string> => {
    const jwt = await posSession.selectCompany(id);
    await posSession.setActive(jwt);
    return jwt;
  },
  switchCashier: async (userId: number, pin: string): Promise<string> => {
    const owner = await posSession.ownerCompanyAccess();
    const { data } = await posAccountHttp.post<{ token: string }>("/pos/auth/switch", { userId, pin }, {
      headers: { Authorization: `Bearer ${owner}` },
    });
    await posSession.setActive(data.token);
    return data.token;
  },
  verifyPin: async (pin: string): Promise<void> => {
    await apiClient.post("/pos/verify-pin", { pin });
  },
  getSwitchableUsers: async (): Promise<UserLookup[]> => {
    const { data } = await apiClient.get<UserLookup[]>("/pos/auth/cashiers");
    return data;
  },
};
