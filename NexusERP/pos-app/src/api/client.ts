import axios from "axios";
import { DeviceEventEmitter } from "react-native";
import { posSession, tokenExpiresSoon, tokenType } from "./posSession";
import { API_BASE_URL } from "./apiConfig";

export const TOKEN_KEY = "pos_active_cashier_v2";

interface ApiErrorPayload {
  Message?: unknown; message?: unknown; error?: unknown; title?: unknown;
  errors?: Record<string, string | string[]>;
}

export const getStoredToken = posSession.getActive;
export const setStoredToken = posSession.setActive;
export const deleteStoredToken = posSession.clearActive;

const apiClient = axios.create({ baseURL: API_BASE_URL, timeout: 15000 });

apiClient.interceptors.request.use(async config => {
  let token = await posSession.getActive();

  if (!token) {
    throw new Error("Unlock POS with your PIN.");
  }

  if (
    tokenType(token) === "company" &&
    tokenExpiresSoon(token)
  ) {
    const oldToken = token;
    const renewed = await posSession.ownerCompanyAccess();

    if ((await posSession.getActive()) !== oldToken) {
      throw new Error("Cashier changed. Please retry.");
    }

    await posSession.setActive(renewed);
    token = renewed;
  }

  if (
    tokenType(token) === "pos_cashier" &&
    tokenExpiresSoon(token, 0)
  ) {
    DeviceEventEmitter.emit(
      "tenexerp-pos-cashier-locked",
      token
    );

    throw new Error("Cashier session expired. Enter PIN again.");
  }

  config.headers.Authorization = `Bearer ${token}`;
  return config;
});

apiClient.interceptors.response.use(
  response => response,
  error => {
    if (
      axios.isAxiosError(error) &&
      error.response?.status === 401
    ) {
      const auth = String(
        error.config?.headers?.Authorization ?? ""
      );

      const failedToken = auth.startsWith("Bearer ")
        ? auth.slice(7)
        : null;

      if (
        failedToken &&
        ["company", "pos_cashier"].includes(tokenType(failedToken))
      ) {
        DeviceEventEmitter.emit(
          "tenexerp-pos-cashier-locked",
          failedToken
        );
      }
    }

    return Promise.reject(error);
  }
);

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
