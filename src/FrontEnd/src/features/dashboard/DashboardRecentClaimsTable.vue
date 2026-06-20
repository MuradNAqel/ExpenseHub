<script setup lang="ts">
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Tag from 'primevue/tag'

import { getEmployeeName } from '@/features/expenses/expenseEmployees'
import {
  formatCurrency,
  formatDisplayDate,
  getStatusSeverity,
} from '@/features/expenses/expenseFormatters'
import type { ExpenseClaimSummaryResponse } from '@/core/api/expensesApi'

defineProps<{
  claims: ExpenseClaimSummaryResponse[]
}>()
</script>

<template>
  <Card class="expenses-table-card">
    <template #title>Recent Claims</template>
    <template #content>
      <DataTable
        :value="claims"
        data-key="id"
        striped-rows
        responsive-layout="scroll"
        class="expenses-table"
      >
        <template #empty>
          <div class="dashboard-empty-state">
            <i class="pi pi-receipt" />
            <strong>No claims yet</strong>
            <p>Recent submitted claims will appear here.</p>
          </div>
        </template>

        <Column field="id" header="Claim ID" />
        <Column header="Employee">
          <template #body="{ data }">
            {{ getEmployeeName(data.employeeId) }}
          </template>
        </Column>
        <Column field="title" header="Title" />
        <Column header="Submitted">
          <template #body="{ data }">
            {{ formatDisplayDate(data.createdAt) }}
          </template>
        </Column>
        <Column header="Amount">
          <template #body="{ data }">
            {{ formatCurrency(data.totalAmount) }}
          </template>
        </Column>
        <Column header="Status">
          <template #body="{ data }">
            <Tag :value="data.status" :severity="getStatusSeverity(data.status)" />
          </template>
        </Column>
      </DataTable>
    </template>
  </Card>
</template>
