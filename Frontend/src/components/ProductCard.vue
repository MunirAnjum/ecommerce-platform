<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { ShoppingBag, Check, Eye } from 'lucide-vue-next'
import type { Product } from '../types/productType'
import { useCartStore } from '../stores/cart'
import { useAuthStore } from '../stores/auth'
import { formatPrice } from '../utils/formatters'
import { getProductImageUrl, DEFAULT_PRODUCT_FALLBACK } from '../utils/productImages'

const props = defineProps<{
  product: Product
  availableStock?: number | null
}>()

const router = useRouter()
const cartStore = useCartStore()
const authStore = useAuthStore()

const isAdding = ref(false)
const justAdded = ref(false)
const imgError = ref(false)

function onImageError() {
  imgError.value = true
}

async function handleAddToCart(e: Event) {
  e.preventDefault()
  e.stopPropagation()

  if (!authStore.isAuthenticated) {
    router.push({ name: 'login', query: { redirect: `/products/${props.product.id}` } })
    return
  }

  if (props.availableStock !== undefined && props.availableStock !== null && props.availableStock <= 0) {
    return
  }

  isAdding.value = true
  try {
    await cartStore.addToCart(props.product.id)
    justAdded.value = true
    setTimeout(() => {
      justAdded.value = false
    }, 2000)
  } catch (error) {
    console.error('Failed to add product to cart:', error)
  } finally {
    isAdding.value = false
  }
}
</script>

<template>
  <div
    class="group relative flex flex-col overflow-hidden rounded-2xl border border-stone-200/80 bg-white transition-all duration-300 hover:-translate-y-1 hover:border-amber-700/30 hover:shadow-xl hover:shadow-stone-900/5"
  >
    <!-- Image container -->
    <div class="relative aspect-4/3 w-full overflow-hidden bg-stone-100">
      <img
        :src="imgError ? DEFAULT_PRODUCT_FALLBACK : getProductImageUrl(product.name, product.categoryName, undefined, product.id)"
        :alt="product.name"
        class="h-full w-full object-cover transition-transform duration-500 ease-out group-hover:scale-105"
        loading="lazy"
        @error="onImageError"
      />

      <!-- Category Pill -->
      <div class="absolute top-3 left-3">
        <span class="inline-flex items-center rounded-full bg-white/90 px-3 py-1 text-xs font-semibold tracking-wide text-stone-800 backdrop-blur-xs shadow-xs">
          {{ product.categoryName || 'Baltistan Dry Fruit' }}
        </span>
      </div>

      <!-- Stock status indicator -->
      <div v-if="availableStock !== undefined && availableStock !== null" class="absolute top-3 right-3">
        <span
          v-if="availableStock <= 0"
          class="inline-flex items-center rounded-full bg-rose-900/90 px-2.5 py-1 text-xs font-semibold text-white backdrop-blur-xs shadow-xs"
        >
          Sold Out
        </span>
        <span
          v-else-if="availableStock <= 5"
          class="inline-flex items-center rounded-full bg-amber-700/90 px-2.5 py-1 text-xs font-semibold text-white backdrop-blur-xs shadow-xs"
        >
          Low Stock
        </span>
      </div>

      <!-- Quick action overlay on hover -->
      <div class="absolute inset-0 flex items-center justify-center gap-2 bg-stone-950/25 opacity-0 transition-opacity duration-200 group-hover:opacity-100">
        <router-link
          :to="`/products/${product.id}`"
          class="flex h-10 w-10 items-center justify-center rounded-full bg-white text-stone-800 shadow-md transition hover:bg-stone-900 hover:text-white"
          title="View Details"
        >
          <Eye class="h-4 w-4" />
        </router-link>
      </div>
    </div>

    <!-- Content details -->
    <div class="flex flex-1 flex-col p-5">
      <router-link
        :to="`/products/${product.id}`"
        class="group/title block text-lg font-bold text-stone-900 transition hover:text-emerald-800"
      >
        <h3 class="line-clamp-1">{{ product.name }}</h3>
      </router-link>

      <p class="mt-1 line-clamp-2 text-sm text-stone-500 flex-1 leading-relaxed">
        {{ product.description || 'Authentic organic mountain harvest naturally cured under Karakoram sunshine.' }}
      </p>

      <!-- Price & Actions footer -->
      <div class="mt-4 flex items-center justify-between border-t border-stone-100 pt-3">
        <div>
          <span class="block text-xs font-medium text-stone-400 uppercase tracking-wider">Per Pack</span>
          <span class="text-lg font-extrabold text-stone-900">
            {{ formatPrice(product.price) }}
          </span>
        </div>

        <button
          type="button"
          :disabled="isAdding || (availableStock !== undefined && availableStock !== null && availableStock <= 0)"
          class="inline-flex items-center justify-center gap-1.5 rounded-xl px-4 py-2 text-sm font-semibold transition duration-200"
          :class="[
            availableStock !== undefined && availableStock !== null && availableStock <= 0
              ? 'cursor-not-allowed bg-stone-100 text-stone-400'
              : justAdded
                ? 'bg-emerald-700 text-white shadow-sm'
                : 'bg-stone-900 text-white hover:bg-emerald-900 shadow-sm active:scale-95'
          ]"
          @click="handleAddToCart"
        >
          <template v-if="justAdded">
            <Check class="h-4 w-4 text-emerald-200" />
            <span>Added</span>
          </template>
          <template v-else-if="isAdding">
            <span class="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
            <span>Adding...</span>
          </template>
          <template v-else-if="availableStock !== undefined && availableStock !== null && availableStock <= 0">
            <span>Sold Out</span>
          </template>
          <template v-else>
            <ShoppingBag class="h-4 w-4" />
            <span>Add</span>
          </template>
        </button>
      </div>
    </div>
  </div>
</template>
