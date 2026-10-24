import React, { useState } from "react";
import { ActivityIndicator, KeyboardAvoidingView, Linking, Platform, Pressable, Text, TextInput, View } from "react-native";
import { Lock, LogIn, Mail, Store } from "lucide-react-native";
import { useAuth } from "../context/AuthContext";
import { getErrorMessage } from "../api/client";
import { AppScreen } from "../components/layout/AppScreen";

export function LoginScreen() {
  const { login } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);

  const submit = async () => {
    if (!email.trim() || !password) return setError("Enter your account email and password.");
    setBusy(true); setError("");
    try { await login(email.trim(), password); }
    catch (err) { setError(getErrorMessage(err)); }
    finally { setBusy(false); }
  };

  return <AppScreen>
    <KeyboardAvoidingView className="flex-1 items-center justify-center px-5" behavior={Platform.OS === "ios" ? "padding" : undefined}>
      <View className="w-full max-w-[410px] rounded-2xl bg-white p-7 shadow-md">
        <View className="mb-4 h-14 w-14 items-center justify-center rounded-2xl bg-emerald-600"><Store color="white" size={27} /></View>
        <Text className="text-2xl font-extrabold text-slate-800">TenexERP POS</Text>
        <Text className="mb-6 mt-2 text-sm leading-5 text-slate-500">Activate this register using a verified TenexERP Account. Afterwards, your team can switch cashiers with their POS PINs.</Text>
        <View className="mb-3 flex-row items-center gap-3 rounded-lg border border-slate-300 px-3 py-2.5">
          <Mail size={18} color="#64748b" />
          <TextInput className="flex-1 text-[15px]" autoCapitalize="none" keyboardType="email-address" autoComplete="email" value={email} onChangeText={setEmail} placeholder="Account email" />
        </View>
        <View className="mb-3 flex-row items-center gap-3 rounded-lg border border-slate-300 px-3 py-2.5">
          <Lock size={18} color="#64748b" />
          <TextInput className="flex-1 text-[15px]" secureTextEntry autoComplete="password" autoCapitalize="none" value={password} onChangeText={setPassword} placeholder="Password" onSubmitEditing={() => void submit()} />
        </View>
        {!!error && <Text className="mb-3 text-sm text-red-600">{error}</Text>}
        <Pressable disabled={busy} onPress={() => void submit()} className="mt-3 flex-row items-center justify-center gap-2 rounded-xl bg-emerald-600 py-3.5 disabled:opacity-50">
          {busy ? <ActivityIndicator color="white" /> : <><LogIn color="white" size={19} /><Text className="font-bold text-white">Sign in</Text></>}
        </Pressable>
        <Pressable onPress={() => void Linking.openURL("https://tenexerp.com/register")} className="mt-5 items-center"><Text className="text-sm font-medium text-emerald-700">New company? Register on our website</Text></Pressable>
        <Pressable onPress={() => void Linking.openURL("https://tenexerp.com/privacy-policy")} className="mt-4 items-center"><Text className="text-xs text-slate-500">Privacy Policy</Text></Pressable>
      </View>
    </KeyboardAvoidingView>
  </AppScreen>;
}
