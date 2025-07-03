import { environment } from '../../../environments/environment.development';

export const API_ENDPOINTS = {
  // auth
  login: `${environment.appUrl}/api/auth/login/`,
  set_role: `${environment.appUrl}/api/auth/set-role/`,

  // people
  all_people: `${environment.appUrl}/api/people/`,
  people_not_user: `${environment.appUrl}/api/people/not-user/`,

  // account
  all_accs: `${environment.appUrl}/api/account/users/`,
  single_acc: `${environment.appUrl}/api/account/user/`,
  create_acc: `${environment.appUrl}/api/account/register/`,
  update_acc: `${environment.appUrl}/api/account/user/update/`,
  block_acc: `${environment.appUrl}/api/account/block-user/`,

  // config
  get_general_conf: `${environment.appUrl}/api/generalinfo/`,
  update_general_conf: `${environment.appUrl}/api/generalinfo/update/`,
  get_taxes: `${environment.appUrl}/api/tax/`,
  create_tax: `${environment.appUrl}/api/tax/create/`,
  update_tax: `${environment.appUrl}/api/tax/update/`,
  delete_tax: `${environment.appUrl}/api/tax/delete/`,

  // companies
  companies: `${environment.appUrl}/api/companies/`,
};
