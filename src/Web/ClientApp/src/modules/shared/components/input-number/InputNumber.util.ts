import type { FormatNumeralOptions } from './InputNumber.type'

import { DefaultNumeralDecimalMark, DECIMAL_SEPARATORS } from './InputNumber.constant'

export const unformatNumeral = (value: string, options?: Pick<FormatNumeralOptions, 'numeralDecimalMark'>): string => {
  const { numeralDecimalMark = DefaultNumeralDecimalMark } = options ?? {}
  console.debug({ numeralDecimalMark })
  const result = value
    .replace(numeralDecimalMark, 'M')
    .replace(/[^0-9-M]/g, '')
    .replace('M', '.')
  console.debug({result})
  return result
}
export const getSeparator = (separatorType: 'group' | 'decimal') => {
  const numberWithGroupAndDecimalSeparator = 1000.1
  return Intl.NumberFormat(navigator.language)
    .formatToParts(numberWithGroupAndDecimalSeparator)
    .find((part) => part.type === separatorType)?.value as string
}

export function formatNumeral({
  value,
  delimiter,
  numeralDecimalMark,
  numeralDecimalScale,
  numeralPositiveOnly,
  numeralIntegerScale
}: {
  value: string | undefined
} & FormatNumeralOptions): string {
  let parts: string[]
  let partInteger: string
  let partDecimal = ''
  if (!value) {
    return ''
  }
  let result = value

  const inputtedDecimalMarks = [...value].filter((el) => DECIMAL_SEPARATORS.includes(el))
  const lastCharacter = result[result.length - 1]
  const lastDecimalMark = inputtedDecimalMarks[inputtedDecimalMarks.length - 1]
  // if user input a decimal mark
  if (lastDecimalMark === lastCharacter && lastDecimalMark !== numeralDecimalMark) {
    // we replaced the inputted mark with the numeralDecimalMark one
    result = result.substring(0, result.length - 1) + numeralDecimalMark
  }
  result = result
    .replace(/[A-Za-z]/g, '')
    // replace the first decimal mark with reserved placeholder
    .replace(numeralDecimalMark, 'M')
    // strip non numeric letters except minus and "M"
    // this is to ensure prefix has been stripped
    .replace(/[^\dM-]/g, '')
    // replace the leading minus with reserved placeholder
    .replace(/^-/, 'N')
    // strip the other minus sign (if present)
    .replace(/-/g, '')
    // replace the minus sign (if present)
    .replace('N', (numeralPositiveOnly ?? false) ? '' : '-')
    // replace decimal mark
    .replace('M', numeralDecimalMark)

  // strip any leading zeros
  result = result.replace(/^(-)?0+(?=\d)/, '$1')
  const partSign: string = result.startsWith('-') ? '-' : ''
  partInteger = result

  if (result.includes(numeralDecimalMark)) {
    parts = result.split(numeralDecimalMark)
    partInteger = parts[0]
    partDecimal = numeralDecimalMark + parts[1].slice(0, numeralDecimalScale)
  }

  if (partSign === '-') {
    partInteger = partInteger.slice(1)
  }

  partInteger = partInteger.replace(/(\d)(?=(\d{3})+$)/g, '$1' + delimiter)

  if (numeralIntegerScale > 0) {
    partInteger = partInteger.slice(0, numeralIntegerScale)
  }
  if (partInteger === '' && partDecimal === '') {
    if (partSign === '-') {
      return partSign
    }
    return partInteger
  }
  return partSign + (isNaN(parseInt(partInteger)) ? '0' : partInteger) + (numeralDecimalScale > 0 ? partDecimal : '')
}
