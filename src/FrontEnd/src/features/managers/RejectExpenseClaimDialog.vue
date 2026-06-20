<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import Textarea from 'primevue/textarea'

const props = defineProps<{
  visible: boolean
  isSaving: boolean
  errorMessage: string
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  reject: [reason: string]
}>()

const reason = ref('')

const dialogVisible = computed({
  get: () => props.visible,
  set: (value) => emit('update:visible', value),
})

watch(
  () => props.visible,
  (visible) => {
    if (visible) {
      reason.value = ''
    }
  },
)

function rejectClaim() {
  const trimmedReason = reason.value.trim()

  if (trimmedReason.length > 0) {
    emit('reject', trimmedReason)
  }
}
</script>

<template>
  <Dialog
    v-model:visible="dialogVisible"
    modal
    header="Reject Expense Claim"
    class="reject-claim-dialog"
    :draggable="false"
  >
    <label class="reject-reason-field">
      <span>Reason</span>
      <Textarea v-model="reason" rows="4" auto-resize placeholder="Reason for rejection" />
    </label>

    <p v-if="errorMessage" class="manager-error">{{ errorMessage }}</p>

    <template #footer>
      <Button label="Cancel" severity="secondary" outlined @click="dialogVisible = false" />
      <Button
        label="Reject Claim"
        icon="pi pi-times"
        severity="danger"
        :disabled="reason.trim().length === 0 || isSaving"
        :loading="isSaving"
        @click="rejectClaim"
      />
    </template>
  </Dialog>
</template>
