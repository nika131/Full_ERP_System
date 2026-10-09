import React, { useEffect } from "react";
import "./global.css";
import { StatusBar } from "expo-status-bar";
import { View, ActivityIndicator, StyleSheet } from "react-native";
import { GestureHandlerRootView } from "react-native-gesture-handler";
import { SafeAreaProvider } from "react-native-safe-area-context";
import { NavigationContainer } from "@react-navigation/native";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";

import { AuthProvider, useAuth } from "./src/context/AuthContext";
import { useTerminalStore } from "./src/store/terminalStore";
import { LoginScreen } from "./src/screens/LoginScreen";
import { RootNavigator } from "./src/navigation/RootNavigator";
import { colors } from "./src/theme/colors";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

function AppContent() {
  const { user, isLoading } = useAuth();

  const loadTerminal = useTerminalStore((s) => s.load);
  const terminalLoaded = useTerminalStore((s) => s.isLoaded);


  useEffect(() => {
    if (!terminalLoaded) {
      loadTerminal();
    }
  }, [terminalLoaded, loadTerminal]);

  if (isLoading || !terminalLoaded) {
    return (
      <View style={styles.splash}>
        <ActivityIndicator
          color={colors.emerald600}
          size="large"
        />
      </View>
    );
  }

  return (
    <NavigationContainer>
      {user ? <RootNavigator /> : <LoginScreen />}
    </NavigationContainer>
  );
}

export default function App() {
  return (
    <GestureHandlerRootView style={{ flex: 1 }}>
      <SafeAreaProvider>
        <QueryClientProvider client={queryClient}>
          <AuthProvider>
            <StatusBar style="dark" />
            <AppContent />
          </AuthProvider>
        </QueryClientProvider>
      </SafeAreaProvider>
    </GestureHandlerRootView>
  );
}

const styles = StyleSheet.create({
  splash: {
    flex: 1,
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: colors.slate50,
  },
});
