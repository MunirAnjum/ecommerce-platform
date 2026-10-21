<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  status: string | number
  type?: 'order' | 'stock'
}>()

const statusNumber = computed(() => {
  if (typeof props.status === 'number') return props.status
  const parsed = parseInt(props.status, 10)
  return isNaN(parsed) ? props.status : parsed
})

const badgeConfig = computed(() => {
  if (props.type === 'stock') {
    const qty = typeof props.status === 'number' ? props.status : parseInt(props.status as string, 10)
    if (isNaN(qty) || qty <= 0) {
      return { label: 'Out of Stock', classes: 'bg-red-50 text-red-700 border-red-200' }
    }
    if (qty <= 5) {
      return { label: `Low Stock (${qty})`, classes: 'bg-amber-50 text-amber-800 border-amber-200' }
    }
    return { label: `In Stock (${qty})`, classes: 'bg-emerald-50 text-emerald-800 border-emerald-200' }
  }

  // Order status mappings
  switch (statusNumber.value) {
    case 1:
    case 'Pending':
      return { label: 'Pending', classes: 'bg-amber-50 text-amber-800 border-amber-200' }
    case 2:
    case 'Confirmed':
      return { label: 'Confirmed', classes: 'bg-blue-50 text-blue-800 border-blue-200' }
    case 3:
    case 'Processing':
      return { label: 'Processing', classes: 'bg-indigo-50 text-indigo-800 border-indigo-200' }
    case 4:
    case 'Shipped':
      return { label: 'Shipped', classes: 'bg-purple-50 text-purple-800 border-purple-200' }
    case 5:
    case 'Delivered':
      return { label: 'Delivered', classes: 'bg-emerald-50 text-emerald-800 border-emerald-200' }
    case 6:
    case 'Cancelled':
      return { label: 'Cancelled', classes: 'bg-rose-50 text-rose-800 border-rose-200' }
    default:
      return { label: String(props.status), classes: 'bg-stone-100 text-stone-700 border-stone-200' }
  }
})
</script>

<template>
  <span
    class="inline-flex items-center gap-1.5 rounded-full border px-2.5 py-0.5 text-xs font-semibold tracking-wide"
    :class="badgeConfig.classes"
  >
    <span class="h-1.5 w-1.5 rounded-full bg-current opacity-75"></span>
    {{ badgeConfig.label }}
  </span>
</template>
