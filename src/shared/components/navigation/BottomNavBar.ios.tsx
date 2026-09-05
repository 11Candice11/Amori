import React from 'react';
import { StyleSheet, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

import { TAB_CONFIG } from '../../../navigation/types';
import { colors, layout } from '../../theme';
import type { BottomNavBarProps } from './BottomNavBar.types';
import { TabBarItem } from './TabBarItem';

/**
 * iOS bottom navigation bar.
 * Custom floating tab bar with safe-area insets and subtle top border (no elevation).
 */
export function BottomNavBar({ state, descriptors, navigation }: BottomNavBarProps) {
  const insets = useSafeAreaInsets();

  return (
    <View
      style={[
        styles.container,
        { paddingBottom: insets.bottom },
      ]}
    >
      <View style={styles.bar}>
        {state.routes.map((route, index) => {
          const { options } = descriptors[route.key];
          const tabConfig = TAB_CONFIG.find((tab) => tab.name === route.name);
          const label = tabConfig?.label ?? options.title ?? route.name;
          const isFocused = state.index === index;

          const onPress = () => {
            const event = navigation.emit({
              type: 'tabPress',
              target: route.key,
              canPreventDefault: true,
            });

            if (!isFocused && !event.defaultPrevented) {
              navigation.navigate(route.name, route.params);
            }
          };

          const onLongPress = () => {
            navigation.emit({
              type: 'tabLongPress',
              target: route.key,
            });
          };

          return (
            <TabBarItem
              key={route.key}
              route={route}
              label={label}
              isFocused={isFocused}
              onPress={onPress}
              onLongPress={onLongPress}
            />
          );
        })}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    backgroundColor: colors.background,
  },
  bar: {
    flexDirection: 'row',
    alignItems: 'stretch',
    minHeight: layout.tabBarHeight,
    backgroundColor: colors.surface,
    borderTopLeftRadius: layout.tabBarTopRadius,
    borderTopRightRadius: layout.tabBarTopRadius,
    borderTopWidth: StyleSheet.hairlineWidth,
    borderLeftWidth: StyleSheet.hairlineWidth,
    borderRightWidth: StyleSheet.hairlineWidth,
    borderColor: colors.border,
  },
});
