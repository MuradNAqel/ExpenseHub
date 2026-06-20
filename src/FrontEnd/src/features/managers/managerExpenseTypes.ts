import type { ExpenseClaimStatus } from '@/core/api/expensesApi'
import type { ExpenseClaimRow } from '@/features/expenses/expenseViewModels'

export type StatusFilter = ExpenseClaimStatus | 'All'

export type SkeletonClaimRow = {
  id: string
  isSkeleton: true
}

export type ClaimTableRow = ExpenseClaimRow | SkeletonClaimRow

export type PageEvent = {
  first: number
  rows: number
}

export function isSkeletonRow(row: ClaimTableRow): row is SkeletonClaimRow {
  return 'isSkeleton' in row
}
