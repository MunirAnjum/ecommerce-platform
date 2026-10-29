<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import {
  ShoppingBag,
  Trash2,
  AlertCircle,
  ShieldCheck,
  ArrowRight,
  Minus,
  Plus,
  RefreshCw,
  Truck,
} from 'lucide-vue-next'
import { useCartStore } from '../stores/cart'
import { getInventoryByProductId } from '../services/inventoryService'
import { getProducts } from '../services/productService'
import type { Product } from '../types/productType'
import { formatPrice } from '../utils/formatters'
import { getProductImageUrl } from '../utils/productImages'
import { brandConfig } from '../config/brand'

const cartStore = useCartStore()
const productMap = ref<Record<string, Product>>({})

const availableStock = ref<Record<string, number>>({})
const inventoryErrors = ref<Record<string, string>>({})
const isCheckingStock = ref(false)

async function checkStock() {
  isCheckingStock.value = true

  await Promise.all(
    cartStore.items.map(async (item) => {
      try {
        const inventory = await getInventoryByProductId(item.productId)
        availableStock.value[item.productId] = inventory.availableQuantity
        delete inventoryErrors.value[item.productId]
      } catch (error) {
        console.error(`Failed to check stock for ${item.productName}:`, error)
        inventoryErrors.value[item.productId] = 'Unable to verify stock. Please try again.'
      }
    }),
  )

  isCheckingStock.value = false
}

function getTotalProductQuantity(productId: string): number {
  return cartStore.items
    .filter(item => item.productId === productId)
    .reduce((total, item) => total + item.quantity, 0)
}

async function increaseQuantity(productId: string, weightInGrams: number) {
  const item = cartStore.items.find(
    item =>
      item.productId === productId &&
      item.weightInGrams === weightInGrams,
  )

  if (!item) return

  const stock = availableStock.value[productId]

  if (stock === undefined) {
    inventoryErrors.value[productId] =
      'Stock has not been verified yet.'
    return
  }

  // Inventory is currently tracked per product, not per weight variant.
  const totalProductQuantity = cartStore.items
    .filter(item => item.productId === productId)
    .reduce((total, item) => total + item.quantity, 0)

  if (totalProductQuantity >= stock) {
    inventoryErrors.value[productId] =
      `Only ${stock} package(s) are available across all weights.`
    return
  }

  try {
    await cartStore.increaseQuantity(productId, weightInGrams)
    delete inventoryErrors.value[productId]
  } catch (error) {
    console.error('Failed to increase quantity:', error)
    inventoryErrors.value[productId] =
      'Unable to update quantity. Please try again.'
  }
}

async function decreaseQuantity(productId: string, weightInGrams: number) {
  try {
    await cartStore.decreaseQuantity(productId, weightInGrams)
    delete inventoryErrors.value[productId]
  } catch (error) {
    console.error('Failed to decrease quantity:', error)
    inventoryErrors.value[productId] =
      'Unable to update quantity. Please try again.'
  }
}

async function removeItem(productId: string, weightInGrams: number) {
  try {
    await cartStore.removeFromCart(productId, weightInGrams)
    delete inventoryErrors.value[productId]
  } catch (error) {
    console.error('Failed to remove item:', error)
    inventoryErrors.value[productId] =
      'Unable to remove item. Please try again.'
  }
}

const canCheckout = computed(() => {
  if (
    isCheckingStock.value ||
    cartStore.items.length === 0 ||
    Object.keys(inventoryErrors.value).length > 0
  ) {
    return false
  }

  const totalQuantityByProduct = new Map<string, number>()

  for (const item of cartStore.items) {
    const currentQuantity =
      totalQuantityByProduct.get(item.productId) ?? 0

    totalQuantityByProduct.set(
      item.productId,
      currentQuantity + item.quantity,
    )
  }

  return Array.from(totalQuantityByProduct.entries()).every(
    ([productId, totalQuantity]) => {
      const stock = availableStock.value[productId]

      return stock !== undefined && totalQuantity <= stock
    },
  )
})

onMounted(async () => {
  try {
    const [prods] = await Promise.all([
      getProducts().catch(() => [] as Product[]),
      cartStore.loadCart(),
    ])
    prods.forEach(p => {
      productMap.value[p.id] = p
    })
    await checkStock()
  } catch (error) {
    console.error('Failed to load cart:', error)
  }
})
</script>

