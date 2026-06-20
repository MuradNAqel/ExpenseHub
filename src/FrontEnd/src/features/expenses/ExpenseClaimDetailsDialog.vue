<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import Button from 'primevue/button'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Dialog from 'primevue/dialog'
import Tag from 'primevue/tag'
import { getEmployeeCode, getEmployeeName } from './expenseEmployees'
import { formatCurrency, formatDisplayDate, getStatusSeverity } from './expenseFormatters'
import {
  getExpenseApiErrorMessage,
  getExpenseClaimById,
  type ExpenseClaimDetailsResponse,
} from './expensesApi'
import type { ExpenseClaimRow } from './expenseViewModels'

const props = defineProps<{
  visible: boolean
  claim: ExpenseClaimRow | null
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
