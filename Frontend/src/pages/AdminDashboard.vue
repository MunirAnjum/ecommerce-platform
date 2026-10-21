<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import {
  ShoppingBag,
  Package,
  Layers,
  TrendingUp,
  Clock,
  ArrowRight,
  AlertCircle,
  RefreshCw,
  FolderTree,
} from 'lucide-vue-next'
import { getAllOrders, type OrderResponse } from '../services/orderService'
import { getProducts } from '../services/productService'
import { getAllInventory } from '../services/inventoryService'
import type { Product } from '../types/productType'
import { formatPrice, formatDate } from '../utils/formatters'
import StatusBadge from '../components/StatusBadge.vue'

const orders = ref<OrderResponse[]>([])
const products = ref<Product[]>([])
const inventoryCount = ref(0)
const isLoading = ref(true)
const errorMessage = ref('')

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

const totalRevenue = computed(() => {
  return orders.value
    .filter(o => getStatusCode(o.status) !== 6) // exclude cancelled
    .reduce((sum, o) => sum + o.totalAmount, 0)
})

const pendingOrdersCount = computed(() => {
  return orders.value.filter(o => getStatusCode(o.status) === 1).length
})

const confirmedOrdersCount = computed(() => {
  return orders.value.filter(o => [2, 3, 4, 5].includes(getStatusCode(o.status))).length
})

async function loadDashboardData() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    const [ordersData, productsData, inventoryData] = await Promise.allSettled([
      getAllOrders(),
      getProducts(),
      getAllInventory(),
    ])

    if (ordersData.status === 'fulfilled') {
      orders.value = ordersData.value
    } else {
      console.warn('Orders load failed:', ordersData.reason)
    }

    if (productsData.status === 'fulfilled') {
      products.value = productsData.value
    }

    if (inventoryData.status === 'fulfilled') {
      inventoryCount.value = inventoryData.value.length
    }
  } catch (error) {
    console.error('Failed to load dashboard:', error)
    errorMessage.value = 'Failed to load some dashboard metrics.'
  } finally {
    isLoading.value = false
  }
}

onMounted(loadDashboardData)
</script>

