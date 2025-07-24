export interface TeacherForm {
  id: number;
  ivaRegimeId: number;
  irsRegimeId: number;
  personId: number;
  ccp: string;
  competences: string;
  isLecturingFM: boolean;
  isLecturingCQ: boolean;
}

export interface Teacher {
  id: number;
  ivaRegime: string;
  irsRegime: string;
  ccp: string;
  competences: string;
  avarageRating: number;
  ivaRegimeId: number;
  irsRegimeId: number;
  personId: number;
  isLecturingFM: boolean;
  isLecturingCQ: boolean;
}

// {
//     "id": 2,
//     "ivaRegime": "Regime de Isenção",
//     "irsRegime": "Rentenção na Fonte de IRS à taxa de 11,5%",
//     "ccp": "dausodo11i1dj2983/112",
//     "competences": "muito bom prog",
//     "avarageRating": 0,
//     "enrolledFM": "N�o Participa",
//     "enrolledCQ": "N�o Participa"
// }
