<script setup lang="ts">
import { onMounted, ref } from 'vue'
import Button from 'primevue/button'
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Tag from 'primevue/tag'
import CreateExpenseClaimDialog from './CreateExpenseClaimDialog.vue'
import ExpenseClaimDetailsDialog from './ExpenseClaimDetailsDialog.vue'
import { formatCurrency, getStatusSeverity } from './expenseFormatters'
import { getExpenseApiErrorMessage, getExpenseClaims } from './expensesApi'
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

onMounted(loadExpenseClaims)

async function loadExpenseClaims() {
  isLoadingClaims.value = true
  claimsError.value = ''

  try {
    const page = Math.floor(firstRecordIndex.value / pageSize.value) + 1
    const result = await getExpenseClaims({ page, pageSize: pageSize.value })

    submittedClaims.value = result.items.map(mapExpenseClaimSummary)
    totalRecords.value = result.totalCount
  } catch (error) {
    claimsError.value = getExpenseApiErrorMessage(error)
  } finally {
    isLoadingClaims.value = false
  }
}

function openClaimDetails(claim: ExpenseClaimRow) {
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
          :value="submittedClaims"
          data-key="id"
          striped-rows
          responsive-layout="scroll"
          class="claims-table"
          selection-mode="single"
          :loading="isLoadingClaims"
          paginator
          lazy
          :rows="pageSize"
          :first="firstRecordIndex"
          :total-records="totalRecords"
          :rows-per-page-options="[10]"
          current-page-report-template="{first} to {last} of {totalRecords}"
          paginator-template="FirstPageLink PrevPageLink CurrentPageReport NextPageLink LastPageLink"
          @row-click="openClaimDetails($event.data)"
          @page="changePage"
        >
          <Column field="id" header="Claim ID" />
          <Column field="employeeName" header="Employee" />
          <Column field="employeeId" header="Employee ID" />
          <Column field="title" header="Title" />
          <Column header="Items">
            <template #body="{ data }">
              {{ data.itemCount ?? 'View' }}
            </template>
          </Column>
          <Column header="Total">
            <template #body="{ data }">
              {{ formatCurrency(data.totalAmount) }}
            </template>
          </Column>
          <Column field="submittedAt" header="Submitted" />
          <Column header="Status">
            <template #body="{ data }">
              <Tag :value="data.status" :severity="getStatusSeverity(data.status)" />
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
