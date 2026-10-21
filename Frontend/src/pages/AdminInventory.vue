<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import {
  Layers,
  Plus,
  RefreshCw,
  Search,
  AlertCircle,
  CheckCircle2,
} from 'lucide-vue-next'
import {
  getAllInventory,
  addStock,
  createInventory,
  type InventoryResponse,
} from '../services/inventoryService'
import { getProducts } from '../services/productService'
import type { Product } from '../types/productType'
import StatusBadge from '../components/StatusBadge.vue'
import Modal from '../components/Modal.vue'

const inventories = ref<InventoryResponse[]>([])
const products = ref<Product[]>([])
const isLoading = ref(true)
const errorMessage = ref('')
const successMessage = ref('')
const searchQuery = ref('')

// Add stock modal
const isAddStockModalOpen = ref(false)
const selectedInventory = ref<InventoryResponse | null>(null)
const stockToAdd = ref<number | ''>('')
const isAddingStock = ref(false)

// Initialize inventory modal
const isInitModalOpen = ref(false)
const selectedProductIdForInit = ref('')
const initialQuantity = ref<number | ''>(50)
const isInitializing = ref(false)

// Combined display row
interface InventoryDisplayRow {
  productId: string
  productName: string
  categoryName: string
  price: number
  inventoryId?: string
  availableQuantity: number
  reservedQuantity: number
  hasInventoryRecord: boolean
}

const inventoryRows = computed<InventoryDisplayRow[]>(() => {
  const invMap = new Map<string, InventoryResponse>()
  inventories.value.forEach(inv => {
    invMap.set(inv.productId, inv)
  })

  const rows: InventoryDisplayRow[] = []

  // Add all products with their inventory state
  products.value.forEach(prod => {
    const inv = invMap.get(prod.id)
    rows.push({
      productId: prod.id,
      productName: prod.name,
      categoryName: prod.categoryName,
      price: prod.price,
      inventoryId: inv?.id,
      availableQuantity: inv ? inv.availableQuantity : 0,
      reservedQuantity: inv?.reservedQuantity || 0,
      hasInventoryRecord: !!inv,
    })
  })

  // Filter
  const q = searchQuery.value.trim().toLowerCase()
  if (!q) return rows
  return rows.filter(
    r =>
      r.productName.toLowerCase().includes(q) ||
      r.productId.toLowerCase().includes(q) ||
      (r.categoryName && r.categoryName.toLowerCase().includes(q))
  )
})

const uninitializedProducts = computed(() => {
  const invProductIds = new Set(inventories.value.map(i => i.productId))
  return products.value.filter(p => !invProductIds.has(p.id))
})

async function loadData() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    const [invList, prodList] = await Promise.all([
      getAllInventory(),
      getProducts(),
    ])
    inventories.value = invList
    products.value = prodList
  } catch (error) {
    console.error('Failed to load inventory data:', error)
    errorMessage.value = 'Failed to load stock data from Inventory microservice.'
  } finally {
    isLoading.value = false
  }
}

function openAddStockModal(row: InventoryDisplayRow) {
  selectedInventory.value = {
    id: row.inventoryId,
    productId: row.productId,
    availableQuantity: row.availableQuantity,
    reservedQuantity: row.reservedQuantity,
  }
  stockToAdd.value = 25
  isAddStockModalOpen.value = true
}

async function handleAddStock() {
  if (!selectedInventory.value || stockToAdd.value === '' || Number(stockToAdd.value) <= 0) {
    errorMessage.value = 'Please specify a valid positive quantity to add.'
    return
  }

  isAddingStock.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    await addStock(selectedInventory.value.productId, Number(stockToAdd.value))
    successMessage.value = `Successfully added ${stockToAdd.value} units to stock.`
    isAddStockModalOpen.value = false
    await loadData()
  } catch (error: any) {
    console.error('Failed to add stock:', error)
    errorMessage.value = error.response?.data?.message || 'Could not update stock quantity in Inventory Service.'
  } finally {
    isAddingStock.value = false
  }
}

function openInitModal() {
  if (uninitializedProducts.value.length > 0) {
    selectedProductIdForInit.value = uninitializedProducts.value[0].id
  }
  initialQuantity.value = 50
  isInitModalOpen.value = true
}

