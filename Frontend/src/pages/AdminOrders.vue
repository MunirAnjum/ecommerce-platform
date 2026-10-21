<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import {
  RefreshCw,
  Search,
  CheckCircle2,
  AlertCircle,
} from 'lucide-vue-next'
import {
  getAllOrders,
  updateOrderStatus,
  type OrderResponse,
} from '../services/orderService'
import { formatPrice, formatDateTime } from '../utils/formatters'
import StatusBadge from '../components/StatusBadge.vue'
import Modal from '../components/Modal.vue'

const orders = ref<OrderResponse[]>([])
const isLoading = ref(true)
const errorMessage = ref('')
const successMessage = ref('')
const statusFilter = ref('All')
const searchQuery = ref('')
const selectedStatuses = ref<Record<string, number>>({})
const updatingOrderId = ref<string | null>(null)

// Modal confirmation state
const isConfirmModalOpen = ref(false)
const pendingStatusChange = ref<{ order: OrderResponse; newStatus: number } | null>(null)

const statusNames: Record<number, string> = {
  1: 'Pending',
  2: 'Confirmed',
  3: 'Processing',
  4: 'Shipped',
  5: 'Delivered',
  6: 'Cancelled',
}

const statusValues: Record<string, number> = {
  Pending: 1,
  Confirmed: 2,
  Processing: 3,
  Shipped: 4,
  Delivered: 5,
  Cancelled: 6,
}

function statusCode(status: string | number): number {
  if (typeof status === 'number') return status
  const parsed = parseInt(status, 10)
  if (!isNaN(parsed)) return parsed
  return statusValues[status] ?? 0
}

function statusLabel(status: string | number): string {
  return statusNames[statusCode(status)] ?? String(status)
}

function allowedNextStatuses(status: string | number): number[] {
  switch (statusCode(status)) {
    case 1:
      return [2, 6] // Pending can transition to Confirmed or Cancelled
    case 2:
      return [3] // Confirmed can transition to Processing
    case 3:
      return [4] // Processing can transition to Shipped
    case 4:
      return [5] // Shipped can transition to Delivered
    default:
      return [] // Terminal states have no transitions
  }
}

const filteredOrders = computed(() => {
  let list = orders.value

  // Status Filter
  if (statusFilter.value !== 'All') {
    list = list.filter(order => statusLabel(order.status) === statusFilter.value)
  }

  // Search Filter
  const q = searchQuery.value.trim().toLowerCase()
  if (q) {
    list = list.filter(
      order =>
        order.id.toLowerCase().includes(q) ||
        order.userId.toLowerCase().includes(q) ||
        order.items.some(i => i.productName.toLowerCase().includes(q))
    )
  }

  return list
})

async function loadOrders() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    const list = await getAllOrders()
    orders.value = list
    for (const order of list) {
      selectedStatuses.value[order.id] = statusCode(order.status)
    }
  } catch (error) {
    console.error('Failed to load admin orders:', error)
    errorMessage.value = 'Unable to load orders. Ensure your session has active Administrator rights.'
  } finally {
    isLoading.value = false
  }
}

function promptStatusChange(order: OrderResponse) {
  const newStatus = selectedStatuses.value[order.id]
  const current = statusCode(order.status)

  if (newStatus === current || !allowedNextStatuses(current).includes(newStatus)) {
    return
  }

  pendingStatusChange.value = { order, newStatus }
  isConfirmModalOpen.value = true
}

async function confirmStatusUpdate() {
  if (!pendingStatusChange.value) return

  const { order, newStatus } = pendingStatusChange.value
  updatingOrderId.value = order.id
  errorMessage.value = ''
  successMessage.value = ''
  isConfirmModalOpen.value = false

  try {
    const updatedOrder = await updateOrderStatus(order.id, newStatus)
    const index = orders.value.findIndex(item => item.id === order.id)

    if (index !== -1) {
      orders.value[index] = updatedOrder
    }

    selectedStatuses.value[order.id] = statusCode(updatedOrder.status)
    successMessage.value = `Order #${order.id} status successfully transitioned to ${statusLabel(updatedOrder.status)}.`
  } catch (error) {
    console.error('Failed to update status:', error)
    selectedStatuses.value[order.id] = statusCode(order.status)
    errorMessage.value = 'Could not update order status. Please verify allowed status transitions and try again.'
  } finally {
    updatingOrderId.value = null
    pendingStatusChange.value = null
  }
}

