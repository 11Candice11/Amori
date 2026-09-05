export const colors = {
  primary: '#064E3B',
  primaryLight: '#A7D2B6',
  secondary: '#022C22',
  tertiary: '#D4AF37',
  neutral: '#FDFCF0',

  background: '#0A140F',
  surface: '#0A140F',

  tabActive: '#C5A365',
  tabInactive: '#E0E0E0',

  border: 'rgba(253, 252, 240, 0.08)',
} as const;

export type ColorToken = keyof typeof colors;
