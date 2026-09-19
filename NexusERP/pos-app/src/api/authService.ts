import apiClient from "./client";
import type { UserLookup } from "../types";

export const authService = {
  login: async (username: string, password: string): Promise<string> => {
    const res = await apiClient.post("/auth/login", { username, password });
    return res.data.token as string;
  },

  pinLogin: async (userId: number, pin: string): Promise<string> => {
    const res = await apiClient.post("/auth/pin-login", { userId, pin });
    return res.data.token as string;
  },

  verifyPin: async (pin: string): Promise<void> => {
    await apiClient.post("/pos/verify-pin", { pin });
  },

  getSwitchableUsers: async (): Promise<UserLookup[]> => {
    const res = await apiClient.get("/pos/switchable-users");
    return res.data;
  },
};