function cancelStatusPrompt() {
  if (pendingStatusChange.value) {
    const { order } = pendingStatusChange.value
    selectedStatuses.value[order.id] = statusCode(order.status)
  }
  isConfirmModalOpen.value = false
  pendingStatusChange.value = null
}

onMounted(loadOrders)
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <div>
        <h2 class="font-serif text-2xl font-bold text-stone-900 sm:text-3xl">
          Order Fulfillment Management
        </h2>
        <p class="mt-1 text-xs text-stone-500">
          Review customer orders, verify items, and advance status along the fulfillment lifecycle.
        </p>
      </div>

      <button
        type="button"
        class="inline-flex items-center gap-2 rounded-xl border border-stone-200 bg-white px-4 py-2 text-xs font-semibold text-stone-700 shadow-2xs hover:bg-stone-50 transition"
        :disabled="isLoading"
        @click="loadOrders"
      >
        <RefreshCw class="h-3.5 w-3.5" :class="{ 'animate-spin': isLoading }" />
        <span>Refresh Orders</span>
      </button>
    </div>

    <!-- Notifications -->
    <div
      v-if="errorMessage"
      class="flex items-center gap-2 rounded-2xl border border-rose-200 bg-rose-50 p-4 text-xs font-bold text-rose-800"
    >
      <AlertCircle class="h-4 w-4 shrink-0" />
      <span>{{ errorMessage }}</span>
    </div>

    <div
      v-if="successMessage"
      class="flex items-center gap-2 rounded-2xl border border-emerald-200 bg-emerald-50 p-4 text-xs font-bold text-emerald-800"
    >
      <CheckCircle2 class="h-4 w-4 shrink-0" />
      <span>{{ successMessage }}</span>
    </div>

    <!-- Filter and Search Bar -->
    <div class="flex flex-col sm:flex-row items-center justify-between gap-4 rounded-3xl border border-stone-200 bg-white p-4 shadow-xs">
      <!-- Search Input -->
      <div class="relative w-full sm:max-w-xs">
        <Search class="absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-stone-400" />
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Filter by Order ID or User..."
          class="w-full rounded-2xl border border-stone-200 bg-stone-50/50 pl-10 pr-4 py-2 text-xs text-stone-900 placeholder-stone-400 focus:border-amber-700 focus:bg-white focus:outline-none"
        />
      </div>

      <!-- Status Filter Tabs / Select -->
      <div class="flex items-center gap-2 w-full sm:w-auto overflow-x-auto pb-1 sm:pb-0">
        <span class="text-xs font-bold text-stone-400 shrink-0">Status:</span>
        <select
          v-model="statusFilter"
          class="rounded-xl border border-stone-200 bg-white px-3 py-1.5 text-xs font-semibold text-stone-800 focus:outline-none focus:border-amber-700"
        >
          <option value="All">All Statuses ({{ orders.length }})</option>
          <option value="Pending">Pending ({{ orders.filter(o => statusCode(o.status) === 1).length }})</option>
          <option value="Confirmed">Confirmed</option>
          <option value="Processing">Processing</option>
          <option value="Shipped">Shipped</option>
          <option value="Delivered">Delivered</option>
          <option value="Cancelled">Cancelled</option>
        </select>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading" class="rounded-3xl border border-stone-200 bg-white p-12 text-center text-stone-500">
      <span class="h-6 w-6 inline-block animate-spin rounded-full border-2 border-stone-600 border-t-transparent" />
      <p class="mt-3 text-sm font-medium">Fetching orders from Order Service...</p>
    </div>

    <!-- Empty Orders -->
    <div
      v-else-if="filteredOrders.length === 0"
      class="rounded-3xl border border-stone-200 bg-white p-12 text-center text-stone-500 shadow-xs"
    >
      <p class="text-sm">No orders match the current search or status filter.</p>
    </div>

    <!-- Orders Accordion List -->
    <div v-else class="space-y-4">
      <div
        v-for="order in filteredOrders"
        :key="order.id"
        class="rounded-3xl border border-stone-200 bg-white p-6 shadow-xs transition hover:border-stone-300"
      >
        <!-- Top summary row -->
        <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div class="min-w-0">
            <div class="flex items-center gap-3">
              <span class="font-mono text-sm font-bold text-stone-900 break-all">
                #{{ order.id }}
              </span>
              <StatusBadge :status="order.status" type="order" />
            </div>

            <p class="text-xs text-stone-500 mt-1">
              Customer: <span class="font-mono text-stone-700 font-semibold">{{ order.userId }}</span> • Placed {{ formatDateTime(order.createdAt) }}
            </p>
          </div>

          <div class="flex items-center gap-6">
            <div class="text-right">
              <span class="block text-2xs uppercase tracking-wider text-stone-400 font-bold">Total</span>
              <span class="text-base font-extrabold text-stone-900">{{ formatPrice(order.totalAmount) }}</span>
            </div>
          </div>
        </div>

        <!-- Items Breakdown -->
        <div class="mt-4 border-t border-stone-100 pt-4">
          <h4 class="text-xs font-bold uppercase tracking-wider text-stone-500 mb-2">Order Line Items</h4>
          <div class="divide-y divide-stone-50 rounded-2xl bg-stone-50/70 p-4 border border-stone-100 text-xs">
            <div
              v-for="item in order.items"
              :key="item.id"
              class="flex items-center justify-between py-2"
            >
              <div>
                <span class="font-bold text-stone-900">{{ item.productName }}</span>
                <span class="text-stone-500 ml-2">({{ item.quantity }} × {{ formatPrice(item.unitPrice) }})</span>
              </div>
              <span class="font-bold text-stone-900">{{ formatPrice(item.totalPrice) }}</span>
            </div>
          </div>
        </div>

        <!-- Status Management Action Strip -->
        <div class="mt-4 flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-t border-stone-100 pt-4">
          <div class="flex items-center gap-2">
            <span class="text-xs font-semibold text-stone-600">Current Phase:</span>
            <span class="text-xs font-bold text-stone-900">{{ statusLabel(order.status) }}</span>
          </div>

          <!-- Status Transition Control -->
          <div v-if="allowedNextStatuses(order.status).length > 0" class="flex items-center gap-3">
            <label :for="`status-select-${order.id}`" class="text-xs font-semibold text-stone-600">
              Advance to:
            </label>

            <select
              :id="`status-select-${order.id}`"
              v-model.number="selectedStatuses[order.id]"
              class="rounded-xl border border-stone-300 bg-white px-3 py-1.5 text-xs font-semibold text-stone-900 shadow-2xs focus:border-amber-700 focus:outline-none"
              :disabled="updatingOrderId === order.id"
            >
              <option :value="statusCode(order.status)">
                {{ statusLabel(order.status) }} (current)
              </option>
              <option
                v-for="nextCode in allowedNextStatuses(order.status)"
                :key="nextCode"
                :value="nextCode"
              >
                {{ statusNames[nextCode] }}
              </option>
            </select>

            <button
              type="button"
              class="rounded-xl bg-stone-900 px-4 py-1.5 text-xs font-bold text-white shadow-2xs hover:bg-emerald-950 disabled:opacity-40 transition"
              :disabled="
                updatingOrderId === order.id ||
                selectedStatuses[order.id] === statusCode(order.status)
              "
              @click="promptStatusChange(order)"
            >
              {{ updatingOrderId === order.id ? 'Updating...' : 'Update Status' }}
            </button>
          </div>

          <div v-else class="text-2xs font-semibold text-stone-400">
            Terminal state reached (No further transitions allowed)
          </div>
        </div>
      </div>
    </div>

    <!-- Status Change Confirmation Modal -->
    <Modal
      :is-open="isConfirmModalOpen"
      title="Confirm Order Status Transition"
      @close="cancelStatusPrompt"
    >
      <div v-if="pendingStatusChange" class="space-y-3">
        <p class="text-sm text-stone-600 leading-relaxed">
          Are you sure you want to transition order <strong>#{{ pendingStatusChange.order.id }}</strong> from
          <span class="font-bold text-stone-900">{{ statusLabel(pendingStatusChange.order.status) }}</span>
          to
          <span class="font-bold text-emerald-800">{{ statusNames[pendingStatusChange.newStatus] }}</span>?
        </p>
        <p class="text-2xs text-stone-500">
          This operation will notify downstream microservices and update customer tracking.
        </p>
      </div>

      <template #footer>
        <div class="flex justify-end gap-3">
          <button
            type="button"
            class="rounded-xl border border-stone-200 px-4 py-2 text-xs font-semibold text-stone-700 hover:bg-stone-100"
            @click="cancelStatusPrompt"
          >
            Cancel
          </button>
          <button
            type="button"
            class="rounded-xl bg-emerald-800 px-4 py-2 text-xs font-bold text-white hover:bg-emerald-900"
            @click="confirmStatusUpdate"
          >
            Confirm & Save
          </button>
        </div>
      </template>
    </Modal>
  </div>
</template>
