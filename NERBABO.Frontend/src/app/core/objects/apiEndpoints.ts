import { environment } from '../../../environments/environment';

export const API_ENDPOINTS = {
  // Add this for debugging
  init: (() => {
    console.log('🔧 Environment loaded:', environment);
    console.log('🌍 App URL:', environment.appUrl);
    console.log('🏭 Is Production:', environment.production);
    return true;
  })(),

  // auth
  login: `${environment.appUrl}/api/auth/login/`,
  set_role: `${environment.appUrl}/api/auth/set-role/`,

  // people
  all_people: `${environment.appUrl}/api/people/`,
  people_not_user: `${environment.appUrl}/api/people/not-`,

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
  students: `${environment.appUrl}/api/students/`,

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

  // teacher
  teachers: `${environment.appUrl}/api/teacher/`,
  teacherByPerson: `${environment.appUrl}/api/teacher/person/`,
};
