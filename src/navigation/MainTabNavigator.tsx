import React from 'react';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';

import { BottomNavBar } from '../shared/components/navigation';
import { HomeScreen } from '../features/home/screens/HomeScreen';
import { CheckInScreen } from '../features/mood/screens/CheckInScreen';
import { MemoriesScreen } from '../features/memories/screens/MemoriesScreen';
import { TogetherScreen } from '../features/together/screens/TogetherScreen';
import { ProfileScreen } from '../features/profile/screens/ProfileScreen';
import type { MainTabParamList } from './types';

const Tab = createBottomTabNavigator<MainTabParamList>();

export function MainTabNavigator() {
  return (
    <Tab.Navigator
      tabBar={(props) => <BottomNavBar {...props} />}
      screenOptions={{
        headerShown: false,
      }}
    >
      <Tab.Screen name="Home" component={HomeScreen} />
      <Tab.Screen name="CheckIn" component={CheckInScreen} />
      <Tab.Screen name="Memories" component={MemoriesScreen} />
      <Tab.Screen name="Together" component={TogetherScreen} />
      <Tab.Screen name="Profile" component={ProfileScreen} />
    </Tab.Navigator>
  );
}
