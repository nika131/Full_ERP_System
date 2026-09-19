import React from "react";
import { createDrawerNavigator } from "@react-navigation/drawer";
import { createNativeStackNavigator } from "@react-navigation/native-stack";
import { useWindowDimensions } from "react-native";

import { DrawerContent } from "./DrawerContent";
import { SalesScreen } from "../screens/SalesScreen";
import { CheckoutScreen } from "../screens/CheckoutScreen";
import { ReceiptsScreen } from "../screens/ReceiptsScreen";
import { ReceiptDetailScreen } from "../screens/ReceiptDetailScreen";
import { ShiftScreen } from "../screens/ShiftScreen";
import { colors } from "../theme/colors";
import { WIDE_BREAKPOINT } from "@/hooks/seIsWideLayout";

import type { SalesStackParamList, ReceiptsStackParamList } from "./types";

const Drawer = createDrawerNavigator();
const SalesStack = createNativeStackNavigator<SalesStackParamList>();
const ReceiptsStack = createNativeStackNavigator<ReceiptsStackParamList>();

function SalesStackNavigator() {
  return (
    <SalesStack.Navigator
      screenOptions={{
        headerShown: false,
        contentStyle: {
          backgroundColor: colors.slate50,
        },
      }}
    >
      <SalesStack.Screen name="SalesHome" component={SalesScreen} />
      <SalesStack.Screen name="Checkout" component={CheckoutScreen} />
    </SalesStack.Navigator>
  );
}

function ReceiptsStackNavigator() {
  return (
    <ReceiptsStack.Navigator
      screenOptions={{
        headerShown: false,
        contentStyle: {
          backgroundColor: colors.slate50,
        },
      }}
    >
      <ReceiptsStack.Screen name="ReceiptsHome" component={ReceiptsScreen} />
      <ReceiptsStack.Screen name="ReceiptDetail" component={ReceiptDetailScreen} />
    </ReceiptsStack.Navigator>
  );
}

export function RootNavigator() {
  const { width } = useWindowDimensions();
  const isWide = width >= WIDE_BREAKPOINT;

  return (
    <Drawer.Navigator
      screenOptions={{
        headerShown: false,
        drawerType: isWide ? "permanent" : "front",
        drawerStyle: {
          width: 280,
        },
        overlayColor: "rgba(15, 23, 42, 0.4)",
      }}
      drawerContent={(props) => <DrawerContent {...props} />}
    >
      <Drawer.Screen name="Sales" component={SalesStackNavigator} />
      <Drawer.Screen name="Receipts" component={ReceiptsStackNavigator} />
      <Drawer.Screen name="Shift" component={ShiftScreen} />
    </Drawer.Navigator>
  );
}