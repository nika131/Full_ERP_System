import { useEffect, useState } from "react";
import { ActivityIndicator, Linking, Pressable, ScrollView, Text, View } from "react-native";
import { Building2, ChevronRight, LogOut, RefreshCw } from "lucide-react-native";
import { useAuth } from "../context/AuthContext";
import type { PosCompany } from "../api/authService";
import { getErrorMessage } from "../api/client";
import { AppScreen } from "../components/layout/AppScreen";

export function CompanySelectionScreen() {
  const { getCompanies, selectCompany, logout } = useAuth();
  const [companies, setCompanies] = useState<PosCompany[]>([]);
  const [isLoading, setLoading] = useState(true);
  const [busyId, setBusyId] = useState<number | null>(null);
  const [error, setError] = useState("");

  const load = async () => {
    setLoading(true);
    setError("");

    try {
      const companies = await getCompanies();
      setCompanies(companies);

      if (companies.length === 1) {
        await selectCompany(companies[0].companyId);
      }
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { void load(); }, []);

  const choose = async (id: number) => {
    setBusyId(id); setError("");
    try { await selectCompany(id); }
    catch (err) { setError(getErrorMessage(err)); }
    finally { setBusyId(null); }
  };

  return <AppScreen>
    <ScrollView contentContainerClassName="flex-grow items-center justify-center px-6 py-10">
      <View className="w-full max-w-[430px] gap-4">
        <View className="h-14 w-14 items-center justify-center rounded-2xl bg-emerald-600">
          <Building2 color="white" size={27} />
        </View>
        <Text className="text-2xl font-extrabold text-slate-800">
          Choose your company
        </Text>
        <Text className="text-sm leading-6 text-slate-500">
          Select the company this terminal belongs to. 
          Your permissions and store list will be loaded automatically.
        </Text>
        {error ? 
          <Text className="rounded-lg bg-red-50 p-3 text-red-700">
            {error}
          </Text> : null}
        {isLoading ? 
          <ActivityIndicator color="#059669" /> : companies.length === 0 ?
          <View className="gap-3 rounded-xl bg-amber-50 p-4">
            <Text className="text-sm text-slate-700">
              You don't have a company available for POS activation. 
              Create your own company or ask an administrator 
              to grant you POS activation permission.
            </Text>

            <Pressable
              onPress={() =>
                void Linking.openURL(
                  "https://tenexerp.com/companies?view=create"
                )
              }
              className="items-center rounded-lg bg-emerald-600 px-4 py-3"
            >
              <Text className="font-semibold text-white">
                Create your company
              </Text>
            </Pressable>
          </View>
        :
          companies.map(c => 
            <Pressable key={c.companyId} disabled={busyId !== null} onPress={() => void choose(c.companyId)}
            className="flex-row items-center gap-3 rounded-xl border border-slate-200 bg-white p-5">
            <Building2 size={22} color="#059669" />
            <View className="flex-1">
              <Text className="font-bold text-slate-800">{c.companyName}</Text>
              <Text className="text-xs text-slate-500">{c.roleName}</Text>
            </View>
            {busyId === c.companyId ? <ActivityIndicator color="#059669" /> : <ChevronRight color="#64748b" size={18} />}
          </Pressable>)}
        <Pressable onPress={() => void load()} 
          className="mt-2 flex-row items-center gap-2 self-center"
        >
          <RefreshCw size={16} color="#047857" />
          <Text className="font-semibold text-emerald-700">
            Refresh companies
          </Text>
        </Pressable>

        <Pressable onPress={() => void logout()} 
          className="mt-3 flex-row items-center gap-2 self-center"
        >
          <LogOut size={16} color="#dc2626" />
          <Text className="font-semibold text-red-600">
            Sign out
          </Text>
        </Pressable>

      </View>
    </ScrollView>
  </AppScreen>
}
