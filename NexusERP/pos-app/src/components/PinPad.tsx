import React, { useState } from "react";
import { View, Text, Pressable } from "react-native";
import { Delete } from "lucide-react-native";

interface PinPadProps {
  label: string;
  onSubmit: (pin: string) => void;
  error?: string | null;
  isSubmitting?: boolean;
}

const KEYS = ["1", "2", "3", "4", "5", "6", "7", "8", "9", "", "0", "del"];

export function PinPad({ label, onSubmit, error, isSubmitting }: PinPadProps) {
  const [pin, setPin] = useState("");

  const handleKey = (key: string) => {
    if (isSubmitting) return;

    if (key === "del") {
      setPin((p) => p.slice(0, -1));
      return;
    }

    if (key === "") return;

    const next = (pin + key).slice(0, 4);
    setPin(next);

    if (next.length === 4) {
      onSubmit(next);
      setPin("");
    }
  };

  return (
    <View className="items-center">
      <Text className="mb-3 text-sm text-slate-500">
        {label}
      </Text>

      <View className="mb-2 flex-row gap-3">
        {[0, 1, 2, 3].map((i) => (
          <View
            key={i}
            className={[
              "h-3.5 w-3.5 rounded-full border-[1.5px] border-slate-300",
              i < pin.length ? "border-emerald-600 bg-emerald-600" : "",
            ].join(" ")}
          />
        ))}
      </View>

      {error ? (
        <Text className="mb-1 mt-2 text-[13px] text-red-600">
          {error}
        </Text>
      ) : null}

      <View className="mt-4 w-[240px] flex-row flex-wrap justify-center gap-2">
        {KEYS.map((key, idx) => (
          <Pressable
            key={idx}
            disabled={key === "" || isSubmitting}
            onPress={() => handleKey(key)}
            className={[
              "h-14 w-[68px] items-center justify-center rounded-lg bg-slate-50",
              key === "" ? "bg-transparent" : "",
              "active:bg-slate-100",
            ].join(" ")}
          >
            {key === "del" ? (
              <Delete size={20} color="#475569" />
            ) : (
              <Text className="text-xl font-semibold text-slate-800">
                {key}
              </Text>
            )}
          </Pressable>
        ))}
      </View>
    </View>
  );
}