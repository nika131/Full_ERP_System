import axios from "axios";
import { DeviceEventEmitter } from "react-native";
import { posSession, tokenExpiresSoon, tokenType } from "./posSession";

export const API_BASE_URL = process.env.EXPO_PUBLIC_API_BASE_URL || "http://192.168.100.3:5116/api";
export const TOKEN_KEY = "pos_active_cashier_v2";

interface ApiErrorPayload {
  Message?: unknown; message?: unknown; error?: unknown; title?: unknown;
  errors?: Record<string, string | string[]>;
}

export const getStoredToken = posSession.getActive;
export const setStoredToken = posSession.setActive;
export const deleteStoredToken = posSession.clearActive;

const apiClient = axios.create({ baseURL: API_BASE_URL, timeout: 15000 });

apiClient.interceptors.request.use(async (config) => {
  let token = await posSession.getActive();
  if (token && tokenType(token) === "company" && tokenExpiresSoon(token)) {
    try { token = await posSession.ownerCompanyAccess(); }
    catch (err) { DeviceEventEmitter.emit("tenexerp-pos-session-lost"); throw err; }
  }
  if (token && tokenType(token) === "pos_cashier" && tokenExpiresSoon(token, 0)) {
    DeviceEventEmitter.emit("tenexerp-pos-session-lost");
    throw new Error("Cashier session expired. Ask the terminal owner to sign in again.");
  }
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

apiClient.interceptors.response.use(response => response, error => {
  if (axios.isAxiosError(error) && error.response?.status === 401 &&
      !String(error.config?.url ?? "").includes("/verify-pin"))
    DeviceEventEmitter.emit("tenexerp-pos-session-lost");
  return Promise.reject(error);
});

export function getErrorMessage(err: unknown): string {
  if (axios.isAxiosError<ApiErrorPayload | string>(err)) {
    const data = err.response?.data;
    if (typeof data === "string" && data.trim()) return data;
    if (data && typeof data === "object") {
      const message = data.Message ?? data.message ?? data.error ?? data.title;
      if (message != null) return String(message);
      if (data.errors && typeof data.errors === "object") {
        const messages = Object.values(data.errors).flatMap(v => Array.isArray(v) ? v : [v]).filter(Boolean).map(String);
        if (messages.length) return messages.join("\n");
      }
    }
  }
  return err instanceof Error ? err.message : "Something went wrong. Please try again.";
}

export default apiClient;
