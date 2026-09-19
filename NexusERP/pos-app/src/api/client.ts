import axios from "axios";
import * as SecureStore from "expo-secure-store";

export const API_BASE_URL = ""

export const TOKEN_KEY = "pos_auth_token";

async function getStoredToken(): Promise<string | null> {
  if (typeof window !== "undefined") {
    return window.localStorage.getItem(TOKEN_KEY);
  }

  return SecureStore.getItemAsync(TOKEN_KEY);
}

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 15000,
});

apiClient.interceptors.request.use(async (config) => {
  const token = await getStoredToken();

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

export function getErrorMessage(err: unknown): string {
  if (axios.isAxiosError(err)) {
    const data = err.response?.data as any;

    if (typeof data === "string" && data.trim()) {
      return data;
    }

    if (data?.Message) {
      return String(data.Message);
    }

    if (data?.error) {
      return String(data.error);
    }

    if (data?.title) {
      return String(data.title);
    }

    if (data?.errors && typeof data.errors === "object") {
      const messages = Object.values(data.errors)
        .flat()
        .filter(Boolean)
        .map(String);

      if (messages.length > 0) {
        return messages.join("\n");
      }
    }

    if (err.message) {
      return err.message;
    }
  }

  if (err instanceof Error) {
    return err.message;
  }

  return "Something went wrong. Please try again.";
}

export default apiClient;