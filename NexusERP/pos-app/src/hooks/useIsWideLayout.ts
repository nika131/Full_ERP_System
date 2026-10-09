import { useWindowDimensions } from "react-native";

export const WIDE_BREAKPOINT = 1650;

export function useIsWideLayout(): boolean {
  const { width } = useWindowDimensions();
  return width >= WIDE_BREAKPOINT;
}
