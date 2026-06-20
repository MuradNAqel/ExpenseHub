<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import Card from 'primevue/card'
import ExpenseClaimDetailsDialog from '@/shared/components/ExpenseClaimDetailsDialog.vue'
import {
  approveExpenseClaim,
  getExpenseApiErrorMessage,
  getExpenseClaims,
  rejectExpenseClaim,
  type ExpenseClaimStatus,
} from '@/core/api/expensesApi'
import { mapExpenseClaimSummary, type ExpenseClaimRow } from '@/features/expenses/expenseViewModels'
import ManagerExpensesFilters from './ManagerExpensesFilters.vue'
import ManagerExpensesTable from './ManagerExpensesTable.vue'
import RejectExpenseClaimDialog from './RejectExpenseClaimDialog.vue'
import type {
  ClaimTableRow,
  PageEvent,
  SkeletonClaimRow,
  StatusFilter,
} from './managerExpenseTypes'

const statusOptions: StatusFilter[] = ['All', 'Pending', 'Approved', 'Rejected']
const searchTerm = ref('')
const selectedStatus = ref<StatusFilter>('All')
const expenseClaims = ref<ExpenseClaimRow[]>([])
const selectedClaim = ref<ExpenseClaimRow | null>(null)
const isLoadingClaims = ref(false)
const claimsError = ref('')
const detailsVisible = ref(false)
const rejectDialogVisible = ref(false)
const actionError = ref('')
const isSavingAction = ref(false)
const pageSize = ref(10)
const firstRecordIndex = ref(0)
const totalRecords = ref(0)
let loadRequestId = 0

onMounted(loadExpenseClaims)
onBeforeUnmount(() => {
  loadRequestId += 1
})

const skeletonRows = computed<SkeletonClaimRow[]>(() =>
  Array.from({ length: pageSize.value }, (_, index) => ({
    id: `manager-claim-skeleton-${index}`,
    isSkeleton: true,
  })),
)

const tableRows = computed<ClaimTableRow[]>(() =>
  isLoadingClaims.value ? skeletonRows.value : expenseClaims.value,
)

const paginatorTotalRecords = computed(() =>
  isLoadingClaims.value && totalRecords.value === 0 ? pageSize.value : totalRecords.value,
)

const hasActiveFilters = computed(
  () => searchTerm.value.trim().length > 0 || selectedStatus.value !== 'All',
)

const emptyTitle = computed(() =>
  hasActiveFilters.value ? 'No matching expense claims' : 'No expense claims yet',
)

const emptyMessage = computed(() =>
  hasActiveFilters.value
    ? 'There are no records for what you searched or filtered. Adjust the criteria and try again.'
    : 'Submitted expense claims will appear here when employees create them.',
)

async function loadExpenseClaims() {
  const requestId = (loadRequestId += 1)
  isLoadingClaims.value = true
  claimsError.value = ''

  try {
    const page = Math.floor(firstRecordIndex.value / pageSize.value) + 1
    const result = await getExpenseClaims({
      page,
      pageSize: pageSize.value,
      search: searchTerm.value.trim() || undefined,
      status: selectedStatus.value === 'All' ? undefined : selectedStatus.value,
    })

    if (requestId !== loadRequestId) {
      return
    }

    expenseClaims.value = result.items.map(mapExpenseClaimSummary)
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

function applyFilters() {
  firstRecordIndex.value = 0
  loadExpenseClaims()
}

function clearFilters() {
  searchTerm.value = ''
  selectedStatus.value = 'All'
  applyFilters()
}

function changePage(event: PageEvent) {
  firstRecordIndex.value = event.first
  pageSize.value = event.rows
  loadExpenseClaims()
}

function openDetails(claim: ExpenseClaimRow) {
  selectedClaim.value = claim
  detailsVisible.value = true
}

async function approveClaim(claim: ExpenseClaimRow) {
  if (claim.status === 'Approved') {
    return
  }

  selectedClaim.value = claim
  actionError.value = ''
  isSavingAction.value = true

  try {
    await approveExpenseClaim(claim.apiId)
    await loadExpenseClaims()
  } catch (error) {
    actionError.value = getExpenseApiErrorMessage(error)
  } finally {
    isSavingAction.value = false
  }
}

function openRejectDialog(claim: ExpenseClaimRow) {
  if (claim.status === 'Rejected') {
    return
  }

  selectedClaim.value = claim
  actionError.value = ''
  rejectDialogVisible.value = true
}

async function rejectClaim(reason: string) {
  if (!selectedClaim.value) {
    return
  }

  isSavingAction.value = true
  actionError.value = ''

  try {
    await rejectExpenseClaim(selectedClaim.value.apiId, reason)
    rejectDialogVisible.value = false
    await loadExpenseClaims()
  } catch (error) {
    actionError.value = getExpenseApiErrorMessage(error)
  } finally {
    isSavingAction.value = false
  }
}
</script>

<template>
  <section class="manager-expenses-page">
    <Card class="manager-expenses-card">
      <template #title>Expense Claims</template>
      <template #content>
        <ManagerExpensesFilters
          v-model:search-term="searchTerm"
          v-model:selected-status="selectedStatus"
          :status-options="statusOptions"
          @apply="applyFilters"
          @clear="clearFilters"
        />

        <p v-if="claimsError" class="manager-error">{{ claimsError }}</p>
        <p v-if="actionError" class="manager-error">{{ actionError }}</p>

        <ManagerExpensesTable
          :rows="tableRows"
          :page-size="pageSize"
          :first-record-index="firstRecordIndex"
          :total-records="paginatorTotalRecords"
          :is-saving-action="isSavingAction"
          :empty-title="emptyTitle"
          :empty-message="emptyMessage"
          @page="changePage"
          @view="openDetails"
          @approve="approveClaim"
          @reject="openRejectDialog"
        />
      </template>
    </Card>

    <ExpenseClaimDetailsDialog v-model:visible="detailsVisible" :claim="selectedClaim" />

    <RejectExpenseClaimDialog
      v-model:visible="rejectDialogVisible"
      :is-saving="isSavingAction"
      :error-message="actionError"
      @reject="rejectClaim"
    />
  </section>
</template>

<style src="./ManagerExpensesPage.css"></style>
