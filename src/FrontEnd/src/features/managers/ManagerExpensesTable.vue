<script setup lang="ts">
import Button from 'primevue/button'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Skeleton from 'primevue/skeleton'
import Tag from 'primevue/tag'
import { formatCurrency, getStatusSeverity } from '@/features/expenses/expenseFormatters'
import type { ExpenseClaimRow } from '@/features/expenses/expenseViewModels'
import { isSkeletonRow, type ClaimTableRow, type PageEvent } from './managerExpenseTypes'

defineProps<{
  rows: ClaimTableRow[]
  pageSize: number
  firstRecordIndex: number
  totalRecords: number
  isSavingAction: boolean
  emptyTitle: string
  emptyMessage: string
}>()

const emit = defineEmits<{
  page: [event: PageEvent]
  view: [claim: ExpenseClaimRow]
  approve: [claim: ExpenseClaimRow]
  reject: [claim: ExpenseClaimRow]
}>()

function viewClaim(row: ClaimTableRow) {
  if (!isSkeletonRow(row)) {
    emit('view', row)
  }
}

function approveClaim(row: ClaimTableRow) {
  if (!isSkeletonRow(row)) {
    emit('approve', row)
  }
}

function rejectClaim(row: ClaimTableRow) {
  if (!isSkeletonRow(row)) {
    emit('reject', row)
  }
}
</script>

<template>
  <DataTable
    :value="rows"
    data-key="id"
    striped-rows
    responsive-layout="scroll"
    class="manager-expenses-table"
    selection-mode="single"
    paginator
    lazy
    :rows="pageSize"
    :first="firstRecordIndex"
    :total-records="totalRecords"
    :rows-per-page-options="[10]"
    current-page-report-template="{first} to {last} of {totalRecords}"
    paginator-template="FirstPageLink PrevPageLink CurrentPageReport NextPageLink LastPageLink"
    @row-click="viewClaim($event.data)"
    @page="emit('page', $event)"
  >
    <template #empty>
      <div class="table-empty-state">
        <span class="table-empty-state__icon">
          <i class="pi pi-search" />
        </span>
        <strong>{{ emptyTitle }}</strong>
        <p>{{ emptyMessage }}</p>
      </div>
    </template>

    <Column header="Claim ID">
      <template #body="{ data }">
        <Skeleton v-if="isSkeletonRow(data)" width="4rem" height="1rem" />
        <span v-else>{{ data.id }}</span>
      </template>
    </Column>
    <Column header="Employee">
      <template #body="{ data }">
        <Skeleton v-if="isSkeletonRow(data)" width="9rem" height="1rem" />
        <span v-else>{{ data.employeeName }}</span>
      </template>
    </Column>
    <Column header="Employee ID">
      <template #body="{ data }">
        <Skeleton v-if="isSkeletonRow(data)" width="5.5rem" height="1rem" />
        <span v-else>{{ data.employeeId }}</span>
      </template>
    </Column>
    <Column header="Title">
      <template #body="{ data }">
        <Skeleton v-if="isSkeletonRow(data)" width="12rem" height="1rem" />
        <span v-else>{{ data.title }}</span>
      </template>
    </Column>
    <Column header="Submitted">
      <template #body="{ data }">
        <Skeleton v-if="isSkeletonRow(data)" width="7rem" height="1rem" />
        <span v-else>{{ data.submittedAt }}</span>
      </template>
    </Column>
    <Column header="Amount">
      <template #body="{ data }">
        <Skeleton v-if="isSkeletonRow(data)" width="5rem" height="1rem" />
        <span v-else>{{ formatCurrency(data.totalAmount) }}</span>
      </template>
    </Column>
    <Column header="Status">
      <template #body="{ data }">
        <Skeleton v-if="isSkeletonRow(data)" width="5rem" height="1.35rem" />
        <Tag v-else :value="data.status" :severity="getStatusSeverity(data.status)" />
      </template>
    </Column>
    <Column header="Actions">
      <template #body="{ data }">
        <div class="row-actions">
          <Skeleton v-if="isSkeletonRow(data)" width="8rem" height="2rem" />
          <template v-else>
            <Button
              icon="pi pi-eye"
              text
              rounded
              aria-label="View expense claim"
              title="View"
              @click.stop="viewClaim(data)"
            />
            <Button
              icon="pi pi-check"
              text
              rounded
              severity="success"
              aria-label="Approve expense claim"
              title="Approve"
              :disabled="data.status === 'Approved' || isSavingAction"
              @click.stop="approveClaim(data)"
            />
            <Button
              icon="pi pi-times"
              text
              rounded
              severity="danger"
              aria-label="Reject expense claim"
              title="Reject"
              :disabled="data.status === 'Rejected' || isSavingAction"
              @click.stop="rejectClaim(data)"
            />
          </template>
        </div>
      </template>
    </Column>
  </DataTable>
</template>
