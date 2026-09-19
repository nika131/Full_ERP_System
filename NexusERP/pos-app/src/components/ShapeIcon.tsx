import React from "react";
import { View, Text } from "react-native";
import Svg, { Rect, Circle, Polygon, Path } from "react-native-svg";
import type { ShapeType } from "../types";

interface ShapeIconProps {
  shapeType: ShapeType;
  color: string;
  text: string | null;
  size?: number;
}

function starPoints(cx: number, cy: number, outerR: number, innerR: number, points = 5) {
  const step = Math.PI / points;
  let path = "";
  for (let i = 0; i < 2 * points; i++) {
    const r = i % 2 === 0 ? outerR : innerR;
    const angle = i * step - Math.PI / 2;
    const x = cx + r * Math.cos(angle);
    const y = cy + r * Math.sin(angle);
    path += `${x},${y} `;
  }
  return path.trim();
}

function regularPolygonPoints(cx: number, cy: number, r: number, sides: number, rotationDeg = -90) {
  const rotation = (rotationDeg * Math.PI) / 180;
  let path = "";
  for (let i = 0; i < sides; i++) {
    const angle = rotation + (i * 2 * Math.PI) / sides;
    const x = cx + r * Math.cos(angle);
    const y = cy + r * Math.sin(angle);
    path += `${x},${y} `;
  }
  return path.trim();
}

export function ShapeIcon({ shapeType, color, text, size = 64 }: ShapeIconProps) {
  const center = size / 2;
  const r = size * 0.42;

  return (
    <View
      className="items-center justify-center"
      style={{ width: size, height: size }}
    >
      <Svg width={size} height={size} viewBox={`0 0 ${size} ${size}`}>
        {shapeType === "square" && (
          <Rect
            x={size * 0.1}
            y={size * 0.1}
            width={size * 0.8}
            height={size * 0.8}
            rx={size * 0.08}
            fill={color}
          />
        )}

        {shapeType === "Circle" && (
          <Circle cx={center} cy={center} r={r} fill={color} />
        )}

        {shapeType === "Triangle" && (
          <Polygon
            points={`${center},${size * 0.14} ${size * 0.9},${size * 0.84} ${size * 0.1},${size * 0.84}`}
            fill={color}
          />
        )}

        {shapeType === "Pentagon" && (
          <Polygon
            points={regularPolygonPoints(center, center, r, 5, -90)}
            fill={color}
          />
        )}

        {shapeType === "Diamond" && (
          <Polygon
            points={regularPolygonPoints(center, center, r, 4, -90)}
            fill={color}
          />
        )}

        {shapeType === "Star" && (
          <Polygon
            points={starPoints(center, center, r, r * 0.45)}
            fill={color}
          />
        )}

        {shapeType === "Heart" && (
          <Polygon
            points=""
            fill="none"
          />
        )}
      </Svg>

      {shapeType === "Heart" && (
        <HeartShape color={color} size={size} />
      )}

      {text ? (
        <View
          className="absolute px-1"
          pointerEvents="none"
        >
          <Text
            className="font-bold text-white"
            style={{
              fontSize: Math.max(9, size * 0.16),
              textShadowColor: "rgba(0,0,0,0.35)",
              textShadowOffset: { width: 0, height: 1 },
              textShadowRadius: 2,
            }}
            numberOfLines={1}
          >
            {text}
          </Text>
        </View>
      ) : null}
    </View>
  );
}

function HeartShape({ color, size }: { color: string; size: number }) {
  const s = size;

  return (
    <Svg
      width={s}
      height={s}
      viewBox={`0 0 ${s} ${s}`}
      style={{ position: "absolute", left: 0, top: 0 }}
    >
      <Path
        d={`
          M ${s * 0.5} ${s * 0.88}
          C ${s * 0.42} ${s * 0.81}, ${s * 0.10} ${s * 0.63}, ${s * 0.10} ${s * 0.38}
          C ${s * 0.10} ${s * 0.22}, ${s * 0.24} ${s * 0.15}, ${s * 0.37} ${s * 0.15}
          C ${s * 0.44} ${s * 0.15}, ${s * 0.48} ${s * 0.19}, ${s * 0.5} ${s * 0.25}
          C ${s * 0.52} ${s * 0.19}, ${s * 0.56} ${s * 0.15}, ${s * 0.63} ${s * 0.15}
          C ${s * 0.76} ${s * 0.15}, ${s * 0.90} ${s * 0.22}, ${s * 0.90} ${s * 0.38}
          C ${s * 0.90} ${s * 0.63}, ${s * 0.58} ${s * 0.81}, ${s * 0.5} ${s * 0.88}
          Z
        `}
        fill={color}
      />
    </Svg>
  );
}