export interface Student {
  id: number;
  personId: number;
  companyId: number | null;
  companyName: string;
  studentFullName: string;
  nif: string;
  companyRole: string;
  jobCenter: string;
  employeed: string;
  isEmployeed: boolean;
  isRegisteredWithJobCenter: boolean;
}

export interface StudentForm {
  id: number;
  personId: number;
  companyId: number | null;
  isEmployeed: boolean;
  isRegisteredWithJobCenter: boolean;
  companyRole: string;
}

// {
//     "personId": 7,
//     "companyId": 2,
//     "isEmployeed": true,
//     "isRegisteredWithJobCenter": false,
//     "companyRole": "Desenvolvedor Backend"
// }
