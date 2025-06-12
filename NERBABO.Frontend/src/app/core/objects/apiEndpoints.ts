import { environment } from '../../../environments/environment.development';

export const API_ENDPOINTS = {
  // auth
  login: `${environment.appUrl}/api/auth/login/`,

  // people
  all_people: `${environment.appUrl}/api/people/`,
  people_not_user: `${environment.appUrl}/api/people/not-user/`,

  // account
  all_accs: `${environment.appUrl}/api/account/users/`,
  single_acc: `${environment.appUrl}/api/account/user/`,
  create_acc: `${environment.appUrl}/api/account/register/`,
  update_acc: `${environment.appUrl}/api/account/user/update/`,
};
