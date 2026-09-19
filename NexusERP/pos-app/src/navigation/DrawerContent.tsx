import React, { useState } from "react";
import { View, Text, Pressable, FlatList } from "react-native";
import type { DrawerContentComponentProps } from "@react-navigation/drawer";
import { ShoppingBag, Receipt as ReceiptIcon, Clock, Users, LogOut, X } from "lucide-react-native";
import { useAuth } from "../context/AuthContext";
import { useSwitchableUsersQuery } from "../hooks/queries/useProductQueries";
import { ModalSheet } from "../components/ModalSheet";
import { PinPad } from "../components/PinPad";
import { getErrorMessage } from "../api/client";
import { APP_NAME } from "../theme/colors";
import type { UserLookup } from "../types";

const NAV_ITEMS = [
  { name: "Sales", label: "Sales", icon: ShoppingBag },
  { name: "Receipts", label: "Receipts", icon: ReceiptIcon },
  { name: "Shift", label: "Shift", icon: Clock },
];

export function DrawerContent(props: DrawerContentComponentProps) {
  const { user, logout } = useAuth();
  const [switchOpen, setSwitchOpen] = useState(false);

  const initials =
    user?.fullName
      ?.split(" ")
      .map((p) => p[0])
      .slice(0, 2)
      .join("")
      .toUpperCase() || "?";

  return (
    <View className="flex-1 bg-white px-4 pt-8">
      <View className="mb-3 flex-row items-center gap-3">
        <View className="h-11 w-11 items-center justify-center rounded-full bg-emerald-600">
          <Text className="text-[15px] font-extrabold text-white">
            {initials}
          </Text>
        </View>

        <View className="flex-1">
          <Text className="text-[15px] font-bold text-slate-800" numberOfLines={1}>
            {user?.fullName}
          </Text>

          <Text className="mt-px text-xs text-slate-500">
            {user?.role}
          </Text>
        </View>
      </View>

      <Pressable
        className="mb-6 flex-row items-center justify-center gap-1.5 rounded-lg bg-emerald-50 py-2.5"
        onPress={() => setSwitchOpen(true)}
      >
        <Users size={15} color="#047857" />
        <Text className="text-[13px] font-bold text-emerald-700">
          Switch User
        </Text>
      </Pressable>

      <View className="flex-1 gap-1">
        {NAV_ITEMS.map((item) => {
          const isFocused =
            props.state.routeNames[props.state.index] === item.name;

          const Icon = item.icon;

          return (
            <Pressable
              key={item.name}
              className={`flex-row items-center gap-3 rounded-lg px-3 py-3 ${
                isFocused ? "bg-emerald-50" : ""
              }`}
              onPress={() => props.navigation.navigate(item.name)}
            >
              <Icon
                size={18}
                color={isFocused ? "#047857" : "#64748b"}
              />

              <Text
                className={`text-sm font-semibold ${
                  isFocused ? "text-emerald-700" : "text-slate-500"
                }`}
              >
                {item.label}
              </Text>
            </Pressable>
          );
        })}
      </View>

      <View className="gap-3 border-t border-slate-100 py-4">
        <Text className="text-center text-[11px] text-slate-300">
          {APP_NAME} Register
        </Text>

        <Pressable
          className="flex-row items-center justify-center gap-1.5"
          onPress={logout}
        >
          <LogOut size={15} color="#dc2626" />

          <Text className="text-[13px] font-semibold text-red-600">
            Log Out
          </Text>
        </Pressable>
      </View>

      <SwitchUserModal
        visible={switchOpen}
        onClose={() => setSwitchOpen(false)}
      />
    </View>
  );
}

function SwitchUserModal({
  visible,
  onClose,
}: {
  visible: boolean;
  onClose: () => void;
}) {
  const { switchUser } = useAuth();
  const { data: users, isLoading } = useSwitchableUsersQuery(visible);
  const [selectedUser, setSelectedUser] = useState<UserLookup | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleClose = () => {
    setSelectedUser(null);
    setError(null);
    onClose();
  };

  const handlePin = async (pin: string) => {
    if (!selectedUser) return;

    setIsSubmitting(true);
    setError(null);

    try {
      await switchUser(selectedUser.userId, pin);
      handleClose();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <ModalSheet visible={visible} onClose={handleClose} maxWidth={400}>
      <View className="mb-3 flex-row items-center justify-between">
        <Text className="text-base font-bold text-slate-800">
          {selectedUser ? selectedUser.fullName : "Switch User"}
        </Text>

        <Pressable onPress={handleClose}>
          <X size={20} color="#64748b" />
        </Pressable>
      </View>

      {selectedUser ? (
        <View className="items-center pt-4">
          <PinPad
            label="Enter PIN"
            onSubmit={handlePin}
            error={error}
            isSubmitting={isSubmitting}
          />

          <Pressable
            onPress={() => setSelectedUser(null)}
            className="mt-4"
          >
            <Text className="text-[13px] font-semibold text-emerald-600">
              Choose a different user
            </Text>
          </Pressable>
        </View>
      ) : (
        <FlatList
          data={users || []}
          keyExtractor={(u) => String(u.userId)}
          className="max-h-[360px]"
          renderItem={({ item }) => (
            <Pressable
              className="flex-row items-center gap-3 border-b border-slate-100 py-3"
              onPress={() => setSelectedUser(item)}
            >
              <View className="h-9 w-9 items-center justify-center rounded-full bg-slate-100">
                <Text className="text-xs font-bold text-slate-600">
                  {item.fullName
                    .split(" ")
                    .map((p) => p[0])
                    .slice(0, 2)
                    .join("")
                    .toUpperCase()}
                </Text>
              </View>

              <Text className="text-sm font-semibold text-slate-700">
                {item.fullName}
              </Text>
            </Pressable>
          )}
        />
      )}
    </ModalSheet>
  );
}