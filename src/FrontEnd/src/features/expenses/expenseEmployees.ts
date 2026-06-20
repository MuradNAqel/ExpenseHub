export type Employee = {
  id: number
  code: string
  name: string
}

export const employees: Employee[] = [
  { id: 1001, code: 'EMP-1001', name: 'Lina Haddad' },
  { id: 1002, code: 'EMP-1002', name: 'Omar Nasser' },
  { id: 1003, code: 'EMP-1003', name: 'Sara Mansour' },
  { id: 1004, code: 'EMP-1004', name: 'Adam Saleh' },
  { id: 1005, code: 'EMP-1005', name: 'Nour Khalil' },
  { id: 1006, code: 'EMP-1006', name: 'Maya Taha' },
  { id: 1007, code: 'EMP-1007', name: 'Yousef Darwish' },
  { id: 1008, code: 'EMP-1008', name: 'Rama Awad' },
  { id: 1009, code: 'EMP-1009', name: 'Karim Saad' },
  { id: 1010, code: 'EMP-1010', name: 'Hana Qasem' },
]

export function getEmployeeCode(employeeId: number) {
  return employees.find((employee) => employee.id === employeeId)?.code ?? String(employeeId)
}

export function getEmployeeName(employeeId: number) {
  return employees.find((employee) => employee.id === employeeId)?.name ?? `Employee ${employeeId}`
}
