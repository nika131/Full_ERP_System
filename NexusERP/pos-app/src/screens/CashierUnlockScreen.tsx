
import React, { useState } from "react";
import {
  ActivityIndicator,
  Pressable,
  ScrollView,
  Text,
  View,
} from "react-native";
import { useAuth } from "../context/AuthContext";
import { useSwitchableUsersQuery } from "../hooks/queries/useProductQueries";
import { getErrorMessage } from "../api/client";
import { AppScreen } from "../components/layout/AppScreen";
import { PinPad } from "../components/PinPad";
import type { UserLookup } from "../types";

export function CashierUnlockScreen() {
  const { switchUser, logout } = useAuth();
  const {
    data: users = [],
    isLoading,
    error: loadError,
    refetch,
  } = useSwitchableUsersQuery(true);

  const [selected, setSelected] = useState<UserLookup | null>(null);
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);

  const unlock = async (pin: string) => {
    if (!selected || busy) return;

    setBusy(true);
    setError("");

    try {
      await switchUser(selected.userId, pin);
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setBusy(false);
    }
  };

  return (
    <AppScreen>
      <ScrollView contentContainerClassName="flex-grow items-center justify-center p-5">
        <View className="w-full max-w-[420px] gap-4 rounded-2xl bg-white p-6">
          <Text className="text-center text-2xl font-extrabold text-slate-800">
            Unlock POS
          </Text>

          {isLoading ? (
            <ActivityIndicator color="#059669" />
          ) : loadError ? (
            <Text className="text-center text-red-600">
              {getErrorMessage(loadError)}
            </Text>
          ) : selected ? (
            <View className="items-center gap-4">
              <Text className="font-bold text-slate-800">
                {selected.fullName}
              </Text>

              <PinPad
                key={selected.userId}
                label="Enter your PIN"
                onSubmit={pin => void unlock(pin)}
                error={error}
                isSubmitting={busy}
              />

              <Pressable onPress={() => setSelected(null)}>
                <Text className="font-semibold text-emerald-700">
                  Choose another user
                </Text>
              </Pressable>
            </View>
          ) : (
            <View className="gap-2">
              {users.map(employee => (
                <Pressable
                  key={employee.userId}
                  onPress={() => setSelected(employee)}
                  className="rounded-xl border border-slate-200 p-4"
                >
                  <Text className="font-semibold text-slate-800">
                    {employee.fullName}
                  </Text>
                </Pressable>
              ))}

              {users.length === 0 ? (
                <Text className="text-center text-slate-500">
                  No PIN-enabled cashiers found.
                </Text>
              ) : null}
            </View>
          )}

          <Pressable onPress={() => void refetch()}>
            <Text className="text-center text-emerald-700">
              Refresh cashiers
            </Text>
          </Pressable>

          <Pressable onPress={() => void logout()}>
            <Text className="text-center font-semibold text-red-600">
              Log out of terminal
            </Text>
          </Pressable>
        </View>
      </ScrollView>
    </AppScreen>
  );
}
