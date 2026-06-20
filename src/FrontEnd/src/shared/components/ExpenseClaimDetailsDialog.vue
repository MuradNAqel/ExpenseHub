<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import Button from 'primevue/button'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Dialog from 'primevue/dialog'
import Tag from 'primevue/tag'
import { getEmployeeCode, getEmployeeName } from '@/features/expenses/expenseEmployees'
import {
  formatCurrency,
  formatDisplayDate,
  getStatusSeverity,
} from '@/features/expenses/expenseFormatters'
import {
  getExpenseApiErrorMessage,
  getExpenseClaimById,
  type ExpenseClaimStatus,
  type ExpenseClaimDetailsResponse,
} from '@/core/api/expensesApi'

type ExpenseClaimDetailsDialogClaim = {
  id: string
  apiId: number
  employeeId: string
  employeeName: string
  title: string
  totalAmount: number
  status: ExpenseClaimStatus
  rejectionReason?: string | null
}

const props = defineProps<{
  visible: boolean
  claim: ExpenseClaimDetailsDialogClaim | null
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
}>()

const claimDetails = ref<ExpenseClaimDetailsResponse | null>(null)
const isLoading = ref(false)
const errorMessage = ref('')

const dialogVisible = computed({
  get: () => props.visible,
  set: (value) => emit('update:visible', value),
})

const rejectionReason = computed(() => {
  const status = claimDetails.value?.status ?? props.claim?.status

  if (status !== 'Rejected') {
    return ''
  }

  return claimDetails.value?.rejectionReason ?? props.claim?.rejectionReason ?? ''
})

watch(
  () => [props.visible, props.claim?.apiId] as const,
  async ([visible, apiId]) => {
    claimDetails.value = null
    errorMessage.value = ''

    if (!visible || !apiId) {
      return
    }

    isLoading.value = true

    try {
      claimDetails.value = await getExpenseClaimById(apiId)
    } catch (error) {
      errorMessage.value = getExpenseApiErrorMessage(error)
    } finally {
      isLoading.value = false
    }
  },
)
</script>

<template>
  <Dialog
    v-model:visible="dialogVisible"
    modal
    header="Expense Claim Details"
    class="claim-details-dialog"
    :draggable="false"
  >
    <div v-if="claim" class="claim-details">
      <div class="claim-details-summary">
        <div>
          <span>Claim</span>
          <strong>{{ claimDetails?.id ?? claim.id }}</strong>
        </div>
        <div>
          <span>Employee</span>
          <strong>{{
            claimDetails ? getEmployeeName(claimDetails.employeeId) : claim.employeeName
          }}</strong>
          <small>{{
            claimDetails ? getEmployeeCode(claimDetails.employeeId) : claim.employeeId
          }}</small>
        </div>
        <div>
          <span>Total</span>
          <strong>{{ formatCurrency(claimDetails?.totalAmount ?? claim.totalAmount) }}</strong>
        </div>
        <div>
          <span>Status</span>
          <Tag
            :value="claimDetails?.status ?? claim.status"
            :severity="getStatusSeverity(claimDetails?.status ?? claim.status)"
          />
        </div>
      </div>

      <div class="claim-title-block">
        <span>Title</span>
        <strong>{{ claimDetails?.title ?? claim.title }}</strong>
      </div>

      <div v-if="rejectionReason" class="claim-rejection-block">
        <span>Rejection Reason</span>
        <strong>{{ rejectionReason }}</strong>
      </div>

      <p v-if="isLoading" class="details-message">Loading claim details...</p>
      <p v-else-if="errorMessage" class="submit-error">{{ errorMessage }}</p>
      <p v-else-if="claimDetails && claimDetails.items.length === 0" class="details-message">
        No expense items were found for this claim.
      </p>

      <DataTable
        v-if="claimDetails && claimDetails.items.length > 0"
        :value="claimDetails.items"
        data-key="id"
        responsive-layout="scroll"
        class="claim-items-table"
      >
        <Column field="category" header="Category" />
        <Column header="Expense Date">
          <template #body="{ data }">
            {{ formatDisplayDate(data.expenseDate) }}
          </template>
        </Column>
        <Column header="Description">
          <template #body="{ data }">
            {{ data.description || 'No description' }}
          </template>
        </Column>
        <Column header="Amount">
          <template #body="{ data }">
            {{ formatCurrency(data.amount) }}
          </template>
        </Column>
      </DataTable>
    </div>

    <template #footer>
      <Button label="Close" severity="secondary" outlined @click="dialogVisible = false" />
    </template>
  </Dialog>
</template>

<style>
.claim-details-dialog {
  width: min(58rem, calc(100vw - 2rem));
}

.claim-details {
  display: grid;
  gap: 1rem;
}

.claim-details-summary {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 0.85rem;
}

.claim-details-summary > div,
.claim-title-block,
.claim-rejection-block {
  display: grid;
  gap: 0.25rem;
  padding: 0.9rem;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  background: #f9fafb;
}

.claim-rejection-block {
  border-color: #fecaca;
  background: #fff7f7;
}

.claim-details-summary span,
.claim-title-block span,
.claim-rejection-block span {
  color: #6b7280;
  font-size: 0.8rem;
  font-weight: 700;
}

.claim-rejection-block strong {
  color: #991b1b;
}

.claim-details-summary small {
  color: #6b7280;
}

.details-message {
  margin: 0;
  color: #4b5563;
  font-weight: 700;
}

.claim-items-table {
  min-width: 44rem;
}

@media (max-width: 900px) {
  .claim-details-summary {
    grid-template-columns: 1fr;
  }
}
</style>
