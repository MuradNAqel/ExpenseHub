import { createRouter, createWebHistory } from 'vue-router'
import ManagerDashboardPage from '@/features/dashboard/ManagerDashboardPage.vue'
import EmployeeExpensesPage from '@/features/expenses/EmployeeExpensesPage.vue'
import ManagerExpensesPage from '@/features/managers/ManagerExpensesPage.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/manager/dashboard',
    },
    {
      path: '/manager/dashboard',
      name: 'manager-dashboard',
      component: ManagerDashboardPage,
      meta: {
        title: 'Dashboard',
        description: 'Manager expense overview',
      },
    },
    {
      path: '/manager/expenses',
      name: 'manager-expenses',
      component: ManagerExpensesPage,
      meta: {
        title: 'Manager Expenses',
        description: 'Review and approve employee expense claims',
      },
    },
    {
      path: '/Employee/expenses',
      name: 'Employee/expenses',
      component: EmployeeExpensesPage,
      meta: {
        title: 'Employee Expenses',
        description: 'Create and track expense claims',
      },
    },
  ],
})

export default router
