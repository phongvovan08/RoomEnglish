export enum NumeralThousandGroupStyles {
  THOUSAND = 'thousand',
  LAKH = 'lakh',
  WAN = 'wan',
  NONE = 'none',
}
export const DECIMAL_SEPARATORS = [
  ',', // comma
  '.', // dot
  '٫', // Persian Momayyez
  '。', // Chinese dot
];

export const DefaultNumeralDecimalMark: string = '.';
export const DefaultNumeralThousandGroupStyle: NumeralThousandGroupStyles =
  NumeralThousandGroupStyles.THOUSAND;

