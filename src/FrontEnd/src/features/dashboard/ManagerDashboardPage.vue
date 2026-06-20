<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'

import { getEmployeeName } from '@/features/expenses/expenseEmployees'
import { formatCurrency } from '@/features/expenses/expenseFormatters'
import {
  getExpenseApiErrorMessage,
  getExpenseDashboard,
  type ExpenseClaimStatus,
  type ExpenseDashboardResponse,
} from '@/core/api/expensesApi'

import DashboardBreakdownList from './DashboardBreakdownList.vue'
import DashboardMonthlyChart from './DashboardMonthlyChart.vue'
import DashboardRecentClaimsTable from './DashboardRecentClaimsTable.vue'
import DashboardStatusChart from './DashboardStatusChart.vue'
import DashboardSummaryCards from './DashboardSummaryCards.vue'
import type {
  DashboardBreakdownItem,
  DashboardMetric,
  DashboardStatusSegment,
} from './dashboardTypes'

const dashboard = ref<ExpenseDashboardResponse | null>(null)
const isLoading = ref(false)
const errorMessage = ref('')

onMounted(loadDashboard)

const metrics = computed<DashboardMetric[]>(() => {
  const data = dashboard.value

  return [
    {
      label: 'Total Claims',
      value: data ? String(data.totalClaims) : '0',
      note: data ? `${data.pendingClaims} pending review` : 'Awaiting data',
      icon: 'pi pi-receipt',
      accent: '#2563eb',
    },
    {
      label: 'Requested Amount',
      value: formatCurrency(data?.totalRequestedAmount ?? 0),
      note: 'All submitted claims',
      icon: 'pi pi-wallet',
      accent: '#7c3aed',
    },
    {
      label: 'Pending Amount',
      value: formatCurrency(data?.pendingAmount ?? 0),
      note: 'Needs manager decision',
      icon: 'pi pi-clock',
      accent: '#d97706',
    },
    {
      label: 'Approved Amount',
      value: formatCurrency(data?.approvedAmount ?? 0),
      note: 'Ready for reimbursement',
      icon: 'pi pi-check-circle',
      accent: '#16a34a',
    },
    {
      label: 'Average Claim',
      value: formatCurrency(data?.averageClaimAmount ?? 0),
      note: 'Mean requested value',
      icon: 'pi pi-chart-line',
      accent: '#0f766e',
    },
  ]
})

const statusSegments = computed<DashboardStatusSegment[]>(() => {
  const data = dashboard.value
  const total = data?.totalClaims ?? 0
  const statusColors: Record<ExpenseClaimStatus, string> = {
    Pending:  '#0369a1',
    Approved: '#16a34a',
    Rejected: '#dc2626',
  }

  return (data?.statusTotals ?? []).map((item) => ({
    ...item,
    color: statusColors[item.status],
    percentage: total === 0 ? 0 : Math.round((item.claimCount / total) * 100),
  }))
})

const statusDonutStyle = computed(() => {
  if (!dashboard.value || dashboard.value.totalClaims === 0 || statusSegments.value.length === 0) {
    return { background: '#eef2f7' }
  }

  let cursor = 0
  const parts = statusSegments.value.map((segment) => {
    const start = cursor
    cursor += segment.percentage
    return `${segment.color} ${start}% ${cursor}%`
  })

  return { background: `conic-gradient(${parts.join(', ')})` }
})

const maxMonthlyAmount = computed(() =>
  Math.max(...(dashboard.value?.monthlyTotals.map((item) => item.totalAmount) ?? [0]), 1),
)

const categoryBreakdown = computed<DashboardBreakdownItem[]>(() => {
  const categoryTotals = dashboard.value?.categoryTotals ?? []
  const maxAmount = Math.max(...categoryTotals.map((item) => item.totalAmount), 1)

  return categoryTotals.map((category) => ({
    id: category.category,
    label: category.category,
    amount: category.totalAmount,
    detail: `${category.itemCount} items`,
    percentage: Math.max((category.totalAmount / maxAmount) * 100, 4),
  }))
})

const employeeBreakdown = computed<DashboardBreakdownItem[]>(() => {
  const topEmployees = dashboard.value?.topEmployees ?? []
  const maxAmount = Math.max(...topEmployees.map((item) => item.totalAmount), 1)

  return topEmployees.map((employee) => ({
    id: employee.employeeId,
    label: getEmployeeName(employee.employeeId),
    amount: employee.totalAmount,
    detail: `${employee.claimCount} claims`,
    percentage: Math.max((employee.totalAmount / maxAmount) * 100, 4),
  }))
})

async function loadDashboard() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    dashboard.value = await getExpenseDashboard()
  } catch (error) {
    errorMessage.value = getExpenseApiErrorMessage(error)
  } finally {
    isLoading.value = false
  }
}
</script>

<template>
  <section class="dashboard-page">
    <p v-if="errorMessage" class="dashboard-error">{{ errorMessage }}</p>

    <DashboardSummaryCards :metrics="metrics" :is-loading="isLoading" />

    <div class="dashboard-grid">
      <DashboardStatusChart
        :total-claims="dashboard?.totalClaims ?? 0"
        :segments="statusSegments"
        :donut-style="statusDonutStyle"
      />
      <DashboardMonthlyChart
        :monthly-totals="dashboard?.monthlyTotals ?? []"
        :max-amount="maxMonthlyAmount"
      />
    </div>

    <div class="dashboard-grid dashboard-grid--secondary">
      <DashboardBreakdownList
        title="Spend By Category"
        :items="categoryBreakdown"
        empty-title="No category data"
        empty-description="Expense items will appear here once claims are submitted."
      />
      <DashboardBreakdownList
        title="Top Employees"
        :items="employeeBreakdown"
        variant="employee"
        empty-title="No employee spend"
        empty-description="Employee totals will appear after claim activity."
      />
    </div>

    <DashboardRecentClaimsTable :claims="dashboard?.recentClaims ?? []" />
  </section>
</template>

<style src="./ManagerDashboaredPage.css"></style>
