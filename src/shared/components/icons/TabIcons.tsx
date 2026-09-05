import React from 'react';
import Svg, { Path, Rect, Circle } from 'react-native-svg';

type TabIconProps = {
  color: string;
  size?: number;
};

export function HomeIcon({ color, size = 24 }: TabIconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path
        d="M4 10.5L12 4l8 6.5V20a1 1 0 01-1 1h-5v-6H10v6H5a1 1 0 01-1-1v-9.5z"
        stroke={color}
        strokeWidth={1.5}
        strokeLinejoin="round"
      />
      <Path
        d="M14 4.5h2v2"
        stroke={color}
        strokeWidth={1.5}
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </Svg>
  );
}

export function CheckInIcon({ color, size = 24 }: TabIconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path
        d="M3 12h2.5l2-4 3 8 2.5-5 2 3H21"
        stroke={color}
        strokeWidth={1.5}
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </Svg>
  );
}

export function MemoriesIcon({ color, size = 24 }: TabIconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Rect
        x={3}
        y={5}
        width={14}
        height={14}
        rx={2}
        stroke={color}
        strokeWidth={1.5}
      />
      <Rect
        x={7}
        y={3}
        width={14}
        height={14}
        rx={2}
        stroke={color}
        strokeWidth={1.5}
      />
      <Path
        d="M14 10l2 2.5L18 10"
        stroke={color}
        strokeWidth={1.5}
        strokeLinecap="round"
        strokeLinejoin="round"
      />
      <Path
        d="M11 14h6"
        stroke={color}
        strokeWidth={1.5}
        strokeLinecap="round"
      />
    </Svg>
  );
}

export function TogetherIcon({ color, size = 24 }: TabIconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Path
        d="M12 3l1.2 3.6L17 7.8l-3 2.2 1.2 3.6L12 10.4 8.8 13.6 10 10 7 7.8l3.8-1.2L12 3z"
        stroke={color}
        strokeWidth={1.5}
        strokeLinejoin="round"
      />
    </Svg>
  );
}

export function ProfileIcon({ color, size = 24 }: TabIconProps) {
  return (
    <Svg width={size} height={size} viewBox="0 0 24 24" fill="none">
      <Circle cx={12} cy={12} r={9} stroke={color} strokeWidth={1.5} />
      <Circle cx={12} cy={10} r={3} stroke={color} strokeWidth={1.5} />
      <Path
        d="M7 18.5c1.2-2.2 3-3.5 5-3.5s3.8 1.3 5 3.5"
        stroke={color}
        strokeWidth={1.5}
        strokeLinecap="round"
      />
    </Svg>
  );
}
