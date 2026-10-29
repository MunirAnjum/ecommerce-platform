<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  ShoppingBag,
  AlertCircle,
  Sun,
  ShieldCheck,
  Mountain,
  ChevronRight,
  Minus,
  Plus,
  RefreshCw,
} from 'lucide-vue-next'
import { getProductById, getProducts } from '../services/productService'
import { getInventoryByProductId } from '../services/inventoryService'
import type { Product } from '../types/productType'
import { useCartStore } from '../stores/cart'
import { useAuthStore } from '../stores/auth'
import { formatPrice } from '../utils/formatters'
import { getProductImageUrl, DEFAULT_PRODUCT_FALLBACK } from '../utils/productImages'
import ProductCard from '../components/ProductCard.vue'

const route = useRoute()
const router = useRouter()
const cartStore = useCartStore()
const authStore = useAuthStore()

const product = ref<Product | null>(null)
const relatedProducts = ref<Product[]>([])
const isLoading = ref(true)
const isStockLoading = ref(true)
const availableQuantity = ref<number | null>(null)
const errorMessage = ref('')
const cartMessage = ref('')
const isSuccess = ref(false)
const isAdding = ref(false)
const quantity = ref(1)
const selectedWeight = ref(250)
const imgError = ref(false)

const weightOptions = [
  { label: '250g', value: 250 },
  { label: '500g', value: 500 },
  { label: '1kg', value: 1000 },
]

const selectedPrice = computed(() => {
  if (!product.value) return 0

  return product.value.price * (selectedWeight.value / 250)
})

async function loadProduct() {
  const id = route.params.id as string
  if (!id) return

  isLoading.value = true
  isStockLoading.value = true
  errorMessage.value = ''
  cartMessage.value = ''
  imgError.value = false
  quantity.value = 1
  selectedWeight.value = 250

  try {
    const data = await getProductById(id)
    product.value = data

    // Load inventory
    try {
      const inventory = await getInventoryByProductId(id)
      availableQuantity.value = inventory.availableQuantity
    } catch {
      availableQuantity.value = null
    }

    // Load related products
    try {
      const all = await getProducts()
      relatedProducts.value = all
        .filter(p => p.id !== id && (p.categoryId === data.categoryId || p.categoryName === data.categoryName))
        .slice(0, 3)
      if (relatedProducts.value.length === 0) {
        relatedProducts.value = all.filter(p => p.id !== id).slice(0, 3)
      }
    } catch {
      relatedProducts.value = []
    }
  } catch (error) {
    console.error('Failed to load product details:', error)
    errorMessage.value = 'Failed to load product details. Please check the URL or try again.'
  } finally {
    isLoading.value = false
    isStockLoading.value = false
  }
}

function incrementQty() {
  if (availableQuantity.value !== null && quantity.value >= availableQuantity.value) {
    return
  }
  quantity.value++
}

function decrementQty() {
  if (quantity.value > 1) {
    quantity.value--
  }
}

async function handleAddToCart() {
  if (!product.value) return

  if (!authStore.isAuthenticated) {
    router.push({ name: 'login', query: { redirect: `/products/${product.value.id}` } })
    return
  }

  if (availableQuantity.value !== null && availableQuantity.value <= 0) {
    cartMessage.value = 'This product is currently out of stock.'
    isSuccess.value = false
    return
  }

  isAdding.value = true
  cartMessage.value = ''

 try {
  await cartStore.addToCart(
    product.value.id,
    selectedWeight.value,
    quantity.value,
  )

  cartMessage.value =
    `Added ${quantity.value} × ${weightOptions.find(
      option => option.value === selectedWeight.value,
    )?.label} to your shopping cart.`

  isSuccess.value = true
} catch (error) {
  console.error('Cart add failed:', error)
  cartMessage.value =
    'Could not add product to cart. Please try again.'
  isSuccess.value = false
} finally {
  isAdding.value = false
}
}

watch(() => route.params.id, () => {
  loadProduct()
})

onMounted(loadProduct)
</script>

