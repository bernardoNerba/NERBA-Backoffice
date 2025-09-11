import { environment } from '../../../environments/environment';

// Dynamic API URL getter
const getApiUrl = (): string => {
  // Use the dynamic method if available in environment
  if (typeof environment.getApiUrl === 'function') {
    return environment.getApiUrl();
  }

  // Fallback to static appUrl
  if (environment.appUrl && environment.appUrl !== '') {
    return environment.appUrl;
  }

  // Final fallback - construct dynamically
  const protocol = window.location.protocol;
  const hostname = window.location.hostname;

  if (environment.production) {
    return `${protocol}//${hostname}:5001`;
  } else {
    return `http://${hostname}:8080`;
  }
};

// Initialize base URL
const BASE_URL = getApiUrl();

export const API_ENDPOINTS = {
  // Debug info (remove in production)
  init: (() => {
    console.log('🔧 Environment loaded:', environment);
    console.log('🌍 Base URL:', BASE_URL);
    console.log('🏭 Is Production:', environment.production);
    console.log('🖥️ Current Host:', window.location.hostname);
    return true;
  })(),

  // Base URL getter for dynamic updates
  getBaseUrl: () => BASE_URL,

  // Update base URL if needed (for reconnection scenarios)
  updateBaseUrl: (newUrl: string) => {
    // This would require refactoring to make BASE_URL mutable
    // For now, recommend page reload after IP change
    console.warn('Base URL change detected. Consider reloading the page.');
    window.location.reload();
  },

  // Auth endpoints
  login: `${BASE_URL}/api/auth/login/`,
  set_role: `${BASE_URL}/api/auth/set-role/`,

  // People endpoints
  all_people: `${BASE_URL}/api/people/`,
  people_not_user: `${BASE_URL}/api/people/not-`,

  // Account endpoints
  all_accs: `${BASE_URL}/api/account/users/`,
  single_acc: `${BASE_URL}/api/account/user/`,
  create_acc: `${BASE_URL}/api/account/register/`,
  update_acc: `${BASE_URL}/api/account/user/update/`,
  block_acc: `${BASE_URL}/api/account/block-user/`,

  // Config endpoints
  get_general_conf: `${BASE_URL}/api/generalinfo/`,
  update_general_conf: `${BASE_URL}/api/generalinfo/update/`,
  get_taxes: `${BASE_URL}/api/tax/`,
  get_taxes_by_type: `${BASE_URL}/api/tax/type/`,
  create_tax: `${BASE_URL}/api/tax/create/`,
  update_tax: `${BASE_URL}/api/tax/update/`,
  delete_tax: `${BASE_URL}/api/tax/delete/`,

  // Companies endpoints
  companies: `${BASE_URL}/api/companies/`,

  // Students endpoints
  studentsByCompany: `${BASE_URL}/api/students/company/`,
  students: `${BASE_URL}/api/students/`,

  // Modules endpoints
  modules: `${BASE_URL}/api/module/`,
  modules_active: `${BASE_URL}/api/module/active/`,

  // Courses endpoints
  courses: `${BASE_URL}/api/courses/`,
  courses_active: `${BASE_URL}/api/courses/active/`,
  coursesKpis: `${BASE_URL}/api/courses/kpis/`,

  // Actions endpoints
  actions: `${BASE_URL}/api/actions/`,
  actionsByModule: `${BASE_URL}/api/actions/module/`,
  actionsByCourse: `${BASE_URL}/api/actions/course/`,
  actionsByCoordinator: `${BASE_URL}/api/actions/coordenator/`,
  actionsKpis: `${BASE_URL}/api/actions/kpis/`,

  // Frames endpoints
  frames: `${BASE_URL}/api/frame/`,

  // Teacher endpoints
  teachers: `${BASE_URL}/api/teacher/`,
  teacherByPerson: `${BASE_URL}/api/teacher/person/`,

  // ModuleTeachings endpoints
  moduleTeachings: `${BASE_URL}/api/ModuleTeachings/`,
  modulesWithoutTeacher: `${BASE_URL}/api/ModuleTeachings/action/`,
  modulesTeachingByAction: `${BASE_URL}/api/ModuleTeachings/action/`,

  // Health check endpoint
  health: `${BASE_URL}/health`,

  // Sessions
  sessions: `${BASE_URL}/api/sessions/`,
  sessionsByAction: `${BASE_URL}/api/sessions/action/`,

  // PDF Reports
  pdfSessionsReport: `${BASE_URL}/api/pdf/action/`,
  pdfSessionDetail: `${BASE_URL}/api/pdf/session/`,
  pdfActionSummary: `${BASE_URL}/api/pdf/action/`,

  // ActionEnrollments endpoints
  actionEnrollments: `${BASE_URL}/api/ActionEnrollment/`,

  // SessionParticipations endpoints
  sessionParticipations: `${BASE_URL}/api/SessionParticipations/`,
  sessionParticipationsBySession: `${BASE_URL}/api/SessionParticipations/session/`,
  sessionParticipationsByAction: `${BASE_URL}/api/SessionParticipations/action/`,
  upsertSessionAttendance: `${BASE_URL}/api/SessionParticipations/upsert-attendance`,

  // ModuleAvaliations endpoints
  moduleAvaliations: `${BASE_URL}/api/moduleavaliations/`,
  moduleAvaliationsByActionId: `${BASE_URL}/api/moduleavaliations/by-action/`,
};
