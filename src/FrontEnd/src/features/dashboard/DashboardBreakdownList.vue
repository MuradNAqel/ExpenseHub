<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'

import { formatCurrency } from '@/features/expenses/expenseFormatters'
import type { DashboardBreakdownItem } from './dashboardTypes'

const props = defineProps<{
  title: string
  items: DashboardBreakdownItem[]
  variant?: 'category' | 'employee'
  emptyTitle: string
  emptyDescription: string
}>()

const listClass = computed(() => (props.variant === 'employee' ? 'employee-list' : 'category-list'))
</script>

<template>
  <Card class="dashboard-card">
    <template #title>{{ title }}</template>
    <template #content>
      <div v-if="items.length" :class="listClass">
        <div v-for="item in items" :key="item.id">
          <div class="list-row-header">
            <strong>{{ item.label }}</strong>
            <span>{{ formatCurrency(item.amount) }}</span>
          </div>
          <div class="progress-track">
            <span
              class="progress-fill"
              :class="{ 'progress-fill--employee': variant === 'employee' }"
              :style="{ width: `${item.percentage}%` }"
            />
          </div>
          <small>{{ item.detail }}</small>
        </div>
      </div>

      <div v-else class="dashboard-empty-state dashboard-empty-state--compact">
        <i class="pi pi-list" />
        <strong>{{ emptyTitle }}</strong>
        <p>{{ emptyDescription }}</p>
      </div>
    </template>
  </Card>
</template>
