export enum CompanySize {
  Micro = 'Micro',
  Small = 'Pequena',
  Medium = 'Média',
}

export const COMPANY_SIZE = Object.entries(CompanySize).map(([key, value]) => ({
  key,
  value,
}));