<template>
  <div class="space-y-8">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <div>
        <h2 class="font-serif text-2xl font-bold text-stone-900 sm:text-3xl">
          Store Operations Dashboard
        </h2>
        <p class="mt-1 text-xs text-stone-500">
          Real-time metrics computed directly from Order, Product, and Inventory microservices.
        </p>
      </div>

      <button
        type="button"
        class="inline-flex items-center gap-2 rounded-xl border border-stone-200 bg-white px-4 py-2 text-xs font-semibold text-stone-700 shadow-2xs hover:bg-stone-50 transition"
        :disabled="isLoading"
        @click="loadDashboardData"
      >
        <RefreshCw class="h-3.5 w-3.5" :class="{ 'animate-spin': isLoading }" />
        <span>Refresh Metrics</span>
      </button>
    </div>

    <!-- Error Banner -->
    <div
      v-if="errorMessage"
      class="flex items-center gap-2 rounded-2xl border border-rose-200 bg-rose-50 p-4 text-xs font-bold text-rose-800"
    >
      <AlertCircle class="h-4 w-4" />
      <span>{{ errorMessage }}</span>
    </div>

    <!-- KPI Metric Cards (Computed strictly from real API data) -->
    <div class="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
      <!-- Total Revenue -->
      <div class="rounded-3xl border border-stone-200 bg-white p-6 shadow-xs">
        <div class="flex items-center justify-between">
          <span class="text-xs font-bold uppercase tracking-wider text-stone-400">Total Net Revenue</span>
          <div class="flex h-10 w-10 items-center justify-center rounded-xl bg-emerald-50 text-emerald-800">
            <TrendingUp class="h-5 w-5" />
          </div>
        </div>
        <div class="mt-4">
          <span class="text-2xl font-extrabold text-stone-900">
            {{ formatPrice(totalRevenue) }}
          </span>
          <p class="mt-1 text-2xs text-stone-400">Excludes cancelled orders</p>
        </div>
      </div>

      <!-- Pending Orders -->
      <div class="rounded-3xl border border-stone-200 bg-white p-6 shadow-xs">
        <div class="flex items-center justify-between">
          <span class="text-xs font-bold uppercase tracking-wider text-stone-400">Pending Orders</span>
          <div class="flex h-10 w-10 items-center justify-center rounded-xl bg-amber-50 text-amber-800">
            <Clock class="h-5 w-5" />
          </div>
        </div>
        <div class="mt-4">
          <span class="text-2xl font-extrabold text-stone-900">
            {{ pendingOrdersCount }}
          </span>
          <p class="mt-1 text-2xs text-amber-700 font-semibold">Requires attention</p>
        </div>
      </div>

      <!-- Total Orders -->
      <div class="rounded-3xl border border-stone-200 bg-white p-6 shadow-xs">
        <div class="flex items-center justify-between">
          <span class="text-xs font-bold uppercase tracking-wider text-stone-400">Total Orders</span>
          <div class="flex h-10 w-10 items-center justify-center rounded-xl bg-blue-50 text-blue-800">
            <ShoppingBag class="h-5 w-5" />
          </div>
        </div>
        <div class="mt-4">
          <span class="text-2xl font-extrabold text-stone-900">
            {{ orders.length }}
          </span>
          <p class="mt-1 text-2xs text-stone-400">{{ confirmedOrdersCount }} active / confirmed</p>
        </div>
      </div>

      <!-- Catalogue Products -->
      <div class="rounded-3xl border border-stone-200 bg-white p-6 shadow-xs">
        <div class="flex items-center justify-between">
          <span class="text-xs font-bold uppercase tracking-wider text-stone-400">Active Catalogue</span>
          <div class="flex h-10 w-10 items-center justify-center rounded-xl bg-purple-50 text-purple-800">
            <Package class="h-5 w-5" />
          </div>
        </div>
        <div class="mt-4">
          <span class="text-2xl font-extrabold text-stone-900">
            {{ products.length }}
          </span>
          <p class="mt-1 text-2xs text-stone-400">Dry fruit items listed</p>
        </div>
      </div>
    </div>

    <!-- Quick Navigation Shortcuts -->
    <div class="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
      <router-link
        to="/admin/orders"
        class="group flex items-center justify-between rounded-3xl border border-stone-200 bg-white p-6 shadow-xs hover:border-amber-700/30 hover:shadow-md transition"
      >
        <div class="flex items-center gap-4">
          <div class="flex h-12 w-12 items-center justify-center rounded-2xl bg-amber-50 text-amber-800">
            <ShoppingBag class="h-6 w-6" />
          </div>
          <div>
            <h3 class="font-bold text-stone-900">Manage Orders</h3>
            <p class="text-xs text-stone-500">Update statuses</p>
          </div>
        </div>
        <ArrowRight class="h-5 w-5 text-stone-400 group-hover:text-stone-900 transition" />
      </router-link>

      <router-link
        to="/admin/products"
        class="group flex items-center justify-between rounded-3xl border border-stone-200 bg-white p-6 shadow-xs hover:border-amber-700/30 hover:shadow-md transition"
      >
        <div class="flex items-center gap-4">
          <div class="flex h-12 w-12 items-center justify-center rounded-2xl bg-emerald-50 text-emerald-800">
            <Package class="h-6 w-6" />
          </div>
          <div>
            <h3 class="font-bold text-stone-900">Products</h3>
            <p class="text-xs text-stone-500">Catalogue & photos</p>
          </div>
        </div>
        <ArrowRight class="h-5 w-5 text-stone-400 group-hover:text-stone-900 transition" />
      </router-link>

      <router-link
        to="/admin/categories"
        class="group flex items-center justify-between rounded-3xl border border-stone-200 bg-white p-6 shadow-xs hover:border-amber-700/30 hover:shadow-md transition"
      >
        <div class="flex items-center gap-4">
          <div class="flex h-12 w-12 items-center justify-center rounded-2xl bg-amber-100 text-amber-900">
            <FolderTree class="h-6 w-6" />
          </div>
          <div>
            <h3 class="font-bold text-stone-900">Categories</h3>
            <p class="text-xs text-stone-500">Dry fruits & nuts</p>
          </div>
        </div>
        <ArrowRight class="h-5 w-5 text-stone-400 group-hover:text-stone-900 transition" />
      </router-link>

      <router-link
        to="/admin/inventory"
        class="group flex items-center justify-between rounded-3xl border border-stone-200 bg-white p-6 shadow-xs hover:border-amber-700/30 hover:shadow-md transition"
      >
        <div class="flex items-center gap-4">
          <div class="flex h-12 w-12 items-center justify-center rounded-2xl bg-purple-50 text-purple-800">
            <Layers class="h-6 w-6" />
          </div>
          <div>
            <h3 class="font-bold text-stone-900">Stock & Inventory</h3>
            <p class="text-xs text-stone-500">Warehouse reserves</p>
          </div>
        </div>
        <ArrowRight class="h-5 w-5 text-stone-400 group-hover:text-stone-900 transition" />
      </router-link>
    </div>

    <!-- Recent Orders Table -->
    <div class="rounded-3xl border border-stone-200 bg-white p-6 shadow-xs sm:p-8">
      <div class="flex items-center justify-between border-b border-stone-100 pb-5">
        <div>
          <h3 class="font-serif text-lg font-bold text-stone-900">Recent Customer Orders</h3>
          <p class="text-xs text-stone-500">Latest customer purchases across Pakistan</p>
        </div>

        <router-link
          to="/admin/orders"
          class="text-xs font-bold text-emerald-800 hover:underline"
        >
          View All Orders →
        </router-link>
      </div>

      <div v-if="orders.length === 0" class="py-12 text-center text-stone-500 text-sm">
        No customer orders have been recorded yet.
      </div>

      <div v-else class="divide-y divide-stone-100 mt-2">
        <div
          v-for="order in orders.slice(0, 5)"
          :key="order.id"
          class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 py-4"
        >
          <div class="min-w-0">
            <p class="font-mono text-xs font-bold text-stone-900 truncate">#{{ order.id }}</p>
            <p class="text-xs text-stone-500 mt-0.5">
              Customer: <span class="font-mono">{{ order.userId }}</span> • {{ formatDate(order.createdAt) }}
            </p>
          </div>

          <div class="flex items-center gap-4">
            <span class="font-extrabold text-stone-900 text-sm">
              {{ formatPrice(order.totalAmount) }}
            </span>
            <StatusBadge :status="order.status" type="order" />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
