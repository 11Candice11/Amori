export const fontFamilies = {
  headline: 'EBGaramond-Regular',
  headlineMedium: 'EBGaramond-Medium',
  body: 'Manrope-Regular',
  bodyMedium: 'Manrope-Medium',
  bodySemiBold: 'Manrope-SemiBold',
} as const;

export const typography = {
  tabLabel: {
    fontFamily: fontFamilies.bodyMedium,
    fontSize: 10,
    letterSpacing: 0.8,
    textTransform: 'uppercase' as const,
  },
  headline: {
    fontFamily: fontFamilies.headline,
    fontSize: 28,
    lineHeight: 34,
  },
  body: {
    fontFamily: fontFamilies.body,
    fontSize: 16,
    lineHeight: 24,
  },
} as const;
