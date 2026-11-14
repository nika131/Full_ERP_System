import type { UserLookup } from "../types";
import apiClient from "./client";
import { posAccountHttp, posSession } from "./posSession";

export type PosCompany = { 
  companyId: number; 
  companyName: string; 
  roleName: string; 
  userId: number; 
  username: string;
  canActivatePos: boolean;
};

export async function verifyPosActivation(token: string) {
  await posAccountHttp.post(
    "/pos/auth/activate",
    {},
    {
      headers: { Authorization: `Bearer ${token}` },
    }
  );
}

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
    return data.companies.filter(c => c.canActivatePos);
  },
  selectCompany: async (id: number): Promise<string> => {
    const jwt = await posSession.selectCompany(id);

    try {
      await verifyPosActivation(jwt);
      await posSession.setActive(jwt);
      return jwt;
    } catch (error) {
      await posSession.clearCompany();
      throw error;
    }
  },
  switchCashier: async (userId: number, pin: string): Promise<string> => {
    const owner = await posSession.ownerCompanyAccess();
    const { data } = await posAccountHttp.post<{ token: string }>("/pos/auth/switch", { userId, pin }, {
      headers: { Authorization: `Bearer ${owner}` },
    });
    await posSession.setActive(data.token);
    return data.token;
  },
  getSwitchableUsers: async (): Promise<UserLookup[]> => {
    const owner = await posSession.ownerCompanyAccess();

    const { data } = await posAccountHttp.get<UserLookup[]>(
      "/pos/auth/cashiers",
      {
        headers: { Authorization: `Bearer ${owner}` },
      }
    );

    return data;
  },

  getPosStatus: async (): Promise<{
    posUserCount: number;
    soleUserId: number | null;
  }> => {
    const owner = await posSession.ownerCompanyAccess();

    const { data } = await posAccountHttp.get(
      "/pos/auth/status",
      {
        headers: { Authorization: `Bearer ${owner}` },
      }
    );

    return data;
  },
};