async function handleInitInventory() {
  if (!selectedProductIdForInit.value || initialQuantity.value === '' || Number(initialQuantity.value) < 0) {
    errorMessage.value = 'Please select a product and valid initial quantity.'
    return
  }

  isInitializing.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    await createInventory({
      productId: selectedProductIdForInit.value,
      initialQuantity: Number(initialQuantity.value),
    })
    successMessage.value = 'Inventory record successfully created for the harvest product.'
    isInitModalOpen.value = false
    await loadData()
  } catch (error: any) {
    console.error('Failed to create inventory:', error)
    errorMessage.value = error.response?.data?.message || 'Failed to initialize inventory record.'
  } finally {
    isInitializing.value = false
  }
}

onMounted(loadData)
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <div>
        <h2 class="font-serif text-2xl font-bold text-stone-900 sm:text-3xl">
          Inventory & Warehouse Management
        </h2>
        <p class="mt-1 text-xs text-stone-500">
          Track warehouse reserve quotas, available packs, and replenish dry fruit stock.
        </p>
      </div>

      <div class="flex items-center gap-3">
        <button
          type="button"
          class="inline-flex items-center gap-2 rounded-xl border border-stone-200 bg-white px-3.5 py-2 text-xs font-semibold text-stone-700 shadow-2xs hover:bg-stone-50 transition"
          :disabled="isLoading"
          @click="loadData"
        >
          <RefreshCw class="h-3.5 w-3.5" :class="{ 'animate-spin': isLoading }" />
          <span>Refresh Stock</span>
        </button>

        <button
          v-if="uninitializedProducts.length > 0"
          type="button"
          class="inline-flex items-center gap-2 rounded-xl bg-stone-900 px-4 py-2 text-xs font-bold text-white shadow-md hover:bg-emerald-950 transition"
          @click="openInitModal"
        >
          <Plus class="h-4 w-4" />
          <span>Initialize Product Stock ({{ uninitializedProducts.length }})</span>
        </button>
      </div>
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

    <!-- Search Toolbar -->
    <div class="rounded-3xl border border-stone-200 bg-white p-4 shadow-xs">
      <div class="relative max-w-sm">
        <Search class="absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-stone-400" />
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Filter inventory by product name or ID..."
          class="w-full rounded-2xl border border-stone-200 bg-stone-50/50 pl-10 pr-4 py-2 text-xs text-stone-900 placeholder-stone-400 focus:border-amber-700 focus:bg-white focus:outline-none"
        />
      </div>
    </div>

    <!-- Inventory Table -->
    <div class="overflow-hidden rounded-3xl border border-stone-200 bg-white shadow-xs">
      <div v-if="isLoading" class="p-12 text-center text-stone-500">
        <span class="h-6 w-6 inline-block animate-spin rounded-full border-2 border-stone-600 border-t-transparent" />
        <p class="mt-3 text-sm font-medium">Fetching real-time inventory from microservice...</p>
      </div>

      <div v-else-if="inventoryRows.length === 0" class="p-12 text-center text-stone-500">
        <Layers class="mx-auto h-10 w-10 text-stone-300" />
        <p class="mt-3 text-sm font-medium">No inventory items found.</p>
      </div>

      <div v-else class="overflow-x-auto">
        <table class="w-full text-left border-collapse text-xs">
          <thead>
            <tr class="border-b border-stone-200 bg-stone-50/80 font-bold uppercase tracking-wider text-stone-500">
              <th class="px-6 py-4">Product Name</th>
              <th class="px-6 py-4">Status</th>
              <th class="px-6 py-4 text-center">Available Units</th>
              <th class="px-6 py-4 text-center">Reserved Units</th>
              <th class="px-6 py-4 text-right">Stock Action</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-stone-100">
            <tr
              v-for="row in inventoryRows"
              :key="row.productId"
              class="hover:bg-stone-50/50 transition"
            >
              <td class="px-6 py-4">
                <span class="font-bold text-stone-900 text-sm block">{{ row.productName }}</span>
                <span class="text-stone-400 font-mono text-2xs block truncate max-w-xs">ID: {{ row.productId }}</span>
              </td>
              <td class="px-6 py-4">
                <StatusBadge
                  v-if="row.hasInventoryRecord"
                  :status="row.availableQuantity"
                  type="stock"
                />
                <span
                  v-else
                  class="inline-block rounded-full bg-stone-100 px-2.5 py-0.5 text-2xs font-semibold text-stone-500"
                >
                  Uninitialized
                </span>
              </td>
              <td class="px-6 py-4 text-center font-bold text-sm text-stone-900">
                {{ row.availableQuantity }}
              </td>
              <td class="px-6 py-4 text-center text-stone-500 font-mono">
                {{ row.reservedQuantity }}
              </td>
              <td class="px-6 py-4 text-right">
                <button
                  v-if="row.hasInventoryRecord"
                  type="button"
                  class="inline-flex items-center gap-1.5 rounded-xl border border-stone-200 bg-stone-50 px-3 py-1.5 text-xs font-bold text-stone-800 hover:bg-stone-100 hover:border-stone-300 transition"
                  @click="openAddStockModal(row)"
                >
                  <Plus class="h-3.5 w-3.5" />
                  <span>Restock</span>
                </button>
                <button
                  v-else
                  type="button"
                  class="inline-flex items-center gap-1.5 rounded-xl bg-amber-500 px-3 py-1.5 text-xs font-bold text-stone-950 hover:bg-amber-400 transition"
                  @click="selectedProductIdForInit = row.productId; isInitModalOpen = true;"
                >
                  <span>Initialize</span>
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Restock Modal -->
    <Modal
      :is-open="isAddStockModalOpen"
      title="Restock Harvest Product"
      @close="isAddStockModalOpen = false"
    >
      <div v-if="selectedInventory" class="space-y-4">
        <p class="text-xs text-stone-600">
          Target Product ID: <span class="font-mono font-bold">{{ selectedInventory.productId }}</span>
        </p>

        <div>
          <label class="block text-xs font-bold uppercase tracking-wider text-stone-700">
            Quantity to Add
          </label>
          <input
            v-model.number="stockToAdd"
            type="number"
            min="1"
            step="1"
            required
            class="mt-1.5 w-full rounded-2xl border border-stone-200 bg-stone-50/50 px-4 py-2.5 text-sm text-stone-900 focus:border-amber-700 focus:bg-white focus:outline-none"
          />
        </div>
      </div>

      <template #footer>
        <div class="flex justify-end gap-3">
          <button
            type="button"
            class="rounded-xl border border-stone-200 px-4 py-2 text-xs font-semibold text-stone-700 hover:bg-stone-100"
            @click="isAddStockModalOpen = false"
          >
            Cancel
          </button>
          <button
            type="button"
            :disabled="isAddingStock"
            class="rounded-xl bg-stone-900 px-5 py-2 text-xs font-bold text-white hover:bg-emerald-950 disabled:opacity-50"
            @click="handleAddStock"
          >
            {{ isAddingStock ? 'Updating Stock...' : 'Confirm Restock' }}
          </button>
        </div>
      </template>
    </Modal>

    <!-- Initialize Stock Modal -->
    <Modal
      :is-open="isInitModalOpen"
      title="Initialize Product Inventory Record"
      @close="isInitModalOpen = false"
    >
      <div class="space-y-4">
        <div>
          <label class="block text-xs font-bold uppercase tracking-wider text-stone-700">
            Select Harvest Product
          </label>
          <select
            v-model="selectedProductIdForInit"
            class="mt-1.5 w-full rounded-2xl border border-stone-200 bg-stone-50/50 px-4 py-2.5 text-xs text-stone-900 focus:border-amber-700 focus:bg-white focus:outline-none"
          >
            <option
              v-for="p in uninitializedProducts"
              :key="p.id"
              :value="p.id"
            >
              {{ p.name }} ({{ p.id }})
            </option>
          </select>
        </div>

        <div>
          <label class="block text-xs font-bold uppercase tracking-wider text-stone-700">
            Initial Warehouse Packs
          </label>
          <input
            v-model.number="initialQuantity"
            type="number"
            min="0"
            step="1"
            class="mt-1.5 w-full rounded-2xl border border-stone-200 bg-stone-50/50 px-4 py-2.5 text-sm text-stone-900 focus:border-amber-700 focus:bg-white focus:outline-none"
          />
        </div>
      </div>

      <template #footer>
        <div class="flex justify-end gap-3">
          <button
            type="button"
            class="rounded-xl border border-stone-200 px-4 py-2 text-xs font-semibold text-stone-700 hover:bg-stone-100"
            @click="isInitModalOpen = false"
          >
            Cancel
          </button>
          <button
            type="button"
            :disabled="isInitializing"
            class="rounded-xl bg-stone-900 px-5 py-2 text-xs font-bold text-white hover:bg-emerald-950 disabled:opacity-50"
            @click="handleInitInventory"
          >
            {{ isInitializing ? 'Initializing...' : 'Create Inventory Record' }}
          </button>
        </div>
      </template>
    </Modal>
  </div>
</template>
