export enum HabilitationEnum {
  WithoutProof = 'Sem Comprovativo',
  WithoutHabilitation = 'Sem escolaridade',
  FirstYear = '1º Ano',
  SecondYear = '2º Ano',
  ThirdYear = '3º Ano',
  FourthYear = '4º Ano',
  FifthYear = '5º Ano',
  SixthYear = '6º Ano',
  SeventhYear = '7º Ano',
  EighthYear = '8º Ano',
  NinthYear = '9º Ano',
  TenthYear = '10º Ano',
  EleventhYear = '11º Ano',
  TwelfthYear = '12º Ano',
  PostSecondary = 'Pós-Secundário',
  Bachelors = 'Bacharelato',
  Undergraduate = 'Licenciatura',
  Masters = 'Mestrado',
  Doctorate = 'Doutoramento',
}

export const HABILITATIONS = Object.entries(HabilitationEnum).map(
  ([key, value]) => ({
    key,
    value,
  })
);
