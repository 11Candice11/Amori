import type { BottomTabBarProps } from '@react-navigation/bottom-tabs';
import type { MainTabParamList } from '../../../navigation/types';

export type BottomNavBarProps = BottomTabBarProps & {
  /** Reserved for future platform-specific options */
};

export type TabRouteName = keyof MainTabParamList;
