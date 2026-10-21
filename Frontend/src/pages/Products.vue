<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  Search,
  SlidersHorizontal,
  RefreshCw,
  ShoppingBag,
  X,
} from 'lucide-vue-next'
import { getProducts } from '../services/productService'
import { getInventoryByProductId } from '../services/inventoryService'
import type { Product } from '../types/productType'
import ProductCard from '../components/ProductCard.vue'

const route = useRoute()
const router = useRouter()

const products = ref<Product[]>([])
const stockMap = ref<Record<string, number>>({})
const isLoading = ref(true)
const errorMessage = ref('')

const searchQuery = ref('')
const selectedCategory = ref('All')
const inStockOnly = ref(false)
const sortBy = ref<'featured' | 'price-asc' | 'price-desc' | 'name-asc'>('featured')

// Extract unique categories from loaded products
const categories = computed(() => {
  const set = new Set<string>()
  products.value.forEach(p => {
    if (p.categoryName) {
      set.add(p.categoryName)
    }
  })
  return ['All', ...Array.from(set)]
})

async function loadProducts() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    const list = await getProducts()
    products.value = list

    // Load stock in background for all products
    list.forEach(async (p) => {
      try {
        const inv = await getInventoryByProductId(p.id)
        stockMap.value[p.id] = inv.availableQuantity
      } catch {
        // gracefully skip inventory load failure
      }
    })
  } catch (err) {
    console.error('Failed to load products:', err)
    errorMessage.value = 'Failed to load products. Please check your connection and try again.'
  } finally {
    isLoading.value = false
  }
}

// Filtered and sorted products
const filteredProducts = computed(() => {
  let result = products.value.filter(p => p.isActive !== false)

  // Search filter
  const q = searchQuery.value.trim().toLowerCase()
  if (q) {
    result = result.filter(
      p =>
        p.name.toLowerCase().includes(q) ||
        (p.description && p.description.toLowerCase().includes(q)) ||
        (p.categoryName && p.categoryName.toLowerCase().includes(q))
    )
  }

  // Category filter
  if (selectedCategory.value !== 'All') {
    const filterCat = selectedCategory.value.toLowerCase()
    result = result.filter(
      p => p.categoryName && p.categoryName.toLowerCase() === filterCat
    )
  }

  // Stock filter
  if (inStockOnly.value) {
    result = result.filter(p => {
      const stock = stockMap.value[p.id]
      return stock === undefined || stock > 0
    })
  }

  // Sorting
  if (sortBy.value === 'price-asc') {
    result = [...result].sort((a, b) => a.price - b.price)
  } else if (sortBy.value === 'price-desc') {
    result = [...result].sort((a, b) => b.price - a.price)
  } else if (sortBy.value === 'name-asc') {
    result = [...result].sort((a, b) => a.name.localeCompare(b.name))
  }

  return result
})

function resetFilters() {
  searchQuery.value = ''
  selectedCategory.value = 'All'
  inStockOnly.value = false
  sortBy.value = 'featured'
  router.replace({ query: {} })
}

function selectCategory(cat: string) {
  selectedCategory.value = cat
  if (cat.toLowerCase() === 'all') {
    const q = { ...route.query }
    delete q.category
    router.replace({ query: q })
  } else {
    router.replace({ query: { ...route.query, category: cat } })
  }
}

// Watch for search in query param
watch(
  () => route.query.search,
  (newSearch) => {
    if (typeof newSearch === 'string') {
      searchQuery.value = newSearch
    }
  },
  { immediate: true }
)

// Watch for category in query param
watch(
  () => route.query.category,
  (newCat) => {
    if (typeof newCat === 'string' && newCat.trim()) {
      selectedCategory.value = newCat.trim()
    } else {
      selectedCategory.value = 'All'
    }
  },
  { immediate: true }
)

onMounted(loadProducts)
</script>

