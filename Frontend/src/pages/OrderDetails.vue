
<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  cancelOrder,
  getOrderById,
  type OrderResponse,
} from '../services/orderService'

const route = useRoute()
const router = useRouter()

const order = ref<OrderResponse | null>(null)
const isLoading = ref(true)
const isCancelling = ref(false)
const errorMessage = ref('')
const cancelMessage = ref('')

const orderId = computed(() => String(route.params.id ?? ''))

const statusSteps = [
  'Pending',
  'Confirmed',
  'Processing',
  'Shipped',
  'Delivered',
]

function getStatusLabel(status: string | number): string {
  if (typeof status === 'number') {
    return (
      {
        1: 'Pending',
        2: 'Confirmed',
        3: 'Processing',
        4: 'Shipped',
        5: 'Delivered',
        6: 'Cancelled',
      }[status] ?? 'Unknown'
    )
  }

  return status
}

const currentStatus = computed(() =>
  order.value ? getStatusLabel(order.value.status) : '',
)

const currentStep = computed(() =>
  statusSteps.indexOf(currentStatus.value),
)

const canCancel = computed(() =>
  currentStatus.value === 'Pending'
)

function formatDate(date: string): string {
  return new Date(date).toLocaleString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

function formatPrice(amount: number): string {
  return new Intl.NumberFormat(undefined, {
    style: 'currency',
    currency: 'PKR',
  }).format(amount)
}

async function loadOrder() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    order.value = await getOrderById(orderId.value)
  } catch (error) {
    console.error('Failed to load order:', error)
    errorMessage.value =
      'Unable to load this order. It may not exist or you may not have access to it.'
  } finally {
    isLoading.value = false
  }
}

async function handleCancel() {
  if (!order.value || !canCancel.value || isCancelling.value) return

  const confirmed = window.confirm(
    'Are you sure you want to cancel this order?',
  )

  if (!confirmed) return

  isCancelling.value = true
  cancelMessage.value = ''

  try {
    order.value = await cancelOrder(order.value.id)
    cancelMessage.value = 'Your order was cancelled successfully.'
  } catch (error) {
    console.error('Failed to cancel order:', error)
    cancelMessage.value =
      'Unable to cancel this order. Please refresh and check its current status.'
  } finally {
    isCancelling.value = false
  }
}

onMounted(loadOrder)
</script>

<template>
  <main class="mx-auto max-w-5xl px-4 py-10 sm:px-6">
    <button
      type="button"
      class="mb-6 text-sm font-medium text-gray-600 hover:text-gray-900"
      @click="router.push({ name: 'my-orders' })"
    >
      ← Back to My Orders
    </button>

    <div v-if="isLoading" class="py-16 text-center text-gray-500">
      Loading order details...
    </div>

    <div
      v-else-if="errorMessage"
      class="rounded-xl border border-red-200 bg-red-50 p-6 text-red-700"
    >
      {{ errorMessage }}
      <button
        type="button"
        class="ml-2 font-semibold underline"
        @click="loadOrder"
      >
        Try again
      </button>
    </div>

    <template v-else-if="order">
      <header class="mb-8 flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">
            Order Details
          </h1>
          <p class="mt-2 break-all text-sm text-gray-500">
            Order #{{ order.id }}
          </p>
          <p class="mt-1 text-sm text-gray-500">
            Placed {{ formatDate(order.createdAt) }}
          </p>
        </div>

        <span
          class="rounded-full px-4 py-2 text-sm font-semibold"
          :class="{
            'bg-yellow-100 text-yellow-800': currentStatus === 'Pending',
            'bg-blue-100 text-blue-800':
              ['Confirmed', 'Processing'].includes(currentStatus),
            'bg-purple-100 text-purple-800': currentStatus === 'Shipped',
            'bg-green-100 text-green-800': currentStatus === 'Delivered',
            'bg-red-100 text-red-800': currentStatus === 'Cancelled',
          }"
        >
          {{ currentStatus }}
        </span>
      </header>

      <div
        v-if="cancelMessage"
        class="mb-6 rounded-lg bg-gray-100 p-4 text-sm text-gray-700"
      >
        {{ cancelMessage }}
      </div>

      <section class="mb-6 rounded-2xl border border-gray-200 bg-white p-6">
        <h2 class="text-lg font-semibold text-gray-900">
          Order Tracking
        </h2>

        <div
          v-if="currentStatus === 'Cancelled'"
          class="mt-5 rounded-lg bg-red-50 p-4 text-red-700"
        >
          This order has been cancelled.
        </div>

        <div v-else class="mt-6 space-y-5">
          <div
            v-for="(step, index) in statusSteps"
            :key="step"
            class="flex items-start gap-3"
          >
            <div
              class="flex h-8 w-8 shrink-0 items-center justify-center rounded-full text-sm font-semibold"
              :class="{
                'bg-green-600 text-white': index <= currentStep,
                'bg-gray-100 text-gray-500': index > currentStep,
              }"
            >
              <span v-if="index < currentStep">✓</span>
              <span v-else>{{ index + 1 }}</span>
            </div>

            <div class="pt-1">
              <p
                class="font-medium"
                :class="index <= currentStep ? 'text-gray-900' : 'text-gray-400'"
              >
                {{ step }}
              </p>
              <p
                v-if="index === currentStep"
                class="mt-1 text-sm text-gray-500"
              >
                Current order status
              </p>
            </div>
          </div>
        </div>
      </section>

      <section class="mb-6 rounded-2xl border border-gray-200 bg-white p-6">
        <h2 class="mb-5 text-lg font-semibold text-gray-900">
          Items Ordered
        </h2>

        <div
          v-for="item in order.items"
          :key="item.id"
          class="flex flex-wrap items-center justify-between gap-4 border-b border-gray-100 py-4 last:border-b-0"
        >
          <div class="min-w-0 flex-1">
            <p class="font-medium text-gray-900">
              {{ item.productName }}
            </p>
            <p class="mt-1 text-sm text-gray-500">
              {{ formatPrice(item.unitPrice) }} × {{ item.quantity }}
            </p>
          </div>

          <p class="font-semibold text-gray-900">
            {{ formatPrice(item.totalPrice) }}
          </p>
        </div>

        <div class="mt-5 flex justify-between border-t border-gray-200 pt-5">
          <span class="font-semibold text-gray-700">Order Total</span>
          <span class="text-xl font-bold text-gray-900">
            {{ formatPrice(order.totalAmount) }}
          </span>
        </div>
      </section>

      <div class="flex flex-wrap gap-3">
        <button
          v-if="canCancel"
          type="button"
          :disabled="isCancelling"
          class="rounded-lg border border-red-300 px-5 py-3 font-medium text-red-700 hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-50"
          @click="handleCancel"
        >
          {{ isCancelling ? 'Cancelling...' : 'Cancel Order' }}
        </button>

        <button
          type="button"
          class="rounded-lg bg-gray-900 px-5 py-3 font-medium text-white hover:bg-gray-800"
          @click="router.push({ name: 'my-orders' })"
        >
          Back to My Orders
        </button>
      </div>
    </template>
  </main>
</template>
