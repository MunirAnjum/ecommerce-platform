<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  ArrowLeft,
  Calendar,
  Check,
  XCircle,
  AlertCircle,
} from 'lucide-vue-next'
import {
  cancelOrder,
  getOrderById,
  type OrderResponse,
} from '../services/orderService'
import { formatPrice, formatDateTime } from '../utils/formatters'
import StatusBadge from '../components/StatusBadge.vue'
import Modal from '../components/Modal.vue'

const route = useRoute()
const router = useRouter()

const order = ref<OrderResponse | null>(null)
const isLoading = ref(true)
const isCancelling = ref(false)
const showCancelModal = ref(false)
const errorMessage = ref('')
const cancelMessage = ref('')

const orderId = computed(() => String(route.params.id ?? ''))

const statusSteps = [
  { label: 'Pending', desc: 'Order received & registered' },
  { label: 'Confirmed', desc: 'Payment verified' },
  { label: 'Processing', desc: 'Packed at mountain warehouse' },
  { label: 'Shipped', desc: 'Handed to courier partner' },
  { label: 'Delivered', desc: 'Delivered to recipient' },
]

function getStatusCode(status: string | number): number {
  if (typeof status === 'number') return status
  const parsed = parseInt(status, 10)
  if (!isNaN(parsed)) return parsed
  const map: Record<string, number> = {
    Pending: 1,
    Confirmed: 2,
    Processing: 3,
    Shipped: 4,
    Delivered: 5,
    Cancelled: 6,
  }
  return map[status] ?? 0
}

const currentStatusCode = computed(() =>
  order.value ? getStatusCode(order.value.status) : 0
)

const currentStepIndex = computed(() => {
  return currentStatusCode.value - 1
})

const canCancel = computed(() =>
  currentStatusCode.value === 1 // Only Pending orders can be cancelled
)

async function loadOrder() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    order.value = await getOrderById(orderId.value)
  } catch (error) {
    console.error('Failed to load order:', error)
    errorMessage.value =
      'Unable to load this order. It may not exist or you may lack access permissions.'
  } finally {
    isLoading.value = false
  }
}

async function handleCancelConfirm() {
  if (!order.value || !canCancel.value || isCancelling.value) return

  isCancelling.value = true
  cancelMessage.value = ''

  try {
    const updated = await cancelOrder(order.value.id)
    order.value = updated
    cancelMessage.value = 'Order was successfully cancelled.'
    showCancelModal.value = false
  } catch (error) {
    console.error('Failed to cancel order:', error)
    cancelMessage.value = 'Could not cancel the order. Please refresh and check its current status.'
  } finally {
    isCancelling.value = false
  }
}

onMounted(loadOrder)
</script>

