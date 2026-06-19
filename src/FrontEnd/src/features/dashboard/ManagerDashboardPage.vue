<script setup lang="ts">
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Tag from 'primevue/tag'

type ExpenseStatus = 'Pending' | 'Approved' | 'Rejected'

type ExpenseSummary = {
  label: string
  value: string
  icon: string
  accent: string
}

type RecentExpense = {
  id: string
  employee: string
  category: string
  submittedAt: string
  amount: string
  status: ExpenseStatus
}

const summaries: ExpenseSummary[] = [
  { label: 'Total Expenses', value: '248', icon: 'pi pi-receipt', accent: '#2563eb' },
  { label: 'Pending Expenses', value: '32', icon: 'pi pi-clock', accent: '#d97706' },
  { label: 'Approved Expenses', value: '184', icon: 'pi pi-check-circle', accent: '#16a34a' },
  { label: 'Rejected Expenses', value: '12', icon: 'pi pi-times-circle', accent: '#dc2626' },
  { label: 'Total Requested Amount', value: '$128,450', icon: 'pi pi-wallet', accent: '#7c3aed' },
]

const recentExpenses: RecentExpense[] = [
  {
    id: 'EXP-1048',
    employee: 'Lina Haddad',
    category: 'Travel',
    submittedAt: 'Jun 18, 2026',
    amount: '$1,240.00',
    status: 'Pending',
  },
  {
    id: 'EXP-1047',
    employee: 'Omar Nasser',
    category: 'Software',
    submittedAt: 'Jun 18, 2026',
    amount: '$320.00',
    status: 'Approved',
  },
  {
    id: 'EXP-1046',
    employee: 'Sara Mansour',
    category: 'Meals',
    submittedAt: 'Jun 17, 2026',
    amount: '$86.50',
    status: 'Rejected',
  },
  {
    id: 'EXP-1045',
    employee: 'Adam Saleh',
    category: 'Office Supplies',
    submittedAt: 'Jun 16, 2026',
    amount: '$214.30',
    status: 'Approved',
  },
  {
    id: 'EXP-1044',
    employee: 'Nour Khalil',
    category: 'Transport',
    submittedAt: 'Jun 15, 2026',
    amount: '$52.75',
    status: 'Pending',
  },
]

function getStatusSeverity(status: ExpenseStatus) {
  switch (status) {
    case 'Approved':
      return 'success'
    case 'Rejected':
      return 'danger'
    default:
      return 'warn'
  }
}
</script>

<template>
  <section class="dashboard-page">
    <div class="summary-grid">
      <Card v-for="summary in summaries" :key="summary.label" class="summary-card">
        <template #content>
          <div class="summary-card__content">
            <span class="summary-card__icon" :style="{ color: summary.accent }">
              <i :class="summary.icon" />
            </span>
            <div>
              <p>{{ summary.label }}</p>
              <strong>{{ summary.value }}</strong>
            </div>
          </div>
        </template>
      </Card>
    </div>

    <Card class="expenses-table-card">
      <template #title>Recent Expenses</template>
      <template #content>
        <DataTable
          :value="recentExpenses"
          data-key="id"
          striped-rows
          responsive-layout="scroll"
          class="expenses-table"
        >
          <Column field="id" header="Expense ID" />
          <Column field="employee" header="Employee" />
          <Column field="category" header="Category" />
          <Column field="submittedAt" header="Submitted" />
          <Column field="amount" header="Amount" />
          <Column header="Status">
            <template #body="{ data }">
              <Tag :value="data.status" :severity="getStatusSeverity(data.status)" />
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>
  </section>
</template>

<style src="./ManagerDashboaredPage.css"></style>
