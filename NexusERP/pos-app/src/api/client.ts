import axios from "axios";
import * as SecureStore from "expo-secure-store";

export const API_BASE_URL =
  process.env.EXPO_PUBLIC_API_BASE_URL ||
  "http://192.168.100.3:5116/api";

export const TOKEN_KEY = "pos_auth_token";

interface ApiErrorPayload {
  Message?: unknown;
  message?: unknown;
  error?: unknown;
  title?: unknown;
  errors?: Record<string, string | string[]>;
}

let webToken: string | null = null;

export async function getStoredToken(): Promise<string | null> {
  if (typeof window !== "undefined") {
    return webToken;
  }

  return SecureStore.getItemAsync(TOKEN_KEY);
}

export async function setStoredToken(token: string): Promise<void> {
  if (typeof window !== "undefined") {
    webToken = token;
    return;
  }

  await SecureStore.setItemAsync(TOKEN_KEY, token);
}

export async function deleteStoredToken(): Promise<void> {
  if (typeof window !== "undefined") {
    webToken = null;
    return;
  }

  await SecureStore.deleteItemAsync(TOKEN_KEY);
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
  if (axios.isAxiosError<ApiErrorPayload | string>(err)) {
    const data = err.response?.data;

    if (typeof data === "string" && data.trim()) {
      return data;
    }

    if (data && typeof data === "object") {
      const directMessage =
        data.Message ?? data.message ?? data.error ?? data.title;

      if (directMessage != null) {
        return String(directMessage);
      }

      if (data.errors && typeof data.errors === "object") {
        const messages = Object.values(data.errors)
          .flatMap((value) => (Array.isArray(value) ? value : [value]))
          .filter(Boolean)
          .map(String);

        if (messages.length > 0) {
          return messages.join("\n");
        }
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
