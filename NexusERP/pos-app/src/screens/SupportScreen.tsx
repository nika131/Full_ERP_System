import React from "react";
import { Linking, Pressable, ScrollView, Text, View } from "react-native";
import { ExternalLink, FileText, Globe, Info, Mail, Shield } from "lucide-react-native";
import { MenuButton } from "@/components/MenuButton";
import { AppScreen } from "@/components/layout/AppScreen";
import { ScreenHeader } from "@/components/layout/ScreenHeader";

const PRIVACY_POLICY_URL = "https://tenexerp.com/privacy-policy";
const TERMS_OF_SERVICE_URL = "https://tenexerp.com/terms-of-service";
const WEBSITE_URL = "https://tenexerp.com";
const SUPPORT_EMAIL_URL = "mailto:support@tenexerp.com";

export function SupportScreen() {
  return (
    <AppScreen>
      <ScreenHeader title="Support" left={<MenuButton />} />

      <ScrollView contentContainerClassName="items-center p-6">
        <View className="w-full max-w-[700px]">
          <Text className="mb-6 text-sm text-slate-500">
            Help, legal information, and TenexERP links.
          </Text>

          <View className="overflow-hidden rounded-2xl border border-slate-200 bg-white">
            <SupportLink
              label="Contact Support"
              detail="support@tenexerp.com"
              icon={<Mail size={18} color="#059669" />}
              onPress={() => Linking.openURL(SUPPORT_EMAIL_URL)}
            />
            <SupportLink
              label="Privacy Policy"
              detail="View our privacy policy"
              icon={<Shield size={18} color="#059669" />}
              onPress={() => Linking.openURL(PRIVACY_POLICY_URL)}
            />
            <SupportLink
              label="Terms of Service"
              detail="View our terms of service"
              icon={<FileText size={18} color="#059669" />}
              onPress={() => Linking.openURL(TERMS_OF_SERVICE_URL)}
            />
            <SupportLink
              label="TenexERP Website"
              detail="tenexerp.com"
              icon={<Globe size={18} color="#059669" />}
              onPress={() => Linking.openURL(WEBSITE_URL)}
              last
            />
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
                  contact TenexERP Support to request account deletion.
                </Text>
              </View>
            </View>
          </View>

          <Text className="mt-6 text-center text-xs text-slate-400">
            © {new Date().getFullYear()} TenexERP
          </Text>
        </View>
      </ScrollView>
    </AppScreen>
  );
}

function SupportLink({
  label,
  detail,
  icon,
  onPress,
  last = false,
}: {
  label: string;
  detail: string;
  icon: React.ReactNode;
  onPress: () => void;
  last?: boolean;
}) {
  return (
    <Pressable
      className={`flex-row items-center gap-4 px-5 py-4 ${
        last ? "" : "border-b border-slate-100"
      }`}
      onPress={onPress}
    >
      <View className="h-10 w-10 items-center justify-center rounded-full bg-emerald-50">
        {icon}
      </View>
      <View className="flex-1">
        <Text className="text-sm font-bold text-slate-800">{label}</Text>
        <Text className="mt-0.5 text-xs text-slate-500">{detail}</Text>
      </View>
      <ExternalLink size={17} color="#94a3b8" />
    </Pressable>
  );
}
