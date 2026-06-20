<script setup lang="ts">
import Card from 'primevue/card'

import { formatCurrency } from '@/features/expenses/expenseFormatters'
import type { ExpenseMonthlyTotalResponse } from '@/core/api/expensesApi'

defineProps<{
  monthlyTotals: ExpenseMonthlyTotalResponse[]
  maxAmount: number
}>()
</script>

<template>
  <Card class="dashboard-card">
    <template #title>Monthly Requested Amount</template>
    <template #content>
      <div v-if="monthlyTotals.length" class="bar-chart">
        <div v-for="month in monthlyTotals" :key="month.month" class="bar-column">
          <div class="bar-track">
            <span
              class="bar-fill"
              :style="{ height: `${Math.max((month.totalAmount / maxAmount) * 100, 6)}%` }"
            />
          </div>
          <strong>{{ month.month }}</strong>
          <small>{{ formatCurrency(month.totalAmount) }}</small>
        </div>
      </div>

      <div v-else class="dashboard-empty-state dashboard-empty-state--compact">
        <i class="pi pi-chart-bar" />
        <strong>No monthly data</strong>
        <p>Submitted claims will populate this chart.</p>
      </div>
    </template>
  </Card>
</template>
