export enum TaxType {
  Iva = 'IVA',
  Irs = 'IRS',
}

export const TAX_TYPES = Object.entries(TaxType).map(([key, value]) => ({
  key,
  value,
}));
