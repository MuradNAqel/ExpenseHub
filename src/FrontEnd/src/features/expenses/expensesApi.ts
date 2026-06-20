import axios from 'axios'

import { httpClient } from '@/app/api/httpClient'

export type ExpenseCategory = 'Travel' | 'Meals' | 'Gas' | 'Supplies' | 'Other'

export type CreateExpenseClaimRequest = {
  employeeId: number
  title: string
  items: CreateExpenseItemRequest[]
}

export type CreateExpenseItemRequest = {
  category: ExpenseCategory
  amount: number
  description: string | null
  expenseDate: string
}

export type CreateExpenseClaimResponse = {
  id: number
}

export type ExpenseClaimStatus = 'Pending' | 'Approved' | 'Rejected'

export type ExpenseClaimSummaryResponse = {
  id: number
  employeeId: number
  title: string
  totalAmount: number
  status: ExpenseClaimStatus
  createdAt: string
  reviewedAt: string | null
  rejectionReason: string | null
}

export type PagedResponse<T> = {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export type ExpenseClaimsQuery = {
  page: number
  pageSize: number
}

export type ExpenseItemResponse = {
  id: number
  expenseClaimId: number
  category: ExpenseCategory
  amount: number
  description: string | null
  expenseDate: string
}

export type ExpenseClaimDetailsResponse = {
  id: number
  employeeId: number
  title: string
  totalAmount: number
  status: ExpenseClaimStatus
  createdAt: string
  reviewedAt: string | null
  rejectionReason: string | null
  items: ExpenseItemResponse[]
}

type ApiErrorResponse = {
  error?: string
  title?: string
  detail?: string
}

export async function createExpenseClaim(request: CreateExpenseClaimRequest) {
  const { data } = await httpClient.post<CreateExpenseClaimResponse>('/api/expenses', request)

  return data
}

export async function getExpenseClaims(query: ExpenseClaimsQuery) {
  const { data } = await httpClient.get<PagedResponse<ExpenseClaimSummaryResponse>>(
    '/api/expenses',
    {
      params: query,
    },
  )

  return data
}

export async function getExpenseClaimById(id: number) {
  const { data } = await httpClient.get<ExpenseClaimDetailsResponse>(`/api/expenses/${id}`)

  return data
}

export function getExpenseApiErrorMessage(error: unknown) {
  if (axios.isAxiosError<ApiErrorResponse>(error)) {
    return (
      error.response?.data?.error ??
      error.response?.data?.detail ??
      error.response?.data?.title ??
      error.message
    )
  }

  return 'Could not submit the expense claim.'
}
