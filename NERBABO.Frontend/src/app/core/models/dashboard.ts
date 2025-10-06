// KPI Models
export interface Kpi<T> {
  kpiTitle: string;
  queriedAt: Date;
  refersTo: string;
  value: T;
}

export interface ChartDataPoint {
  label: string;
  value: number;
}

export interface MonthDataPoint {
  month: string;
  count: number;
}

export interface GenderTimeSeries {
  gender: string;
  data: MonthDataPoint[];
}

// Dashboard Response Model
export interface DashboardData {
  kpis: {
    studentPayments: Kpi<number>;
    teacherPayments: Kpi<number>;
    totalCompanies: Kpi<number>;
  };
  charts: {
    studentsByHabilitationLvl: Kpi<ChartDataPoint[]>;
    studentResults: Kpi<ChartDataPoint[]>;
    actionHabilitationsLvl: Kpi<ChartDataPoint[]>;
    studentGenders: Kpi<GenderTimeSeries[]>;
  };
  top5: {
    actionsByLocality: Kpi<ChartDataPoint[]>;
    actionsByRegiment: Kpi<ChartDataPoint[]>;
    actionsByStatus: Kpi<ChartDataPoint[]>;
  };
}

// Tab Content Model for UI
export interface TabContent {
  value: string;
  title: string;
  content: { name: string; count: number }[];
}
