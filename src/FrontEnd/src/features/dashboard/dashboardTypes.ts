import type { ExpenseStatusTotalResponse } from '@/core/api/expensesApi'

export type DashboardMetric = {
  label: string
  value: string
  note: string
  icon: string
  accent: string
}

export type DashboardStatusSegment = ExpenseStatusTotalResponse & {
  color: string
  percentage: number
}

export type DashboardBreakdownItem = {
  id: number | string
  label: string
  amount: number
  detail: string
  percentage: number
}
