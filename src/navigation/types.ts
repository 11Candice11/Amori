export type MainTabParamList = {
  Home: undefined;
  CheckIn: undefined;
  Memories: undefined;
  Together: undefined;
  Profile: undefined;
};

export const TAB_CONFIG = [
  { name: 'Home' as const, label: 'HOME' },
  { name: 'CheckIn' as const, label: 'CHECK-IN' },
  { name: 'Memories' as const, label: 'MEMORIES' },
  { name: 'Together' as const, label: 'TOGETHER' },
  { name: 'Profile' as const, label: 'PROFILE' },
];
