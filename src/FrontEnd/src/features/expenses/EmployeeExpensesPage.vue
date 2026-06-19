<script setup lang="ts">
import { computed, ref } from 'vue'
import Button from 'primevue/button'
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import DatePicker from 'primevue/datepicker'
import Dialog from 'primevue/dialog'
import Dropdown from 'primevue/dropdown'
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import Tag from 'primevue/tag'
import Textarea from 'primevue/textarea'

type Employee = {
  id: string
  name: string
}

type ExpenseCategory = 'Travel' | 'Meals' | 'Gas' | 'Supplies' | 'Other'

type ExpenseItemForm = {
  category: ExpenseCategory
  amount: number | null
  description: string
  expenseDate: Date | null
}

type ExpenseClaim = {
  id: string
  employeeId: string
  employeeName: string
  title: string
  itemCount: number
  totalAmount: number
  submittedAt: string
  status: 'Submitted'
}

const employees: Employee[] = [
  { id: 'EMP-1001', name: 'Lina Haddad' },
  { id: 'EMP-1002', name: 'Omar Nasser' },
  { id: 'EMP-1003', name: 'Sara Mansour' },
  { id: 'EMP-1004', name: 'Adam Saleh' },
  { id: 'EMP-1005', name: 'Nour Khalil' },
  { id: 'EMP-1006', name: 'Maya Taha' },
  { id: 'EMP-1007', name: 'Yousef Darwish' },
  { id: 'EMP-1008', name: 'Rama Awad' },
  { id: 'EMP-1009', name: 'Karim Saad' },
  { id: 'EMP-1010', name: 'Hana Qasem' },
]

const categories: ExpenseCategory[] = ['Travel', 'Meals', 'Gas', 'Supplies', 'Other']
const createDialogVisible = ref(false)
const selectedEmployee = ref<Employee | null>(null)
const claimTitle = ref('')
const expenseItems = ref<ExpenseItemForm[]>([createExpenseItem()])
const submittedClaims = ref<ExpenseClaim[]>([
  {
    id: 'CLM-2026-001',
    employeeId: 'EMP-1002',
    employeeName: 'Omar Nasser',
    title: 'Client visit expenses',
    itemCount: 3,
    totalAmount: 412.5,
    submittedAt: 'Jun 18, 2026',
    status: 'Submitted',
  },
  {
    id: 'CLM-2026-002',
    employeeId: 'EMP-1005',
    employeeName: 'Nour Khalil',
    title: 'Office supplies refill',
    itemCount: 2,
    totalAmount: 178.2,
    submittedAt: 'Jun 17, 2026',
    status: 'Submitted',
  },
])

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
  if (expenseItems.value.length === 1) {
    return
  }

  expenseItems.value.splice(index, 1)
}

function openCreateDialog() {
  createDialogVisible.value = true
}

function resetCreateForm() {
  selectedEmployee.value = null
  claimTitle.value = ''
  expenseItems.value = [createExpenseItem()]
}

function submitClaim() {
  if (!selectedEmployee.value || !canSubmitClaim.value) {
    return
  }

  submittedClaims.value.unshift({
    id: `CLM-2026-${String(submittedClaims.value.length + 1).padStart(3, '0')}`,
    employeeId: selectedEmployee.value.id,
    employeeName: selectedEmployee.value.name,
    title: claimTitle.value.trim(),
    itemCount: expenseItems.value.length,
    totalAmount: claimTotal.value,
    submittedAt: new Intl.DateTimeFormat('en', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    }).format(new Date()),
    status: 'Submitted',
  })

  createDialogVisible.value = false
  resetCreateForm()
}

function formatCurrency(value: number) {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(value)
}
</script>

<template>
  <section class="employee-expenses-page">
    <div class="expense-page-toolbar">
      <div>
        <h2>Expense Claims</h2>
        <p>Employees can submit claim titles with one or more expense items.</p>
      </div>

      <Button label="Create Claim" icon="pi pi-plus" @click="openCreateDialog" />
    </div>

    <Card class="claims-table-card">
      <template #title>Submitted Claims</template>
      <template #content>
        <DataTable
          :value="submittedClaims"
          data-key="id"
          striped-rows
          responsive-layout="scroll"
          class="claims-table"
        >
          <Column field="id" header="Claim ID" />
          <Column field="employeeName" header="Employee" />
          <Column field="employeeId" header="Employee ID" />
          <Column field="title" header="Title" />
          <Column field="itemCount" header="Items" />
          <Column header="Total">
            <template #body="{ data }">
              {{ formatCurrency(data.totalAmount) }}
            </template>
          </Column>
          <Column field="submittedAt" header="Submitted" />
          <Column header="Status">
            <template #body="{ data }">
              <Tag :value="data.status" severity="info" />
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>

    <Dialog
      v-model:visible="createDialogVisible"
      modal
      header="Create Expense Claim"
      class="create-claim-dialog"
      :draggable="false"
      @hide="resetCreateForm"
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
                  <small>{{ option.id }}</small>
                </div>
              </template>
              <template #value="{ value, placeholder }">
                <span v-if="value">{{ value.name }} - {{ value.id }}</span>
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
      </div>

      <template #footer>
        <Button label="Cancel" severity="secondary" outlined @click="createDialogVisible = false" />
        <Button label="Submit Claim" icon="pi pi-send" :disabled="!canSubmitClaim" @click="submitClaim" />
      </template>
    </Dialog>
  </section>
</template>

<style scoped>

</style>
