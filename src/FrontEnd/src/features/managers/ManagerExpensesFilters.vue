<script setup lang="ts">
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import type { StatusFilter } from './managerExpenseTypes'

const searchTerm = defineModel<string>('searchTerm', { required: true })
const selectedStatus = defineModel<StatusFilter>('selectedStatus', { required: true })

defineProps<{
  statusOptions: StatusFilter[]
}>()

const emit = defineEmits<{
  apply: []
  clear: []
}>()
</script>

<template>
  <div class="manager-expenses-toolbar">
    <span class="search-field">
      <i class="pi pi-search" />
      <InputText
        v-model="searchTerm"
        placeholder="Search by claim id or title"
        @keyup.enter="emit('apply')"
      />
    </span>

    <Select
      v-model="selectedStatus"
      :options="statusOptions"
      class="status-filter"
      placeholder="Filter status"
      @change="emit('apply')"
    />

    <div class="filter-actions">
      <Button icon="pi pi-filter" label="Apply" outlined @click="emit('apply')" />
      <Button icon="pi pi-times" label="Clear" severity="secondary" text @click="emit('clear')" />
    </div>
  </div>
</template>
