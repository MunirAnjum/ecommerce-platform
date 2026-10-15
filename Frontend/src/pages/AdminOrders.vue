
<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import {
  getAllOrders,
  updateOrderStatus,
  type OrderResponse,
} from '../services/orderService'

const orders = ref<OrderResponse[]>([])
const isLoading = ref(true)
const errorMessage = ref('')
const successMessage = ref('')
const statusFilter = ref('All')
const selectedStatuses = ref<Record<string, number>>({})
const updatingOrderId = ref<string | null>(null)

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
  return statusValues[status] ?? 0
}

function statusLabel(status: string | number): string {
  return statusNames[statusCode(status)] ?? String(status)
}

function statusClass(status: string | number): string {
  switch (statusCode(status)) {
    case 1:
      return 'bg-yellow-100 text-yellow-800'
    case 2:
    case 3:
      return 'bg-blue-100 text-blue-800'
    case 4:
      return 'bg-purple-100 text-purple-800'
    case 5:
      return 'bg-green-100 text-green-800'
    case 6:
      return 'bg-red-100 text-red-800'
    default:
      return 'bg-gray-100 text-gray-700'
  }
}

function allowedNextStatuses(status: string | number): number[] {
  switch (statusCode(status)) {
    case 1:
      return [2, 6]
    case 2:
      return [3]
    case 3:
      return [4]
    case 4:
      return [5]
    default:
      return []
  }
}

const filteredOrders = computed(() => {
  if (statusFilter.value === 'All') return orders.value

  return orders.value.filter(
    order => statusLabel(order.status) === statusFilter.value,
  )
})

const totalRevenue = computed(() =>
  orders.value
    .filter(order => statusCode(order.status) !== 6)
    .reduce((total, order) => total + order.totalAmount, 0),
)

function formatDate(date: string): string {
  return new Date(date).toLocaleDateString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}

function formatPrice(amount: number): string {
  return new Intl.NumberFormat(undefined, {
    style: 'currency',
    currency: 'PKR',
  }).format(amount)
}

async function loadOrders() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    orders.value = await getAllOrders()

    for (const order of orders.value) {
      selectedStatuses.value[order.id] = statusCode(order.status)
    }
  } catch (error) {
    console.error('Failed to load admin orders:', error)
    errorMessage.value =
      'Unable to load orders. Make sure you are signed in with an Admin account.'
  } finally {
    isLoading.value = false
  }
}

async function saveStatus(order: OrderResponse) {
  const newStatus = selectedStatuses.value[order.id]
  const currentStatus = statusCode(order.status)

  if (
    !allowedNextStatuses(currentStatus).includes(newStatus) ||
    updatingOrderId.value
  ) {
    return
  }

  const targetStatus = statusNames[newStatus]
  const confirmed = window.confirm(
    `Change order ${order.id} to ${targetStatus}?`,
  )

  if (!confirmed) {
    selectedStatuses.value[order.id] = currentStatus
    return
  }

  updatingOrderId.value = order.id
  errorMessage.value = ''
  successMessage.value = ''

  try {
    const updatedOrder = await updateOrderStatus(order.id, newStatus)
    const index = orders.value.findIndex(item => item.id === order.id)

    if (index !== -1) {
      orders.value[index] = updatedOrder
    }

    selectedStatuses.value[order.id] = statusCode(updatedOrder.status)
    successMessage.value = `Order status updated to ${statusLabel(updatedOrder.status)}.`
  } catch (error) {
    console.error('Failed to update order status:', error)
    selectedStatuses.value[order.id] = currentStatus
    errorMessage.value =
      'Could not update the order. Check its current status and try again.'
  } finally {
    updatingOrderId.value = null
  }
}

onMounted(loadOrders)
</script>

