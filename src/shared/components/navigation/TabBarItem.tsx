import React from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import type { NavigationRoute, ParamListBase } from '@react-navigation/native';

import {
  CheckInIcon,
  HomeIcon,
  MemoriesIcon,
  ProfileIcon,
  TogetherIcon,
} from '../icons/TabIcons';
import { colors, layout, typography } from '../../theme';
import type { TabRouteName } from './BottomNavBar.types';

type TabBarItemProps = {
  route: NavigationRoute<ParamListBase, string>;
  label: string;
  isFocused: boolean;
  onPress: () => void;
  onLongPress: () => void;
  androidRipple?: boolean;
};

function getTabIcon(name: TabRouteName, color: string) {
  const size = layout.tabIconSize;

  switch (name) {
    case 'Home':
      return <HomeIcon color={color} size={size} />;
    case 'CheckIn':
      return <CheckInIcon color={color} size={size} />;
    case 'Memories':
      return <MemoriesIcon color={color} size={size} />;
    case 'Together':
      return <TogetherIcon color={color} size={size} />;
    case 'Profile':
      return <ProfileIcon color={color} size={size} />;
    default:
      return null;
  }
}

export function TabBarItem({
  route,
  label,
  isFocused,
  onPress,
  onLongPress,
  androidRipple = false,
}: TabBarItemProps) {
  const iconColor = isFocused ? colors.tabActive : colors.tabInactive;

  return (
    <Pressable
      accessibilityRole="button"
      accessibilityState={isFocused ? { selected: true } : {}}
      accessibilityLabel={label}
      onPress={onPress}
      onLongPress={onLongPress}
      style={styles.item}
      android_ripple={
        androidRipple
          ? { color: 'rgba(197, 163, 101, 0.16)', borderless: false }
          : undefined
      }
    >
      <View style={styles.icon}>{getTabIcon(route.name as TabRouteName, iconColor)}</View>
      <Text style={[styles.label, { color: iconColor }]}>{label}</Text>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  item: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    paddingVertical: 10,
    minHeight: 56,
  },
  icon: {
    marginBottom: 4,
  },
  label: {
    ...typography.tabLabel,
  },
});
