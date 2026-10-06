import React from "react";
import { View, Text, Pressable, Linking, ScrollView } from "react-native";
import { Mail, Shield, FileText, Globe, ExternalLink, Info } from "lucide-react-native";
import { MenuButton } from "@/components/MenuButton";

const PRIVACY_POLICY_URL = "https://tenexerp.com/privacy-policy";
const TERMS_OF_SERVICE_URL = "https://tenexerp.com/terms-of-service";
const WEBSITE_URL = "https://tenexerp.com";
const SUPPORT_EMAIL_URL = "mailto:support@tenexerp.com";

export function SupportScreen() {
  return (
    <ScrollView className="flex-1 bg-slate-50" contentContainerClassName="p-6">
      <View className="mx-auto w-full max-w-[700px]">
        <View className="mb-5 flex-row items-center gap-3">
          <MenuButton />

          <Text className="text-2xl font-extrabold text-slate-800">
            Support
          </Text>
        </View>

        <Text className="mb-6 mt-1 text-sm text-slate-500">
          Help, legal information, and TenexERP links.
        </Text>

        <View className="overflow-hidden rounded-2xl border border-slate-200 bg-white">
          <Pressable
            className="flex-row items-center gap-4 border-b border-slate-100 px-5 py-4"
            onPress={() => Linking.openURL(SUPPORT_EMAIL_URL)}
          >
            <View className="h-10 w-10 items-center justify-center rounded-full bg-emerald-50">
              <Mail size={18} color="#059669" />
            </View>

            <View className="flex-1">
              <Text className="text-sm font-bold text-slate-800">
                Contact Support
              </Text>

              <Text className="mt-0.5 text-xs text-slate-500">
                support@tenexerp.com
              </Text>
            </View>

            <ExternalLink size={17} color="#94a3b8" />
          </Pressable>

          <Pressable
            className="flex-row items-center gap-4 border-b border-slate-100 px-5 py-4"
            onPress={() => Linking.openURL(PRIVACY_POLICY_URL)}
          >
            <View className="h-10 w-10 items-center justify-center rounded-full bg-emerald-50">
              <Shield size={18} color="#059669" />
            </View>

            <View className="flex-1">
              <Text className="text-sm font-bold text-slate-800">
                Privacy Policy
              </Text>

              <Text className="mt-0.5 text-xs text-slate-500">
                View our privacy policy
              </Text>
            </View>

            <ExternalLink size={17} color="#94a3b8" />
          </Pressable>

          <Pressable
            className="flex-row items-center gap-4 border-b border-slate-100 px-5 py-4"
            onPress={() => Linking.openURL(TERMS_OF_SERVICE_URL)}
          >
            <View className="h-10 w-10 items-center justify-center rounded-full bg-emerald-50">
              <FileText size={18} color="#059669" />
            </View>

            <View className="flex-1">
              <Text className="text-sm font-bold text-slate-800">
                Terms of Service
              </Text>

              <Text className="mt-0.5 text-xs text-slate-500">
                View our terms of service
              </Text>
            </View>

            <ExternalLink size={17} color="#94a3b8" />
          </Pressable>

          <Pressable
            className="flex-row items-center gap-4 px-5 py-4"
            onPress={() => Linking.openURL(WEBSITE_URL)}
          >
            <View className="h-10 w-10 items-center justify-center rounded-full bg-emerald-50">
              <Globe size={18} color="#059669" />
            </View>

            <View className="flex-1">
              <Text className="text-sm font-bold text-slate-800">
                TenexERP Website
              </Text>

              <Text className="mt-0.5 text-xs text-slate-500">
                tenexerp.com
              </Text>
            </View>

            <ExternalLink size={17} color="#94a3b8" />
          </Pressable>
        </View>

        <Text className="mt-6 text-center text-xs text-slate-400">
          © {new Date().getFullYear()} TenexERP
        </Text>
      </View>

              <View className="mt-5 rounded-2xl border border-slate-200 bg-white p-5">
          <View className="flex-row items-start gap-3">
            <View className="h-9 w-9 items-center justify-center rounded-full bg-emerald-50">
              <Info size={18} color="#059669" />
            </View>

            <View className="flex-1">
              <Text className="text-sm font-bold text-slate-800">
                Account deletion
              </Text>

              <Text className="mt-2 text-xs leading-5 text-slate-500">
                If you created your TenexERP account yourself, you can delete
                your account directly from the TenexERP website.
              </Text>

              <Text className="mt-2 text-xs leading-5 text-slate-500">
                If your account was created by your supervisor, you can ask
                your supervisor to delete it, or you can delete the account
                yourself from the website.
              </Text>

              <Text className="mt-2 text-xs leading-5 text-slate-500">
                In any case, if you have any problems or need assistance,
                you can contact TenexERP Support to request account deletion.
              </Text>
            </View>
          </View>
        </View>
    </ScrollView>
  );
}