
<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { getMyOrders, type OrderResponse } from '../services/orderService'

const router = useRouter()

const orders = ref<OrderResponse[]>([])
const isLoading = ref(true)
const errorMessage = ref('')

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

function getStatusClass(status: string | number): string {
  const normalized = getStatusLabel(status).toLowerCase()

  switch (normalized) {
    case 'delivered':
      return 'bg-green-100 text-green-800'
    case 'confirmed':
    case 'processing':
      return 'bg-blue-100 text-blue-800'
    case 'shipped':
      return 'bg-purple-100 text-purple-800'
    case 'pending':
      return 'bg-yellow-100 text-yellow-800'
    case 'cancelled':
      return 'bg-red-100 text-red-800'
    default:
      return 'bg-gray-100 text-gray-700'
  }
}

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

function viewOrder(orderId: string) {
  router.push(`/my-orders/${orderId}`)
}

onMounted(async () => {
  try {
    orders.value = await getMyOrders()
  } catch (error) {
    console.error('Failed to load orders:', error)
    errorMessage.value = 'Unable to load your orders. Please try again.'
  } finally {
    isLoading.value = false
  }
})
</script>

<template>
  <main class="mx-auto max-w-5xl px-4 py-10 sm:px-6">
    <div class="mb-8">
      <h1 class="text-3xl font-bold text-gray-900">My Orders</h1>
      <p class="mt-2 text-gray-500">
        View your purchases and track your orders.
      </p>
    </div>

    <div
      v-if="isLoading"
      class="py-16 text-center text-gray-500"
    >
      Loading your orders...
    </div>

    <div
      v-else-if="errorMessage"
      class="rounded-xl border border-red-200 bg-red-50 p-6 text-red-700"
    >
      {{ errorMessage }}
    </div>

    <div
      v-else-if="orders.length === 0"
      class="rounded-2xl border border-gray-200 bg-white px-6 py-16 text-center"
    >
      <div class="text-5xl">📦</div>
      <h2 class="mt-4 text-xl font-semibold text-gray-900">
        No orders yet
      </h2>
      <p class="mt-2 text-gray-500">
        Your orders will appear here after you complete checkout.
      </p>
      <RouterLink
        to="/products"
        class="mt-6 inline-block rounded-lg bg-green-600 px-5 py-3 font-medium text-white hover:bg-green-700"
      >
        Browse Products
      </RouterLink>
    </div>

    <div v-else class="space-y-5">
      <article
        v-for="order in orders"
        :key="order.id"
        class="rounded-2xl border border-gray-200 bg-white p-5 shadow-sm transition hover:shadow-md sm:p-6"
      >
        <div class="flex flex-wrap items-start justify-between gap-4">
          <div>
            <p class="text-sm text-gray-500">Order number</p>
            <h2 class="mt-1 font-semibold text-gray-900">
              {{ order.id }}
            </h2>
            <p class="mt-2 text-sm text-gray-500">
              Placed {{ formatDate(order.createdAt) }}
            </p>
          </div>

          <span
            class="rounded-full px-3 py-1 text-sm font-medium"
            :class="getStatusClass(order.status)"
          >
            {{ getStatusLabel(order.status) }}
          </span>
        </div>

        <div class="my-5 border-t border-gray-100" />

        <div class="flex flex-wrap items-end justify-between gap-4">
          <div>
            <p class="text-sm text-gray-500">
              {{ order.items.length }}
              {{ order.items.length === 1 ? 'product' : 'products' }}
            </p>
            <p class="mt-1 text-xl font-bold text-gray-900">
              {{ formatPrice(order.totalAmount) }}
            </p>
          </div>

          <button
            type="button"
            class="inline-flex items-center gap-2 rounded-lg border border-gray-300 px-4 py-2 font-medium text-gray-700 hover:bg-gray-50"
            @click="viewOrder(order.id)"
          >
            View Details
            <span aria-hidden="true">→</span>
          </button>
        </div>
      </article>
    </div>
  </main>
</template>