<template>
  <main class="mx-auto max-w-7xl px-4 py-10 sm:px-6">
    <header class="mb-8 flex flex-wrap items-center justify-between gap-4">
      <div>
        <p class="text-sm font-medium text-green-700">ADMINISTRATION</p>
        <h1 class="mt-1 text-3xl font-bold text-gray-900">
          Order Management
        </h1>
        <p class="mt-2 text-gray-500">
          Review customer orders and manage their progress.
        </p>
      </div>

      <button
        type="button"
        class="rounded-lg border border-gray-300 px-4 py-2 font-medium text-gray-700 hover:bg-gray-50"
        :disabled="isLoading"
        @click="loadOrders"
      >
        Refresh
      </button>
    </header>

    <div v-if="isLoading" class="py-16 text-center text-gray-500">
      Loading orders...
    </div>

    <template v-else>
      <div class="mb-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <section class="rounded-2xl border border-gray-200 bg-white p-5">
          <p class="text-sm text-gray-500">Total orders</p>
          <p class="mt-2 text-3xl font-bold text-gray-900">
            {{ orders.length }}
          </p>
        </section>

        <section class="rounded-2xl border border-gray-200 bg-white p-5">
          <p class="text-sm text-gray-500">Pending orders</p>
          <p class="mt-2 text-3xl font-bold text-gray-900">
            {{ orders.filter(order => statusCode(order.status) === 1).length }}
          </p>
        </section>

        <section class="rounded-2xl border border-gray-200 bg-white p-5">
          <p class="text-sm text-gray-500">Order value (excluding cancelled)</p>
          <p class="mt-2 text-3xl font-bold text-gray-900">
            {{ formatPrice(totalRevenue) }}
          </p>
        </section>
      </div>

      <p
        v-if="errorMessage"
        class="mb-4 rounded-lg border border-red-200 bg-red-50 p-4 text-red-700"
      >
        {{ errorMessage }}
      </p>

      <p
        v-if="successMessage"
        class="mb-4 rounded-lg border border-green-200 bg-green-50 p-4 text-green-800"
      >
        {{ successMessage }}
      </p>

      <section class="overflow-hidden rounded-2xl border border-gray-200 bg-white">
        <div class="flex flex-wrap items-center justify-between gap-3 border-b border-gray-200 p-5">
          <h2 class="text-lg font-semibold text-gray-900">All orders</h2>

          <select
            v-model="statusFilter"
            class="rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm"
            aria-label="Filter orders by status"
          >
            <option>All</option>
            <option>Pending</option>
            <option>Confirmed</option>
            <option>Processing</option>
            <option>Shipped</option>
            <option>Delivered</option>
            <option>Cancelled</option>
          </select>
        </div>

        <div v-if="filteredOrders.length === 0" class="p-10 text-center text-gray-500">
          No orders found for this filter.
        </div>

        <div v-else class="divide-y divide-gray-100">
          <details
            v-for="order in filteredOrders"
            :key="order.id"
            class="group p-5"
          >
            <summary class="flex cursor-pointer list-none flex-wrap items-center justify-between gap-4">
              <div class="min-w-0 flex-1">
                <p class="break-all font-semibold text-gray-900">
                  Order #{{ order.id }}
                </p>
                <p class="mt-1 text-sm text-gray-500">
                  Customer: {{ order.userId }}
                </p>
                <p class="mt-1 text-sm text-gray-500">
                  {{ formatDate(order.createdAt) }} ·
                  {{ order.items.length }}
                  {{ order.items.length === 1 ? 'item' : 'items' }}
                </p>
              </div>

              <div class="flex items-center gap-3">
                <div class="text-right">
                  <p class="font-semibold text-gray-900">
                    {{ formatPrice(order.totalAmount) }}
                  </p>
                  <span
                    class="mt-1 inline-block rounded-full px-3 py-1 text-xs font-semibold"
                    :class="statusClass(order.status)"
                  >
                    {{ statusLabel(order.status) }}
                  </span>
                </div>
                <span class="text-gray-400" aria-hidden="true"></span>
              </div>
            </summary>

            <div class="mt-5 border-t border-gray-100 pt-5">
              <h3 class="font-semibold text-gray-900">Order items</h3>

              <div
                v-for="item in order.items"
                :key="item.id"
                class="flex flex-wrap justify-between gap-3 border-b border-gray-100 py-3 last:border-0"
              >
                <div>
                  <p class="font-medium text-gray-800">
                    {{ item.productName }}
                  </p>
                  <p class="text-sm text-gray-500">
                    {{ formatPrice(item.unitPrice) }} × {{ item.quantity }}
                  </p>
                </div>
                <p class="font-medium text-gray-900">
                  {{ formatPrice(item.totalPrice) }}
                </p>
              </div>

              <div class="mt-5 flex flex-wrap items-end justify-between gap-4">
                <div>
                  <label
                    :for="`status-${order.id}`"
                    class="mb-2 block text-sm font-medium text-gray-700"
                  >
                    Update status
                  </label>

                  <select
                    :id="`status-${order.id}`"
                    v-model.number="selectedStatuses[order.id]"
                    class="rounded-lg border border-gray-300 bg-white px-3 py-2"
                    :disabled="allowedNextStatuses(order.status).length === 0 || updatingOrderId === order.id"
                  >
                    <option :value="statusCode(order.status)">
                      {{ statusLabel(order.status) }} (current)
                    </option>
                    <option
                      v-for="nextStatus in allowedNextStatuses(order.status)"
                      :key="nextStatus"
                      :value="nextStatus"
                    >
                      {{ statusNames[nextStatus] }}
                    </option>
                  </select>
                </div>

                <button
                  v-if="allowedNextStatuses(order.status).length > 0"
                  type="button"
                  class="rounded-lg bg-gray-900 px-4 py-2 font-medium text-white hover:bg-gray-800 disabled:opacity-50"
                  :disabled="
                    updatingOrderId === order.id ||
                    selectedStatuses[order.id] === statusCode(order.status)
                  "
                  @click="saveStatus(order)"
                >
                  {{ updatingOrderId === order.id ? 'Saving...' : 'Save status' }}
                </button>
              </div>
            </div>
          </details>
        </div>
      </section>
    </template>
  </main>
</template>