<template>
  <div class="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
    <!-- Header banner -->
    <div class="mb-8 rounded-3xl bg-stone-900 px-6 py-10 text-white shadow-md sm:px-12">
      <div class="max-w-2xl">
        <span class="text-xs font-bold uppercase tracking-widest text-amber-400">Authentic Harvest</span>
        <h1 class="mt-2 font-serif text-3xl font-extrabold sm:text-4xl">
          Baltistan Dry Fruits Catalogue
        </h1>
        <p class="mt-3 text-sm text-stone-300 leading-relaxed sm:text-base">
          Browse our organic mountain dry fruits, harvested at peak maturity and sun-dried under Karakoram skies.
        </p>
      </div>
    </div>

    <!-- Filter / Search toolbar -->
    <div class="mb-8 space-y-4">
      <div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <!-- Search bar -->
        <div class="relative flex-1 max-w-md">
          <Search class="absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-stone-400" />
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Search almonds, apricots, walnuts..."
            class="w-full rounded-2xl border border-stone-200 bg-white pl-10 pr-10 py-2.5 text-sm text-stone-900 placeholder-stone-400 focus:border-amber-700 focus:outline-none focus:ring-1 focus:ring-amber-700"
          />
          <button
            v-if="searchQuery"
            type="button"
            class="absolute right-3 top-1/2 -translate-y-1/2 text-stone-400 hover:text-stone-700"
            @click="searchQuery = ''"
          >
            <X class="h-4 w-4" />
          </button>
        </div>

        <!-- Controls: Sort & Stock toggle -->
        <div class="flex flex-wrap items-center gap-3">
          <!-- In stock toggle -->
          <label class="flex items-center gap-2 rounded-2xl border border-stone-200 bg-white px-3.5 py-2 text-xs font-semibold text-stone-700 cursor-pointer shadow-2xs hover:bg-stone-50">
            <input
              v-model="inStockOnly"
              type="checkbox"
              class="h-4 w-4 rounded-md border-stone-300 text-emerald-800 focus:ring-emerald-700"
            />
            <span>In Stock Only</span>
          </label>

          <!-- Sort select -->
          <div class="flex items-center gap-2">
            <SlidersHorizontal class="h-4 w-4 text-stone-500" />
            <select
              v-model="sortBy"
              class="rounded-2xl border border-stone-200 bg-white px-3.5 py-2 text-xs font-semibold text-stone-700 shadow-2xs focus:border-amber-700 focus:outline-none"
            >
              <option value="featured">Featured Harvest</option>
              <option value="price-asc">Price: Low to High</option>
              <option value="price-desc">Price: High to Low</option>
              <option value="name-asc">Alphabetical (A-Z)</option>
            </select>
          </div>
        </div>
      </div>

      <!-- Category Filter Pills -->
      <div v-if="categories.length > 1" class="flex flex-wrap items-center gap-2 pt-2">
        <button
          v-for="cat in categories"
          :key="cat"
          type="button"
          class="rounded-xl px-4 py-1.5 text-xs font-semibold transition"
          :class="[
            selectedCategory.toLowerCase() === cat.toLowerCase()
              ? 'bg-stone-900 text-white shadow-xs'
              : 'border border-stone-200 bg-white text-stone-600 hover:border-amber-700/40 hover:text-stone-900'
          ]"
          @click="selectCategory(cat)"
        >
          {{ cat }}
        </button>
      </div>
    </div>

    <!-- Active filter stats -->
    <div class="mb-6 flex items-center justify-between text-xs text-stone-500">
      <span>
        Showing <strong class="text-stone-800">{{ filteredProducts.length }}</strong> products
      </span>
      <button
        v-if="searchQuery || selectedCategory !== 'All' || inStockOnly || sortBy !== 'featured'"
        type="button"
        class="font-semibold text-amber-800 hover:underline"
        @click="resetFilters"
      >
        Reset Filters
      </button>
    </div>

    <!-- Loading Skeletons -->
    <div
      v-if="isLoading"
      class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3"
    >
      <div
        v-for="i in 6"
        :key="i"
        class="animate-pulse rounded-2xl border border-stone-200 bg-white p-5"
      >
        <div class="aspect-4/3 rounded-xl bg-stone-200" />
        <div class="mt-4 h-5 w-2/3 rounded-md bg-stone-200" />
        <div class="mt-2 h-4 w-full rounded-md bg-stone-200" />
        <div class="mt-4 h-6 w-1/3 rounded-md bg-stone-200" />
      </div>
    </div>

    <!-- Error State with Retry -->
    <div
      v-else-if="errorMessage"
      class="rounded-2xl border border-rose-200 bg-rose-50 p-8 text-center text-rose-800"
    >
      <p class="font-semibold">{{ errorMessage }}</p>
      <button
        type="button"
        class="mt-4 inline-flex items-center gap-2 rounded-xl bg-rose-800 px-5 py-2.5 text-xs font-bold text-white hover:bg-rose-900"
        @click="loadProducts"
      >
        <RefreshCw class="h-4 w-4" />
        Retry Loading
      </button>
    </div>

    <!-- Empty Search Results State -->
    <div
      v-else-if="filteredProducts.length === 0"
      class="rounded-3xl border border-stone-200 bg-white px-6 py-16 text-center shadow-xs"
    >
      <ShoppingBag class="mx-auto h-12 w-12 text-stone-300" />
      <h3 class="mt-4 font-serif text-xl font-bold text-stone-900">No matching dry fruits found</h3>
      <p class="mt-2 text-sm text-stone-500">
        Try adjusting your search terms or clearing current filters.
      </p>
      <button
        type="button"
        class="mt-6 rounded-xl bg-stone-900 px-5 py-2.5 text-sm font-semibold text-white hover:bg-stone-800"
        @click="resetFilters"
      >
        Clear All Filters
      </button>
    </div>

    <!-- Products Grid -->
    <div
      v-else
      class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3"
    >
      <ProductCard
        v-for="product in filteredProducts"
        :key="product.id"
        :product="product"
        :available-stock="stockMap[product.id]"
      />
    </div>
  </div>
</template>