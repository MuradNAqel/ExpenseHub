<script setup lang="ts">
import { computed, ref } from 'vue'
import Button from 'primevue/button'
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Dialog from 'primevue/dialog'
import Dropdown from 'primevue/dropdown'
import InputText from 'primevue/inputtext'
import Tag from 'primevue/tag'

type ExpenseStatus = 'Pending' | 'Approved' | 'Rejected'

type ExpenseItem = {
  category: string
  amount: number
  description: string
  expenseDate: string
}

type ExpenseClaim = {
  id: string
  employeeId: string
  employeeName: string
  title: string
  submittedAt: string
  totalAmount: number
  status: ExpenseStatus
  items: ExpenseItem[]
}

const statusOptions: Array<ExpenseStatus | 'All'> = ['All', 'Pending', 'Approved', 'Rejected']
const searchTerm = ref('')
const selectedStatus = ref<ExpenseStatus | 'All'>('All')
const detailsVisible = ref(false)
const selectedClaim = ref<ExpenseClaim | null>(null)

const expenseClaims = ref<ExpenseClaim[]>([
  {
    id: 'CLM-2026-001',
    employeeId: 'EMP-1001',
    employeeName: 'Lina Haddad',
    title: 'Sales trip to Aqaba',
    submittedAt: 'Jun 18, 2026',
    totalAmount: 1240,
    status: 'Pending',
    items: [
      {
        category: 'Travel',
        amount: 890,
        description: 'Hotel and intercity transport',
        expenseDate: '2026-06-16',
      },
      {
        category: 'Meals',
        amount: 350,
        description: 'Client meals',
        expenseDate: '2026-06-17',
      },
    ],
  },
  {
    id: 'CLM-2026-002',
    employeeId: 'EMP-1004',
    employeeName: 'Adam Saleh',
    title: 'Office equipment purchase',
    submittedAt: 'Jun 17, 2026',
    totalAmount: 475.75,
    status: 'Approved',
    items: [
      {
        category: 'Supplies',
        amount: 475.75,
        description: 'Keyboards, docking station, and cables',
        expenseDate: '2026-06-15',
      },
    ],
  },
  {
    id: 'CLM-2026-003',
    employeeId: 'EMP-1006',
    employeeName: 'Maya Taha',
    title: 'Weekly transport reimbursement',
    submittedAt: 'Jun 16, 2026',
    totalAmount: 68.4,
    status: 'Pending',
    items: [
      {
        category: 'Gas',
        amount: 42,
        description: 'Fuel refill',
        expenseDate: '2026-06-14',
      },
      {
        category: 'Travel',
        amount: 26.4,
        description: 'Parking and tolls',
        expenseDate: '2026-06-14',
      },
    ],
  },
  {
    id: 'CLM-2026-004',
    employeeId: 'EMP-1008',
    employeeName: 'Rama Awad',
    title: 'Team lunch',
    submittedAt: 'Jun 15, 2026',
    totalAmount: 215,
    status: 'Rejected',
    items: [
      {
        category: 'Meals',
        amount: 215,
        description: 'Team lunch without attached receipt',
        expenseDate: '2026-06-13',
      },
    ],
  },
  {
    id: 'CLM-2026-005',
    employeeId: 'EMP-1010',
    employeeName: 'Hana Qasem',
    title: 'Conference registration',
    submittedAt: 'Jun 14, 2026',
    totalAmount: 650,
    status: 'Pending',
    items: [
      {
        category: 'Other',
        amount: 650,
        description: 'Finance operations conference pass',
        expenseDate: '2026-06-12',
      },
    ],
  },
])

const filteredClaims = computed(() => {
  const normalizedSearch = searchTerm.value.trim().toLowerCase()

  return expenseClaims.value.filter((claim) => {
    const matchesStatus = selectedStatus.value === 'All' || claim.status === selectedStatus.value
    const matchesSearch =
      normalizedSearch.length === 0 ||
      claim.id.toLowerCase().includes(normalizedSearch) ||
      claim.employeeId.toLowerCase().includes(normalizedSearch) ||
      claim.employeeName.toLowerCase().includes(normalizedSearch) ||
      claim.title.toLowerCase().includes(normalizedSearch)

    return matchesStatus && matchesSearch
  })
})

