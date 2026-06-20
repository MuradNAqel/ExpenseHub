<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import Button from 'primevue/button'
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Skeleton from 'primevue/skeleton'
import Tag from 'primevue/tag'
import CreateExpenseClaimDialog from './CreateExpenseClaimDialog.vue'
import ExpenseClaimDetailsDialog from '@/shared/components/ExpenseClaimDetailsDialog.vue'
import { formatCurrency, getStatusSeverity } from './expenseFormatters'
import { getExpenseApiErrorMessage, getExpenseClaims } from '@/core/api/expensesApi'
import { mapExpenseClaimSummary, type ExpenseClaimRow } from './expenseViewModels'

const createDialogVisible = ref(false)
const detailsDialogVisible = ref(false)
const selectedClaim = ref<ExpenseClaimRow | null>(null)
const submittedClaims = ref<ExpenseClaimRow[]>([])
const isLoadingClaims = ref(false)
const claimsError = ref('')
const pageSize = ref(10)
const firstRecordIndex = ref(0)
const totalRecords = ref(0)
let loadRequestId = 0

type SkeletonClaimRow = {
  id: string
  isSkeleton: true
}

type ClaimTableRow = ExpenseClaimRow | SkeletonClaimRow

onMounted(loadExpenseClaims)
onBeforeUnmount(() => {
  loadRequestId += 1
})

const skeletonRows = computed<SkeletonClaimRow[]>(() =>
  Array.from({ length: pageSize.value }, (_, index) => ({
    id: `claim-skeleton-${index}`,
    isSkeleton: true,
  })),
)

const tableRows = computed<ClaimTableRow[]>(() =>
  isLoadingClaims.value ? skeletonRows.value : submittedClaims.value,
)

const paginatorTotalRecords = computed(() =>
  isLoadingClaims.value && totalRecords.value === 0 ? pageSize.value : totalRecords.value,
)

async function loadExpenseClaims() {
  const requestId = (loadRequestId += 1)
  isLoadingClaims.value = true
  claimsError.value = ''

  try {
    const page = Math.floor(firstRecordIndex.value / pageSize.value) + 1
    const result = await getExpenseClaims({ page, pageSize: pageSize.value })

    if (requestId !== loadRequestId) {
      return
    }

    submittedClaims.value = result.items.map(mapExpenseClaimSummary)
    totalRecords.value = result.totalCount
  } catch (error) {
    if (requestId === loadRequestId) {
      claimsError.value = getExpenseApiErrorMessage(error)
    }
  } finally {
    if (requestId === loadRequestId) {
      isLoadingClaims.value = false
    }
  }
}

function openClaimDetails(claim: ClaimTableRow) {
  if (isSkeletonRow(claim)) {
    return
  }

  selectedClaim.value = claim
  detailsDialogVisible.value = true
}

function addSubmittedClaim(claim: ExpenseClaimRow) {
  firstRecordIndex.value = 0
  submittedClaims.value = [claim, ...submittedClaims.value].slice(0, pageSize.value)
  totalRecords.value += 1
}

function changePage(event: { first: number; rows: number }) {
  firstRecordIndex.value = event.first
  pageSize.value = event.rows
  loadExpenseClaims()
}

function isSkeletonRow(row: ClaimTableRow): row is SkeletonClaimRow {
  return 'isSkeleton' in row
}
</script>

<template>
  <section class="employee-expenses-page">
    <div class="expense-page-toolbar">
      <div>
        <h2>Expense Claims</h2>
        <p>Employees can submit claim titles with one or more expense items.</p>
      </div>

      <Button label="Create Claim" icon="pi pi-plus" @click="createDialogVisible = true" />
    </div>

    <Card class="claims-table-card">
      <template #title>Submitted Claims</template>
      <template #content>
        <p v-if="claimsError" class="submit-error">{{ claimsError }}</p>

        <DataTable
          :value="tableRows"
          data-key="id"
          striped-rows
          responsive-layout="scroll"
          class="claims-table"
          selection-mode="single"
          paginator
          lazy
          :rows="pageSize"
          :first="firstRecordIndex"
          :total-records="paginatorTotalRecords"
          :rows-per-page-options="[10]"
          current-page-report-template="{first} to {last} of {totalRecords}"
          paginator-template="FirstPageLink PrevPageLink CurrentPageReport NextPageLink LastPageLink"
          @row-click="openClaimDetails($event.data)"
          @page="changePage"
        >
          <template #empty>
            <div class="table-empty-state">
              <span class="table-empty-state__icon">
                <i class="pi pi-receipt" />
              </span>
              <strong>No expense claims yet</strong>
              <p>Create a claim and submitted records will appear here.</p>
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
          <Column header="Items">
            <template #body="{ data }">
              <Skeleton v-if="isSkeletonRow(data)" width="3rem" height="1rem" />
              <span v-else>{{ data.itemCount ?? 'View' }}</span>
            </template>
          </Column>
          <Column header="Total">
            <template #body="{ data }">
              <Skeleton v-if="isSkeletonRow(data)" width="5rem" height="1rem" />
              <span v-else>{{ formatCurrency(data.totalAmount) }}</span>
            </template>
          </Column>
          <Column header="Submitted">
            <template #body="{ data }">
              <Skeleton v-if="isSkeletonRow(data)" width="7rem" height="1rem" />
              <span v-else>{{ data.submittedAt }}</span>
            </template>
          </Column>
          <Column header="Status">
            <template #body="{ data }">
              <Skeleton v-if="isSkeletonRow(data)" width="5rem" height="1.35rem" />
              <Tag v-else :value="data.status" :severity="getStatusSeverity(data.status)" />
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>

    <CreateExpenseClaimDialog
      v-model:visible="createDialogVisible"
      @submitted="addSubmittedClaim"
    />

    <ExpenseClaimDetailsDialog v-model:visible="detailsDialogVisible" :claim="selectedClaim" />
  </section>
</template>

<style src="./EmployeeExpensesPage.css"></style>
