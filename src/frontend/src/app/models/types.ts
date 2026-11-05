export enum UserRole {
  Employee = 1,
  Manager = 2,
  HR = 3
}

export enum TimeRecordType {
  ClockIn = 1,
  LunchStart = 2,
  LunchEnd = 3,
  ClockOut = 4
}

export enum RecordSource {
  Web = 1,
  Mobile = 2,
  ManualAdjustment = 3
}

export enum AdjustmentStatus {
  Pending = 1,
  Approved = 2,
  Rejected = 3
}

export interface User {
  id: string;
  name: string;
  email: string;
  role: UserRole;
  defaultTimezoneId: string;
  dailyWorkHours: number;
  department: string;
  managerName?: string;
}

export interface LoginResponse {
  token: string;
  userId: string;
  name: string;
  email: string;
  role: UserRole;
  defaultTimezoneId: string;
  department: string;
}

export interface TimeRecordResponse {
  id: string;
  userId: string;
  timestampUtc: string;
  localTimestamp: string;
  timezoneId: string;
  offsetMinutes: number;
  type: TimeRecordType;
  source: RecordSource;
  locationNotes?: string;
}

export interface DailyTimeReport {
  date: string;
  formattedDate: string;
  timezoneId: string;
  records: TimeRecordResponse[];
  totalWorkedTime: string;
  formattedTotalWorked: string;
  totalBreakTime: string;
  formattedTotalBreak: string;
  expectedWorkTime: string;
  balance: string;
  formattedBalance: string;
  isPositiveBalance: boolean;
  hasIrregularity: boolean;
  irregularityReason?: string;
}

export interface MonthlyTimesheetReport {
  userId: string;
  userName: string;
  department: string;
  year: number;
  month: number;
  days: DailyTimeReport[];
  totalWorked: string;
  formattedTotalWorked: string;
  totalExpected: string;
  formattedTotalExpected: string;
  totalBalance: string;
  formattedTotalBalance: string;
  isPositiveTotalBalance: boolean;
}

export interface AdjustmentResponse {
  id: string;
  userId: string;
  userName: string;
  targetDate: string;
  requestedType: TimeRecordType;
  requestedTimestampUtc: string;
  requestedLocalTime: string;
  timezoneId: string;
  offsetMinutes: number;
  reason: string;
  status: AdjustmentStatus;
  reviewedById?: string;
  reviewedByName?: string;
  reviewedAtUtc?: string;
  reviewNotes?: string;
  createdAtUtc: string;
}

export interface CreateAdjustmentRequest {
  targetDate: string;
  requestedType: TimeRecordType;
  requestedLocalTime: string;
  timezoneId: string;
  offsetMinutes: number;
  reason: string;
}

export interface ReviewAdjustmentRequest {
  status: AdjustmentStatus;
  reviewNotes?: string;
}

export interface ClockPunchRequest {
  type?: TimeRecordType;
  timezoneId?: string;
  offsetMinutes?: number;
  locationNotes?: string;
  simulatedTimestampUtc?: string;
}

export interface TeamMonthlySummaryItem {
  userId: string;
  name: string;
  email: string;
  department: string;
  defaultTimezoneId: string;
  dailyWorkHours: number;
  formattedTotalWorked: string;
  formattedTotalExpected: string;
  formattedTotalBalance: string;
  isPositiveTotalBalance: boolean;
  irregularitiesCount: number;
  activeDaysCount: number;
}

export interface TeamMonthlySummaryReport {
  year: number;
  month: number;
  totalEmployees: number;
  formattedTotalWorkedCompany: string;
  formattedTotalBalanceCompany: string;
  isPositiveTotalBalanceCompany: boolean;
  totalIrregularitiesCompany: number;
  items: TeamMonthlySummaryItem[];
}

