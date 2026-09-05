import React from 'react';
import { NavigationContainer } from '@react-navigation/native';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { StatusBar } from 'react-native';

import { MainTabNavigator } from '../navigation/MainTabNavigator';
import { colors } from '../shared/theme';

export default function App() {
  return (
    <SafeAreaProvider>
      <StatusBar barStyle="light-content" backgroundColor={colors.background} />
      <NavigationContainer>
        <MainTabNavigator />
      </NavigationContainer>
    </SafeAreaProvider>
  );
}
