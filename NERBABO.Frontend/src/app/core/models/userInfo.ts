export type UserInfo = {
  id: string;
  personId: number;
  fullName?: string;
  userName: string;
  email: string;
  roles: Array<string> | string;
  isActive: boolean;
};
