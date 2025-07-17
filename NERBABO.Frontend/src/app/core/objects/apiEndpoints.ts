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
  get_taxes_by_type: `${environment.appUrl}/api/tax/type/`,
  create_tax: `${environment.appUrl}/api/tax/create/`,
  update_tax: `${environment.appUrl}/api/tax/update/`,
  delete_tax: `${environment.appUrl}/api/tax/delete/`,

  // companies
  companies: `${environment.appUrl}/api/companies/`,

  // students
  studentsByCompany: `${environment.appUrl}/api/students/company/`,

  // modules
  modules: `${environment.appUrl}/api/module/`,
  modules_active: `${environment.appUrl}/api/module/active/`,

  // courses
  courses: `${environment.appUrl}/api/courses/`,
  courses_active: `${environment.appUrl}/api/courses/active/`,

  // actions
  actions: `${environment.appUrl}/api/actions/`,
  actionsByModule: `${environment.appUrl}/api/actions/module/`,
  actionsByCourse: `${environment.appUrl}/api/actions/course/`,

  // frames
  frames: `${environment.appUrl}/api/frame/`,
};
