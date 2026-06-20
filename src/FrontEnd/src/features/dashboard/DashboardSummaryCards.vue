<script setup lang="ts">
import Card from 'primevue/card'
import Skeleton from 'primevue/skeleton'

import type { DashboardMetric } from './dashboardTypes'

defineProps<{
  metrics: DashboardMetric[]
  isLoading: boolean
}>()
</script>

<template>
  <div class="summary-grid">
    <Card v-for="summary in metrics" :key="summary.label" class="summary-card">
      <template #content>
        <div class="summary-card__content">
          <span class="summary-card__icon" :style="{ color: summary.accent }">
            <i :class="summary.icon" />
          </span>
          <div>
            <p>{{ summary.label }}</p>
            <Skeleton v-if="isLoading" width="7rem" height="1.8rem" />
            <strong v-else>{{ summary.value }}</strong>
            <small>{{ summary.note }}</small>
          </div>
        </div>
      </template>
    </Card>
  </div>
</template>
