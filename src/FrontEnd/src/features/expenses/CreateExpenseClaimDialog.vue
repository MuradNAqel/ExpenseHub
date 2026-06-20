<script setup lang="ts">
import { computed, ref } from 'vue'
import Button from 'primevue/button'
import DatePicker from 'primevue/datepicker'
import Dialog from 'primevue/dialog'
import Dropdown from 'primevue/dropdown'
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import { employees, type Employee } from './expenseEmployees'
import { formatCurrency, formatDateOnly, formatDisplayDate } from './expenseFormatters'
import {
  createExpenseClaim,
  getExpenseApiErrorMessage,
  type CreateExpenseClaimRequest,
  type ExpenseCategory,
} from './expensesApi'
import type { ExpenseClaimRow } from './expenseViewModels'

type ExpenseItemForm = {
  category: ExpenseCategory
  amount: number | null
  description: string
  expenseDate: Date | null
}

const props = defineProps<{
  visible: boolean
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  submitted: [claim: ExpenseClaimRow]
}>()

const categories: ExpenseCategory[] = ['Travel', 'Meals', 'Gas', 'Supplies', 'Other']
const selectedEmployee = ref<Employee | null>(null)
const claimTitle = ref('')
const expenseItems = ref<ExpenseItemForm[]>([createExpenseItem()])
const isSubmitting = ref(false)
const submitError = ref('')

const dialogVisible = computed({
  get: () => props.visible,
  set: (value) => emit('update:visible', value),
})

const claimTotal = computed(() =>
  expenseItems.value.reduce((total, item) => total + (item.amount ?? 0), 0),
)

const canSubmitClaim = computed(
  () =>
    selectedEmployee.value !== null &&
    claimTitle.value.trim().length > 0 &&
    expenseItems.value.length > 0 &&
    expenseItems.value.every((item) => item.amount !== null && item.amount > 0 && item.expenseDate),
)

function createExpenseItem(): ExpenseItemForm {
  return {
    category: 'Other',
    amount: null,
    description: '',
    expenseDate: new Date(),
  }
}

function addExpenseItem() {
  expenseItems.value.push(createExpenseItem())
}

function removeExpenseItem(index: number) {
  if (expenseItems.value.length > 1) {
    expenseItems.value.splice(index, 1)
  }
}

function resetForm() {
  selectedEmployee.value = null
  claimTitle.value = ''
  expenseItems.value = [createExpenseItem()]
  submitError.value = ''
}

async function submitClaim() {
  if (!selectedEmployee.value || !canSubmitClaim.value) {
    return
  }

  const employee = selectedEmployee.value
  const request: CreateExpenseClaimRequest = {
    employeeId: employee.id,
    title: claimTitle.value.trim(),
    items: expenseItems.value.map((item) => ({
      category: item.category,
      amount: item.amount ?? 0,
      description: item.description.trim() || null,
      expenseDate: formatDateOnly(item.expenseDate),
    })),
  }

  isSubmitting.value = true
  submitError.value = ''

  try {
    const { id } = await createExpenseClaim(request)

    emit('submitted', {
      id: String(id),
      apiId: id,
      employeeId: employee.code,
      employeeName: employee.name,
      title: request.title,
      itemCount: request.items.length,
      totalAmount: claimTotal.value,
      submittedAt: formatDisplayDate(new Date().toISOString()),
      status: 'Pending',
    })

    dialogVisible.value = false
    resetForm()
  } catch (error) {
    submitError.value = getExpenseApiErrorMessage(error)
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <Dialog
    v-model:visible="dialogVisible"
    modal
    header="Create Expense Claim"
    class="create-claim-dialog"
    :draggable="false"
    @hide="resetForm"
  >
    <div class="claim-form">
      <div class="form-grid">
        <label class="field">
          <span>Employee</span>
          <Dropdown
            v-model="selectedEmployee"
            :options="employees"
            option-label="name"
            placeholder="Choose employee"
            filter
            :virtual-scroller-options="{ itemSize: 44 }"
          >
            <template #option="{ option }">
              <div class="employee-option">
                <strong>{{ option.name }}</strong>
                <small>{{ option.code }}</small>
              </div>
            </template>
            <template #value="{ value, placeholder }">
              <span v-if="value">{{ value.name }} - {{ value.code }}</span>
              <span v-else>{{ placeholder }}</span>
            </template>
          </Dropdown>
        </label>

        <label class="field">
          <span>Expense Claim Title</span>
          <InputText v-model="claimTitle" placeholder="e.g. June travel expenses" />
        </label>
      </div>

      <div class="items-header">
        <div>
          <h3>Expense Items</h3>
          <p>You could add multiple items.</p>
        </div>
        <Button label="Add Item" icon="pi pi-plus" outlined @click="addExpenseItem" />
      </div>

      <div class="expense-items">
        <div v-for="(item, index) in expenseItems" :key="index" class="expense-item-row">
          <label class="field">
            <span>Category</span>
            <Dropdown v-model="item.category" :options="categories" />
          </label>

          <label class="field">
            <span>Amount</span>
            <InputNumber
              v-model="item.amount"
              mode="currency"
              currency="USD"
              locale="en-US"
              :min="0"
            />
          </label>

          <label class="field">
            <span>Expense Date</span>
            <DatePicker v-model="item.expenseDate" show-icon date-format="yy-mm-dd" />
          </label>

          <label class="field field--wide">
            <span>Description</span>
            <Textarea
              v-model="item.description"
              rows="2"
              auto-resize
              placeholder="Optional details"
            />
          </label>

          <Button
            icon="pi pi-trash"
            severity="danger"
            text
            rounded
            aria-label="Remove item"
            :disabled="expenseItems.length === 1"
            @click="removeExpenseItem(index)"
          />
        </div>
      </div>

      <div class="claim-total">
        <span>Total Requested Amount</span>
        <strong>{{ formatCurrency(claimTotal) }}</strong>
      </div>

      <p v-if="submitError" class="submit-error">{{ submitError }}</p>
    </div>

    <template #footer>
      <Button label="Cancel" severity="secondary" outlined @click="dialogVisible = false" />
      <Button
        label="Submit Claim"
        icon="pi pi-send"
        :disabled="!canSubmitClaim || isSubmitting"
        :loading="isSubmitting"
        @click="submitClaim"
      />
    </template>
  </Dialog>
</template>
