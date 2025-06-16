export interface JwtPayload {
  nameid: string;
  email: string;
  given_name: string;
  family_name: string;
  role: Array<string>;
}
