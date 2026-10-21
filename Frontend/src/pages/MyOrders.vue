<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import {
  Package,
  Calendar,
  ArrowRight,
  ShoppingBag,
  RefreshCw,
  AlertCircle,
} from 'lucide-vue-next'
import { getMyOrders, type OrderResponse } from '../services/orderService'
import { formatPrice, formatDate } from '../utils/formatters'
import StatusBadge from '../components/StatusBadge.vue'

const router = useRouter()

const orders = ref<OrderResponse[]>([])
const isLoading = ref(true)
const errorMessage = ref('')

async function loadOrders() {
  isLoading.value = true
  errorMessage.value = ''
  try {
    orders.value = await getMyOrders()
  } catch (error) {
    console.error('Failed to load customer orders:', error)
    errorMessage.value = 'Unable to retrieve your order history. Please try again.'
  } finally {
    isLoading.value = false
  }
}

function viewOrder(orderId: string) {
  router.push(`/my-orders/${orderId}`)
}

onMounted(loadOrders)
</script>

<template>
  <div class="mx-auto max-w-5xl px-4 py-8 sm:px-6 lg:px-8">
    <!-- Header -->
    <div class="mb-8 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <div>
        <span class="text-xs font-bold uppercase tracking-widest text-amber-800">Account Dashboard</span>
        <h1 class="mt-1 font-serif text-3xl font-extrabold text-stone-900 sm:text-4xl">
          My Order History
        </h1>
        <p class="mt-2 text-sm text-stone-500">
          Track packages, view receipts, and monitor progress from Baltistan to your doorstep.
        </p>
      </div>

      <button
        type="button"
        class="inline-flex items-center gap-2 rounded-xl border border-stone-200 bg-white px-4 py-2 text-xs font-semibold text-stone-700 shadow-2xs hover:bg-stone-50"
        :disabled="isLoading"
        @click="loadOrders"
      >
        <RefreshCw class="h-3.5 w-3.5" :class="{ 'animate-spin': isLoading }" />
        <span>Refresh</span>
      </button>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading" class="space-y-4">
      <div v-for="i in 3" :key="i" class="animate-pulse rounded-3xl border border-stone-200 bg-white p-6">
        <div class="h-5 w-1/3 rounded-md bg-stone-200" />
        <div class="mt-4 h-4 w-1/4 rounded-md bg-stone-200" />
        <div class="mt-4 h-8 w-1/5 rounded-md bg-stone-200" />
      </div>
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
        @click="loadOrders"
      >
        Retry
      </button>
    </div>

    <!-- Empty Orders State -->
    <div
      v-else-if="orders.length === 0"
      class="rounded-3xl border border-stone-200 bg-white p-12 text-center shadow-xs sm:p-16"
    >
      <div class="mx-auto flex h-16 w-16 items-center justify-center rounded-2xl bg-stone-100 text-stone-400">
        <Package class="h-8 w-8" />
      </div>
      <h2 class="mt-4 font-serif text-2xl font-bold text-stone-900">No orders placed yet</h2>
      <p class="mt-2 text-sm text-stone-500 max-w-sm mx-auto">
        Your order receipts and live shipment tracking details will appear here once you make a purchase.
      </p>
      <router-link
        to="/products"
        class="mt-8 inline-flex items-center gap-2 rounded-2xl bg-stone-900 px-6 py-3 text-sm font-bold text-white shadow-md hover:bg-emerald-950 transition"
      >
        <ShoppingBag class="h-4 w-4" />
        <span>Explore Catalogue</span>
      </router-link>
    </div>

    <!-- Orders List -->
    <div v-else class="space-y-4">
      <article
        v-for="order in orders"
        :key="order.id"
        class="rounded-3xl border border-stone-200 bg-white p-6 shadow-xs transition hover:border-amber-700/30 hover:shadow-md sm:p-7"
      >
        <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <div class="flex items-center gap-3">
              <span class="text-xs font-bold uppercase tracking-wider text-stone-400">Order ID:</span>
              <span class="font-mono text-sm font-bold text-stone-900 break-all">{{ order.id }}</span>
            </div>

            <div class="mt-1 flex items-center gap-2 text-xs text-stone-500">
              <Calendar class="h-3.5 w-3.5 text-stone-400" />
              <span>Placed on {{ formatDate(order.createdAt) }}</span>
            </div>
          </div>

          <StatusBadge :status="order.status" type="order" />
        </div>

        <div class="my-5 border-t border-stone-100" />

        <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <p class="text-xs text-stone-500">
              {{ order.items.length }} {{ order.items.length === 1 ? 'item' : 'items' }} included
            </p>
            <p class="mt-0.5 text-xl font-extrabold text-stone-900">
              {{ formatPrice(order.totalAmount) }}
            </p>
          </div>

          <button
            type="button"
            class="inline-flex items-center justify-center gap-2 rounded-2xl border border-stone-200 bg-stone-50 px-5 py-2.5 text-xs font-bold text-stone-800 shadow-2xs hover:bg-stone-100 hover:border-stone-300 transition"
            @click="viewOrder(order.id)"
          >
            <span>View Details & Tracking</span>
            <ArrowRight class="h-3.5 w-3.5 text-stone-500" />
          </button>
        </div>
      </article>
    </div>
  </div>
</template>