function openDetails(claim: ExpenseClaim) {
  selectedClaim.value = claim
  detailsVisible.value = true
}

function approveClaim(claim: ExpenseClaim) {
  claim.status = 'Approved'
}

function rejectClaim(claim: ExpenseClaim) {
  claim.status = 'Rejected'
}

function getStatusSeverity(status: ExpenseStatus) {
  switch (status) {
    case 'Approved':
      return 'success'
    case 'Rejected':
      return 'danger'
    default:
      return 'warn'
  }
}

function formatCurrency(value: number) {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(value)
}
</script>

<template>
  <section class="manager-expenses-page">
    <Card class="manager-expenses-card">
      <template #title>Expense Claims</template>
      <template #content>
        <div class="manager-expenses-toolbar">
          <span class="search-field">
            <i class="pi pi-search" />
            <InputText v-model="searchTerm" placeholder="Search claims" />
          </span>

          <Dropdown
            v-model="selectedStatus"
            :options="statusOptions"
            class="status-filter"
            placeholder="Filter status"
          />
        </div>

        <DataTable
          :value="filteredClaims"
          data-key="id"
          striped-rows
          responsive-layout="scroll"
          class="manager-expenses-table"
        >
          <Column field="id" header="Claim ID" sortable />
          <Column field="employeeName" header="Employee" sortable />
          <Column field="employeeId" header="Employee ID" />
          <Column field="title" header="Title" />
          <Column field="submittedAt" header="Submitted" sortable />
          <Column header="Amount" sortable sort-field="totalAmount">
            <template #body="{ data }">
              {{ formatCurrency(data.totalAmount) }}
            </template>
          </Column>
          <Column header="Status" sortable sort-field="status">
            <template #body="{ data }">
              <Tag :value="data.status" :severity="getStatusSeverity(data.status)" />
            </template>
          </Column>
          <Column header="Actions">
            <template #body="{ data }">
              <div class="row-actions">
                <Button
                  icon="pi pi-eye"
                  text
                  rounded
                  aria-label="View expense claim"
                  title="View"
                  class="view-action"
                  @click="openDetails(data)"
                />
                <Button
                  icon="pi pi-check"
                  text
                  rounded
                  severity="success"
                  aria-label="Approve expense claim"
                  title="Approve"
                  :disabled="data.status === 'Approved'"
                  @click="approveClaim(data)"
                />
                <Button
                  icon="pi pi-times"
                  text
                  rounded
                  severity="danger"
                  aria-label="Reject expense claim"
                  title="Reject"
                  :disabled="data.status === 'Rejected'"
                  @click="rejectClaim(data)"
                />
              </div>
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>

    <Dialog
      v-model:visible="detailsVisible"
      modal
      header="Expense Claim Details"
      class="claim-details-dialog"
      :draggable="false"
    >
      <div v-if="selectedClaim" class="claim-details">
        <div class="claim-details-summary">
          <div>
            <span>Claim</span>
            <strong>{{ selectedClaim.id }}</strong>
          </div>
          <div>
            <span>Employee</span>
            <strong>{{ selectedClaim.employeeName }}</strong>
            <small>{{ selectedClaim.employeeId }}</small>
          </div>
          <div>
            <span>Total</span>
            <strong>{{ formatCurrency(selectedClaim.totalAmount) }}</strong>
          </div>
          <div>
            <span>Status</span>
            <Tag :value="selectedClaim.status" :severity="getStatusSeverity(selectedClaim.status)" />
          </div>
        </div>

        <div class="claim-title-block">
          <span>Title</span>
          <strong>{{ selectedClaim.title }}</strong>
        </div>

        <DataTable
          :value="selectedClaim.items"
          data-key="description"
          responsive-layout="scroll"
          class="claim-items-table"
        >
          <Column field="category" header="Category" />
          <Column field="expenseDate" header="Expense Date" />
          <Column field="description" header="Description" />
          <Column header="Amount">
            <template #body="{ data }">
              {{ formatCurrency(data.amount) }}
            </template>
          </Column>
        </DataTable>
      </div>

      <template #footer>
        <Button label="Close" severity="secondary" outlined @click="detailsVisible = false" />
      </template>
    </Dialog>
  </section>
</template>

<style src="./ManagerExpensesPage.css"></style>
