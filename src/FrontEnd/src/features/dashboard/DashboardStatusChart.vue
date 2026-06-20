<script setup lang="ts">
import Card from 'primevue/card'

import type { DashboardStatusSegment } from './dashboardTypes'

defineProps<{
  totalClaims: number
  segments: DashboardStatusSegment[]
  donutStyle: Record<string, string>
}>()
</script>

<template>
  <Card class="dashboard-card status-card">
    <template #title>Claims By Status</template>
    <template #content>
      <div class="status-chart">
        <div class="status-donut" :style="donutStyle">
          <span>{{ totalClaims }}</span>
          <small>claims</small>
        </div>
        <div class="status-legend">
          <div v-for="segment in segments" :key="segment.status" class="legend-row">
            <span class="legend-dot" :style="{ background: segment.color }" />
            <strong>{{ segment.status }}</strong>
            <span>{{ segment.claimCount }} claims</span>
            <small>{{ segment.percentage }}%</small>
          </div>
        </div>
      </div>
    </template>
  </Card>
</template>