<template>
  <div class="mx-auto max-w-5xl px-4 py-8 sm:px-6 lg:px-8">
    <!-- Back button -->
    <button
      type="button"
      class="mb-6 inline-flex items-center gap-2 text-xs font-bold text-stone-600 hover:text-stone-900 transition"
      @click="router.push('/my-orders')"
    >
      <ArrowLeft class="h-4 w-4" />
      <span>Back to My Orders</span>
    </button>

    <!-- Loading State -->
    <div v-if="isLoading" class="rounded-3xl border border-stone-200 bg-white p-12 text-center text-stone-500">
      <span class="h-6 w-6 inline-block animate-spin rounded-full border-2 border-stone-600 border-t-transparent" />
      <p class="mt-3 text-sm font-medium">Loading order details...</p>
    </div>

    <!-- Error State -->
    <div
      v-else-if="errorMessage"
      class="rounded-3xl border border-rose-200 bg-rose-50 p-8 text-center text-rose-800"
    >
      <AlertCircle class="mx-auto h-10 w-10 text-rose-600" />
      <p class="mt-3 font-semibold">{{ errorMessage }}</p>
      <button
        type="button"
        class="mt-4 rounded-xl bg-rose-800 px-5 py-2.5 text-xs font-bold text-white hover:bg-rose-900"
        @click="loadOrder"
      >
        Try Again
      </button>
    </div>

    <template v-else-if="order">
      <!-- Order Header Card -->
      <div class="rounded-3xl border border-stone-200 bg-white p-6 shadow-xs sm:p-8">
        <div class="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-4">
          <div>
            <span class="text-xs font-bold uppercase tracking-widest text-amber-800">Order Summary</span>
            <h1 class="mt-1 font-serif text-2xl font-extrabold text-stone-900 sm:text-3xl">
              Order Receipt
            </h1>
            <p class="mt-2 text-xs text-stone-500 break-all font-mono">
              #{{ order.id }}
            </p>
            <div class="mt-2 flex items-center gap-2 text-xs text-stone-500">
              <Calendar class="h-3.5 w-3.5 text-stone-400" />
              <span>Placed on {{ formatDateTime(order.createdAt) }}</span>
            </div>
          </div>

          <div class="flex flex-col items-start sm:items-end gap-2">
            <StatusBadge :status="order.status" type="order" />
            <span class="text-2xl font-extrabold text-stone-900">
              {{ formatPrice(order.totalAmount) }}
            </span>
          </div>
        </div>

        <!-- Cancellation Notification -->
        <div
          v-if="cancelMessage"
          class="mt-6 rounded-2xl bg-stone-100 border border-stone-200 p-4 text-xs font-bold text-stone-800"
        >
          {{ cancelMessage }}
        </div>
      </div>

      <!-- Live Order Tracking Stepper -->
      <div class="mt-6 rounded-3xl border border-stone-200 bg-white p-6 shadow-xs sm:p-8">
        <h2 class="font-serif text-xl font-bold text-stone-900">
          Delivery Status
        </h2>

        <!-- Cancelled State Notice -->
        <div
          v-if="currentStatusCode === 6"
          class="mt-6 flex items-center gap-3 rounded-2xl border border-rose-200 bg-rose-50 p-4 text-xs font-bold text-rose-800"
        >
          <XCircle class="h-5 w-5 shrink-0 text-rose-600" />
          <span>This order has been cancelled. If any payment was captured, refund processing will commence.</span>
        </div>

        <!-- Linear Progress Stepper -->
        <div v-else class="mt-8 space-y-6">
          <div class="relative pl-8 space-y-8 before:absolute before:left-3.5 before:top-3 before:bottom-3 before:w-0.5 before:bg-stone-200">
            <div
              v-for="(step, idx) in statusSteps"
              :key="step.label"
              class="relative flex items-start gap-4"
            >
              <!-- Indicator circle -->
              <div
                class="absolute -left-8 flex h-7 w-7 items-center justify-center rounded-full text-xs font-bold transition"
                :class="[
                  idx <= currentStepIndex
                    ? 'bg-emerald-800 text-white shadow-xs'
                    : 'bg-stone-100 text-stone-400 border border-stone-200'
                ]"
              >
                <Check v-if="idx < currentStepIndex" class="h-4 w-4" />
                <span v-else>{{ idx + 1 }}</span>
              </div>

              <!-- Step details -->
              <div>
                <h4
                  class="text-sm font-bold"
                  :class="idx <= currentStepIndex ? 'text-stone-900' : 'text-stone-400'"
                >
                  {{ step.label }}
                </h4>
                <p class="text-xs text-stone-500 mt-0.5">
                  {{ step.desc }}
                </p>
                <span
                  v-if="idx === currentStepIndex"
                  class="mt-1 inline-block text-2xs font-bold text-emerald-800 uppercase tracking-wider"
                >
                  Current Status
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Items Ordered Breakdown -->
      <div class="mt-6 rounded-3xl border border-stone-200 bg-white p-6 shadow-xs sm:p-8">
        <h2 class="font-serif text-xl font-bold text-stone-900 border-b border-stone-100 pb-4">
          Items in this Order ({{ order.items.length }})
        </h2>

        <div class="divide-y divide-stone-100">
          <div
            v-for="item in order.items"
            :key="item.id"
            class="flex items-center justify-between gap-4 py-4"
          >
            <div>
              <p class="font-bold text-stone-900 text-sm sm:text-base">{{ item.productName }}</p>
              <p class="text-xs text-stone-500 mt-0.5">
                {{ formatPrice(item.unitPrice) }} × {{ item.quantity }}
              </p>
            </div>

            <span class="font-extrabold text-stone-900 text-sm sm:text-base">
              {{ formatPrice(item.totalPrice) }}
            </span>
          </div>
        </div>

        <div class="mt-6 flex items-baseline justify-between border-t border-stone-200 pt-6">
          <span class="font-bold text-stone-900">Total Charged</span>
          <span class="text-2xl font-extrabold text-stone-900">
            {{ formatPrice(order.totalAmount) }}
          </span>
        </div>
      </div>

      <!-- Order Actions -->
      <div class="mt-6 flex flex-wrap items-center justify-between gap-4">
        <button
          v-if="canCancel"
          type="button"
          class="rounded-2xl border border-rose-300 bg-white px-5 py-2.5 text-xs font-bold text-rose-700 hover:bg-rose-50 transition"
          @click="showCancelModal = true"
        >
          Cancel Order
        </button>

        <router-link
          to="/products"
          class="inline-flex items-center gap-2 rounded-2xl bg-stone-900 px-6 py-3 text-xs font-bold text-white hover:bg-stone-800 transition"
        >
          <span>Continue Shopping</span>
        </router-link>
      </div>

      <!-- Cancel Confirmation Modal -->
      <Modal
        :is-open="showCancelModal"
        title="Cancel Order Confirmation"
        @close="showCancelModal = false"
      >
        <p class="text-sm text-stone-600 leading-relaxed">
          Are you sure you want to cancel order <strong>#{{ order.id }}</strong>? This action will immediately release reserved inventory and cannot be undone.
        </p>

        <template #footer>
          <div class="flex justify-end gap-3">
            <button
              type="button"
              class="rounded-xl border border-stone-200 px-4 py-2 text-xs font-semibold text-stone-700 hover:bg-stone-100"
              @click="showCancelModal = false"
            >
              Keep Order
            </button>
            <button
              type="button"
              class="rounded-xl bg-rose-700 px-4 py-2 text-xs font-bold text-white hover:bg-rose-800 disabled:opacity-50"
              :disabled="isCancelling"
              @click="handleCancelConfirm"
            >
              {{ isCancelling ? 'Cancelling...' : 'Confirm Cancellation' }}
            </button>
          </div>
        </template>
      </Modal>
    </template>
  </div>
</template>
