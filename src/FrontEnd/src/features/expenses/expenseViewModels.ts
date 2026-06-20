import { getEmployeeCode, getEmployeeName } from './expenseEmployees'
import { formatDisplayDate } from './expenseFormatters'
import type { ExpenseClaimStatus, ExpenseClaimSummaryResponse } from './expensesApi'

export type ExpenseClaimRow = {
  id: string
  apiId: number
  employeeId: string
  employeeName: string
  title: string
  itemCount: number | null
  totalAmount: number
  submittedAt: string
  status: ExpenseClaimStatus
}

export function mapExpenseClaimSummary(summary: ExpenseClaimSummaryResponse): ExpenseClaimRow {
  return {
    id: String(summary.id),
    apiId: summary.id,
    employeeId: getEmployeeCode(summary.employeeId),
    employeeName: getEmployeeName(summary.employeeId),
    title: summary.title,
    itemCount: null,
    totalAmount: summary.totalAmount,
    submittedAt: formatDisplayDate(summary.createdAt),
    status: summary.status,
  }
}