<template>
  <div class="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
    <!-- Breadcrumb -->
    <nav class="mb-8 flex items-center gap-2 text-xs font-semibold text-stone-500">
      <router-link to="/" class="hover:text-stone-900 transition">Home</router-link>
      <ChevronRight class="h-3 w-3" />
      <router-link to="/products" class="hover:text-stone-900 transition">Catalogue</router-link>
      <ChevronRight class="h-3 w-3" />
      <span class="truncate text-stone-800">{{ product?.name || 'Product Details' }}</span>
    </nav>

    <!-- Loading State -->
    <div v-if="isLoading" class="grid grid-cols-1 gap-12 lg:grid-cols-2">
      <div class="aspect-4/3 rounded-3xl bg-stone-200 animate-pulse" />
      <div class="space-y-4">
        <div class="h-8 w-3/4 rounded-lg bg-stone-200 animate-pulse" />
        <div class="h-6 w-1/4 rounded-lg bg-stone-200 animate-pulse" />
        <div class="h-24 w-full rounded-lg bg-stone-200 animate-pulse" />
        <div class="h-12 w-1/2 rounded-lg bg-stone-200 animate-pulse" />
      </div>
    </div>

    <!-- Error State -->
    <div
      v-else-if="errorMessage"
      class="rounded-3xl border border-rose-200 bg-rose-50 p-12 text-center text-rose-800"
    >
      <AlertCircle class="mx-auto h-12 w-12 text-rose-600" />
      <h2 class="mt-4 font-serif text-2xl font-bold">Product Not Found</h2>
      <p class="mt-2 text-sm text-rose-700">{{ errorMessage }}</p>
      <button
        type="button"
        class="mt-6 inline-flex items-center gap-2 rounded-xl bg-rose-800 px-5 py-2.5 text-xs font-bold text-white hover:bg-rose-900"
        @click="loadProduct"
      >
        <RefreshCw class="h-4 w-4" />
        Retry Loading
      </button>
    </div>

    <!-- Main Product Layout -->
    <div v-else-if="product" class="space-y-16">
      <div class="grid grid-cols-1 gap-12 lg:grid-cols-2">
        <!-- Product Visual Showcase -->
        <div class="relative overflow-hidden rounded-3xl border border-stone-200 bg-stone-100 shadow-md">
          <img
            :src="imgError ? DEFAULT_PRODUCT_FALLBACK : getProductImageUrl(product)"
            :alt="product.name"
            class="h-full w-full object-cover max-h-[790px]"
            @error="imgError = true"
          />

          <!-- Category Badge overlay -->
          <div class="absolute top-4 left-4">
            <span class="rounded-full bg-white/95 px-3.5 py-1 text-xs font-bold tracking-wide text-stone-900 backdrop-blur-xs shadow-sm">
              {{ product.categoryName || 'Baltistan Specialty' }}
            </span>
          </div>
        </div>

        <!-- Product Purchase Information -->
        <div class="flex flex-col">
          <span class="text-xs font-bold uppercase tracking-widest text-amber-800">
            Pristine Mountain Harvest
          </span>

          <h1 class="mt-2 font-serif text-3xl font-extrabold text-stone-900 sm:text-4xl">
            {{ product.name }}
          </h1>

          <div class="mt-4 flex flex-wrap items-baseline gap-3">
            <span class="text-3xl font-extrabold text-stone-900">
              {{ formatPrice(selectedPrice) }}
            </span>

            <span class="text-sm font-semibold text-stone-500">
              / {{ weightOptions.find(option => option.value === selectedWeight)?.label }}
            </span>

            <span class="text-xs font-semibold uppercase text-stone-500">
              Tax Included
            </span>
          </div>

          <!-- Description -->
          <div class="mt-6 border-t border-stone-200 pt-6">
            <h3 class="text-xs font-bold uppercase tracking-wider text-stone-500">Harvest Details</h3>
            <p class="mt-2 text-base text-stone-600 leading-relaxed">
              {{ product.description || 'Authentic organic mountain harvest carefully gathered by Balti farming cooperatives. Sun-ripened and preserved using centuries-old natural methods.' }}
            </p>
          </div>

          <!-- Stock Status Banner -->
          <div class="mt-6">
            <div
              v-if="isStockLoading"
              class="inline-flex items-center gap-2 rounded-xl bg-stone-100 px-3.5 py-2 text-xs font-medium text-stone-600"
            >
              <span class="h-3 w-3 animate-spin rounded-full border-2 border-stone-500 border-t-transparent" />
              Verifying real-time orchard inventory...
            </div>

            <div
              v-else-if="availableQuantity === 0"
              class="inline-flex items-center gap-2 rounded-xl bg-rose-50 border border-rose-200 px-3.5 py-2 text-xs font-bold text-rose-700"
            >
              <AlertCircle class="h-4 w-4" />
              Currently Sold Out — Fresh batch arriving with next shipment
            </div>

            <!-- <div
              v-else-if="availableQuantity !== null"
              class="inline-flex items-center gap-2 rounded-xl px-3.5 py-2 text-xs font-bold"
              :class="availableQuantity <= 5 ? 'bg-amber-50 border border-amber-200 text-amber-800' : 'bg-emerald-50 border border-emerald-200 text-emerald-800'"
            >
              <CheckCircle2 class="h-4 w-4" />
              In Stock: {{ availableQuantity }} packs available
            </div> -->
          </div>

          <!-- Quantity Stepper & Add to Cart -->
          <div class="mt-8 space-y-4">
            <div class="flex items-center gap-4">
              <!-- Package Weight Selection -->
              <div class="space-y-3">
                <span class="block text-xs font-bold uppercase tracking-wider text-stone-500">
                  Select Weight
                </span>

                <div class="grid grid-cols-3 gap-3">
                  <button
                    v-for="option in weightOptions"
                    :key="option.value"
                    type="button"
                    class="rounded-xl border px-4 py-3 text-sm font-bold transition"
                    :class="
                      selectedWeight === option.value
                        ? 'border-emerald-800 bg-emerald-50 text-emerald-900 ring-1 ring-emerald-800'
                        : 'border-stone-200 bg-white text-stone-700 hover:border-stone-400'
                    "
                    :aria-pressed="selectedWeight === option.value"
                    @click="selectedWeight = option.value"
                  >
                    {{ option.label }}
                  </button>
                </div>

                <p class="text-xs text-stone-500">
                  Price shown is for the selected package weight.
                </p>
              </div>

              <span class="text-xs font-bold uppercase text-stone-500">Quantity</span>

              <div class="flex items-center rounded-2xl border border-stone-200 bg-white shadow-2xs">
                <button
                  type="button"
                  class="p-2.5 text-stone-600 hover:text-stone-900 disabled:opacity-30 disabled:cursor-not-allowed"
                  :disabled="quantity <= 1 || isStockLoading || availableQuantity === 0"
                  aria-label="Decrease quantity"
                  @click="decrementQty"
                >
                  <Minus class="h-4 w-4" />
                </button>

                <span class="w-10 text-center font-bold text-stone-900 text-sm">
                  {{ quantity }}
                </span>

                <button
                  type="button"
                  class="p-2.5 text-stone-600 hover:text-stone-900 disabled:opacity-30 disabled:cursor-not-allowed"
                  :disabled="isStockLoading || availableQuantity === null || quantity >= availableQuantity"
                  aria-label="Increase quantity"
                  @click="incrementQty"
                >
                  <Plus class="h-4 w-4" />
                </button>
              </div>
            </div>

            <!-- Cart Message Alert -->
            <div
              v-if="cartMessage"
              class="rounded-xl p-4 text-xs font-bold"
              :class="isSuccess ? 'bg-emerald-50 border border-emerald-200 text-emerald-800' : 'bg-rose-50 border border-rose-200 text-rose-800'"
            >
              {{ cartMessage }}
            </div>

            <!-- Submit Button -->
            <button
              type="button"
              class="flex w-full items-center justify-center gap-2.5 rounded-2xl px-8 py-4 font-bold text-white shadow-lg transition active:scale-98"
              :class="[
                availableQuantity === 0 || isStockLoading
                  ? 'cursor-not-allowed bg-stone-300 text-stone-500'
                  : 'bg-stone-900 hover:bg-emerald-950 shadow-stone-900/10'
              ]"
              :disabled="isStockLoading || availableQuantity === 0 || isAdding"
              @click="handleAddToCart"
            >
              <template v-if="isAdding">
                <span class="h-5 w-5 animate-spin rounded-full border-2 border-white border-t-transparent" />
                <span>Adding to Cart...</span>
              </template>
              <template v-else-if="availableQuantity === 0">
                <span>Out of Stock</span>
              </template>
              <template v-else>
                <ShoppingBag class="h-5 w-5" />
                <span>Add {{ quantity }} to Cart</span>
              </template>
            </button>
          </div>

          <!-- Purity Pillars -->
          <div class="mt-10 grid grid-cols-3 gap-3 border-t border-stone-200 pt-6">
            <div class="flex flex-col items-center text-center p-3 rounded-2xl bg-stone-50">
              <Sun class="h-5 w-5 text-amber-700" />
              <span class="mt-2 text-xs font-bold text-stone-800">Sun-Dried</span>
              <span class="text-2xs text-stone-500">No ovens or chemicals</span>
            </div>
            <div class="flex flex-col items-center text-center p-3 rounded-2xl bg-stone-50">
              <Mountain class="h-5 w-5 text-emerald-800" />
              <span class="mt-2 text-xs font-bold text-stone-800">2,500m Origin</span>
              <span class="text-2xs text-stone-500">Skardu & Hunza</span>
            </div>
            <div class="flex flex-col items-center text-center p-3 rounded-2xl bg-stone-50">
              <ShieldCheck class="h-5 w-5 text-amber-700" />
              <span class="mt-2 text-xs font-bold text-stone-800">Airtight Pouch</span>
              <span class="text-2xs text-stone-500">Multi-layer barrier</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Related Products Section -->
      <section v-if="relatedProducts.length > 0" class="border-t border-stone-200 pt-16">
        <div class="flex items-center justify-between">
          <h2 class="font-serif text-2xl font-bold text-stone-900">
            More From Our Mountain Harvest
          </h2>
          <router-link to="/products" class="text-xs font-bold text-emerald-800 hover:underline">
            View All →
          </router-link>
        </div>

        <div class="mt-8 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
          <ProductCard
            v-for="rel in relatedProducts"
            :key="rel.id"
            :product="rel"
          />
        </div>
      </section>
    </div>
  </div>
</template>