<template>
  <div class="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
    <div class="mb-8">
      <span class="text-xs font-bold uppercase tracking-widest text-amber-800">Your Selection</span>
      <h1 class="mt-1 font-serif text-3xl font-extrabold text-stone-900 sm:text-4xl">
        Shopping Cart
      </h1>
      <p class="mt-2 text-sm text-stone-500">
        Review your selected Baltistan dry fruit harvest and verify orchard stock availability.
      </p>
    </div>

    <!-- Empty Cart State -->
    <div
      v-if="cartStore.items.length === 0"
      class="rounded-3xl border border-stone-200 bg-white p-12 text-center shadow-xs sm:p-16"
    >
      <div class="mx-auto flex h-16 w-16 items-center justify-center rounded-2xl bg-stone-100 text-stone-400">
        <ShoppingBag class="h-8 w-8" />
      </div>
      <h2 class="mt-4 font-serif text-2xl font-bold text-stone-900">Your cart is currently empty</h2>
      <p class="mt-2 text-sm text-stone-500 max-w-md mx-auto">
        Discover naturally sun-dried apricots, almonds, walnuts, and mountain pine nuts from Gilgit-Baltistan.
      </p>
      <router-link
        to="/products"
        class="mt-8 inline-flex items-center gap-2 rounded-2xl bg-stone-900 px-6 py-3 text-sm font-bold text-white shadow-md hover:bg-emerald-950 transition"
      >
        <span>Explore Catalogue</span>
        <ArrowRight class="h-4 w-4" />
      </router-link>
    </div>

    <!-- Active Cart Layout -->
    <div v-else class="grid grid-cols-1 gap-8 lg:grid-cols-12">
      <!-- Cart Items List (8 cols) -->
      <div class="lg:col-span-8 space-y-4">
        <!-- Stock verification alert banner -->
        <div
          v-if="isCheckingStock"
          class="flex items-center gap-3 rounded-2xl bg-stone-100 border border-stone-200 p-4 text-xs font-medium text-stone-700"
        >
          <span class="h-4 w-4 animate-spin rounded-full border-2 border-stone-600 border-t-transparent" />
          <span>Verifying live inventory with orchard warehouse...</span>
        </div>

        <div
          v-for="item in cartStore.items"
          :key="`${item.productId}-${item.weightInGrams}`"
          class="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 rounded-3xl border border-stone-200 bg-white p-5 shadow-xs transition hover:border-stone-300"
        >
          <!-- Item info & Thumbnail -->
          <div class="flex items-center gap-4 min-w-0 flex-1">
            <img
              :src="getProductImageUrl(productMap[item.productId] || item.productName)"
              :alt="item.productName"
              class="h-20 w-20 rounded-2xl object-cover bg-stone-100 shrink-0"
              loading="lazy"
            />

            <div class="min-w-0 flex-1">
              <h3 class="font-bold text-base text-stone-900 truncate">
                {{ item.productName }}
              </h3>
              <p>
                Unit Price:
                <strong class="text-stone-700">
                  {{ formatPrice(item.unitPrice) }}
                </strong>
              </p>

              <p>
                Unit Weight:
                <strong class="text-stone-700">
                  {{
                    item.weightInGrams === 1000
                      ? '1kg'
                      : `${item.weightInGrams}g`
                  }}
                </strong>
              </p>

              <!-- Stock information indicators -->
              <div class="mt-2 space-y-1">
                <p
                  v-if="
                    availableStock[item.productId] !== undefined &&
                    getTotalProductQuantity(item.productId) > availableStock[item.productId]
                  "
                  class="text-xs font-bold text-rose-700 flex items-center gap-1"
                >
                  <AlertCircle class="h-3.5 w-3.5 shrink-0" />
                  <span>
                    Only {{ availableStock[item.productId] }} package(s) available across all weights.
                    Reduce quantity before checkout.
                  </span>
                </p>

                <p
                  v-if="inventoryErrors[item.productId]"
                  class="text-xs font-bold text-rose-700 flex items-center gap-1"
                >
                  <AlertCircle class="h-3.5 w-3.5 shrink-0" />
                  <span>{{ inventoryErrors[item.productId] }}</span>
                </p>
              </div>
            </div>
          </div>

          <!-- Quantity Stepper & Price / Delete -->
          <div class="flex items-center justify-between sm:justify-end gap-6 w-full sm:w-auto border-t sm:border-t-0 pt-3 sm:pt-0 border-stone-100">
            <!-- Stepper -->
            <div class="flex items-center rounded-2xl border border-stone-200 bg-stone-50 p-1">
              <button
                type="button"
                class="flex h-8 w-8 items-center justify-center rounded-xl bg-white text-stone-700 shadow-2xs hover:bg-stone-100 transition"
                aria-label="Decrease quantity"
                @click="decreaseQuantity(item.productId, item.weightInGrams)"
              >
                <Minus class="h-3.5 w-3.5" />
              </button>

              <span class="w-10 text-center text-sm font-bold text-stone-900">
                {{ item.quantity }}
              </span>

              <button
                type="button"
                class="flex h-8 w-8 items-center justify-center rounded-xl bg-white text-stone-700 shadow-2xs hover:bg-stone-100 transition disabled:opacity-40 disabled:cursor-not-allowed"
                :disabled="
                  isCheckingStock ||
                  availableStock[item.productId] === undefined ||
                  getTotalProductQuantity(item.productId) >= availableStock[item.productId]
                "
                aria-label="Increase quantity"
                @click="increaseQuantity(item.productId, item.weightInGrams)"
              >
                <Plus class="h-3.5 w-3.5" />
              </button>
            </div>

            <!-- Line Total -->
            <div class="text-right min-w-24">
              <span class="block text-2xs text-stone-400 uppercase font-semibold">Subtotal</span>
              <span class="text-base font-extrabold text-stone-900">
                {{ formatPrice(item.totalPrice) }}
              </span>
            </div>

            <!-- Remove Button -->
            <button
              type="button"
              class="rounded-xl p-2 text-stone-400 hover:bg-rose-50 hover:text-rose-600 transition"
              title="Remove item"
              aria-label="Remove item"
              @click="removeItem(item.productId, item.weightInGrams)"
            >
              <Trash2 class="h-4 w-4" />
            </button>
          </div>
        </div>

        <!-- Re-verify Stock button -->
        <div class="pt-2 flex justify-end">
          <button
            type="button"
            class="inline-flex items-center gap-1.5 text-xs font-bold text-stone-600 hover:text-stone-900"
            :disabled="isCheckingStock"
            @click="checkStock"
          >
            <RefreshCw class="h-3.5 w-3.5" :class="{ 'animate-spin': isCheckingStock }" />
            <span>Re-verify Orchard Stock</span>
          </button>
        </div>
      </div>

      <!-- Order Summary Card (4 cols) -->
      <div class="lg:col-span-4">
        <div class="sticky top-24 rounded-3xl border border-stone-200 bg-white p-6 shadow-sm">
          <h2 class="font-serif text-xl font-bold text-stone-900">
            Order Summary
          </h2>

          <div class="mt-6 space-y-3 border-b border-stone-100 pb-6 text-sm">
            <div class="flex justify-between text-stone-600">
              <span>Items Total ({{ cartStore.totalItems }})</span>
              <span class="font-semibold text-stone-900">{{ formatPrice(cartStore.totalPrice) }}</span>
            </div>

            <div class="flex justify-between text-stone-600">
              <span>Delivery Charges</span>
              <span class="font-semibold text-emerald-800">
                {{ cartStore.totalPrice >= brandConfig.shippingThreshold ? 'FREE' : 'Calculated at checkout' }}
              </span>
            </div>
          </div>

          <div class="mt-6 flex items-baseline justify-between">
            <span class="text-base font-bold text-stone-900">Total Amount</span>
            <span class="text-2xl font-extrabold text-stone-900">
              {{ formatPrice(cartStore.totalPrice) }}
            </span>
          </div>

          <!-- Checkout CTA -->
          <div class="mt-6">
            <router-link
              v-if="canCheckout"
              to="/checkout"
              class="flex w-full items-center justify-center gap-2 rounded-2xl bg-emerald-800 px-6 py-4 font-bold text-white shadow-lg shadow-emerald-900/10 hover:bg-emerald-900 active:scale-98 transition"
            >
              <span>Proceed to Checkout</span>
              <ArrowRight class="h-4 w-4" />
            </router-link>

            <button
              v-else
              type="button"
              disabled
              class="w-full cursor-not-allowed rounded-2xl bg-stone-200 px-6 py-4 text-center font-bold text-stone-500"
            >
              {{ isCheckingStock ? 'Verifying Stock...' : 'Verify Stock Before Checkout' }}
            </button>
          </div>

          <!-- Free shipping progress indicator -->
          <div class="mt-6 rounded-2xl bg-stone-50 p-4 border border-stone-100">
            <div class="flex items-center gap-2 text-xs font-semibold text-stone-700">
              <Truck class="h-4 w-4 text-amber-700 shrink-0" />
              <span v-if="cartStore.totalPrice >= brandConfig.shippingThreshold">
                You've unlocked <strong>FREE nationwide shipping</strong>!
              </span>
              <span v-else>
                Add <strong>{{ formatPrice(brandConfig.shippingThreshold - cartStore.totalPrice) }}</strong> more for FREE shipping.
              </span>
            </div>
          </div>

          <!-- Assurance -->
          <div class="mt-4 flex items-center justify-center gap-2 text-2xs text-stone-400">
            <ShieldCheck class="h-4 w-4 text-emerald-700" />
            <span>Guaranteed fresh from the latest mountain harvest</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>