import React, { useState } from "react";
import { View, Text, TextInput, Pressable, ActivityIndicator, KeyboardAvoidingView, Platform, Linking } from "react-native";
import { LogIn, Lock, User as UserIcon } from "lucide-react-native";
import { useAuth } from "../context/AuthContext";
import { getErrorMessage } from "../api/client";
import { APP_NAME } from "../theme/colors";
import { AppScreen } from "@/components/layout/AppScreen";

export function LoginScreen() {
  const { login } = useAuth();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async () => {
    if (!username || !password) {
      setError("Enter your username and password.");
      return;
    }

    setError(null);
    setIsSubmitting(true);

    try {
      await login(username, password);
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  };

  const openPrivacyPolicy = () => {
    Linking.openURL("https://tenexerp.com/privacy-policy");
  };

  const openTermsOfService = () => {
    Linking.openURL("https://tenexerp.com/terms-of-service");
  };

  const openSupportEmail = () => {
    Linking.openURL("mailto:support@tenexerp.com");
  };

  return (
    <AppScreen>
      <KeyboardAvoidingView
        className="flex-1 items-center justify-center p-6"
        behavior={Platform.OS === "ios" ? "padding" : undefined}
      >
      <View className="w-full max-w-[380px] items-center rounded-2xl bg-white p-8 shadow-md">
        <View className="mb-4 h-14 w-14 items-center justify-center rounded-full bg-emerald-600">
          <Text className="text-2xl font-extrabold text-white">
            {APP_NAME.charAt(0)}
          </Text>
        </View>

        <Text className="text-xl font-extrabold text-slate-800">
          {APP_NAME} Register
        </Text>

        <Text className="mb-6 mt-1 text-[13px] text-slate-500">
          Sign in to start your shift
        </Text>

        <View className="mb-3 w-full flex-row items-center gap-2 rounded-lg border border-slate-300 px-3 py-3">
          <UserIcon size={16} color="#94a3b8" />

          <TextInput
            value={username}
            onChangeText={setUsername}
            placeholder="Username"
            autoCapitalize="none"
            className="flex-1 text-[15px]"
          />
        </View>

        <View className="mb-3 w-full flex-row items-center gap-2 rounded-lg border border-slate-300 px-3 py-3">
          <Lock size={16} color="#94a3b8" />

          <TextInput
            value={password}
            onChangeText={setPassword}
            placeholder="Password"
            secureTextEntry
            className="flex-1 text-[15px]"
            onSubmitEditing={handleSubmit}
          />
        </View>

        {error ? (
          <Text className="mb-3 self-start text-[13px] text-red-600">
            {error}
          </Text>
        ) : null}

        <Pressable
          className="mt-2 w-full flex-row items-center justify-center gap-2 rounded-lg bg-emerald-600 py-3.5"
          onPress={handleSubmit}
          disabled={isSubmitting}
        >
          {isSubmitting ? (
            <ActivityIndicator color="#ffffff" />
          ) : (
            <>
              <LogIn size={16} color="#ffffff" />

              <Text className="text-[15px] font-bold text-white">
                Sign In
              </Text>
            </>
          )}
        </Pressable>

        <View className="mt-6 w-full flex-row flex-wrap items-center justify-center gap-x-4 gap-y-2">
          <Pressable onPress={openPrivacyPolicy}>
            <Text className="text-xs font-medium text-slate-500">
              Privacy Policy
            </Text>
          </Pressable>

          <Pressable onPress={openTermsOfService}>
            <Text className="text-xs font-medium text-slate-500">
              Terms of Service
            </Text>
          </Pressable>

          <Pressable onPress={openSupportEmail}>
            <Text className="text-xs font-medium text-slate-500">
              Support
            </Text>
          </Pressable>
        </View>

        <Text className="mt-4 text-[11px] text-slate-400">
          © {new Date().getFullYear()} TenexERP
        </Text>
      </View>
      </KeyboardAvoidingView>
    </AppScreen>
  );
}
