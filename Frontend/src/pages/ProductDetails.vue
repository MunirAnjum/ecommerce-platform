<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { getProductById } from '../services/productService'
import { getInventoryByProductId } from '../services/inventoryService'
import type { Product } from '../types/productType'
import { useCartStore } from '../stores/cart'

const route = useRoute()
const product = ref<Product | null>(null)
const isLoading = ref(true)
const isStockLoading = ref(true)
const availableQuantity = ref<number | null>(null)
const errorMessage = ref('')
const cartMessage = ref('')
const cartStore = useCartStore()

async function loadProduct() {
  try {
    const id = route.params.id as string
    product.value = await getProductById(id)

    const inventory = await getInventoryByProductId(id)
    availableQuantity.value = inventory.availableQuantity
  } catch (error) {
    console.error(error)
    errorMessage.value = 'Failed to load product or stock availability.'
  } finally {
    isLoading.value = false
    isStockLoading.value = false
  }
}

async function  addToCart() {
    if(!product.value || availableQuantity.value === null){
        return
    }

    if(availableQuantity.value <= 0){
      cartMessage.value ='This Product is currently out of stock.'
      return
    }

    try{
    await cartStore.addToCart(product.value.id)
    cartMessage.value = 'Product added to cart.'
    }catch (error) {
      console.error(error)
      cartMessage.value = 'Could not add this product to cart'
    }

}

onMounted(() => {
  loadProduct()
})
</script>

<template>
  <div class="mx-auto max-w-5xl px-6 py-10">

    <div
      v-if="isLoading"
      class="text-gray-600"
    >
      Loading product...
    </div>

    <div
      v-else-if="errorMessage"
      class="rounded-lg bg-red-100 p-4 text-red-700"
    >
      {{ errorMessage }}
    </div>

    <div
      v-else-if="product"
      class="rounded-xl bg-white p-8 shadow"
    >

      <h1 class="text-3xl font-bold text-gray-900">
        {{ product.name }}
      </h1>

      <p class="mt-4 text-gray-600">
        {{ product.description }}
      </p>

      <p class="mt-6 text-2xl font-bold text-blue-600">
        PKR{{ product.price }}
      </p>
      
      <p v-if="!isStockLoading && availableQuantity === 0"
        class="mt-4 font-semibold text-red-600">
        Out of Stock
      </p>

      <p v-else-if="!isStockLoading && availableQuantity !== null"
        class="mt-4 text-sm text-gray-600">
        Available: {{ availableQuantity }}
      </p>

      <p v-if="cartMessage" class="mt-3 text-sm">
        {{ cartMessage }}
      </p>

      <button
        type="button"
        class="mt-6 rounded-lg px-6 py-3 text-white"
        :class="availableQuantity === 0 || isStockLoading
          ? 'cursor-not-allowed bg-gray-400'
          : 'bg-blue-600 hover:bg-blue-700'"
        :disabled="isStockLoading || availableQuantity === null || availableQuantity <= 0"
        @click="addToCart"
      >
        {{ isStockLoading
          ? 'Checking stock...'
          : availableQuantity === 0
            ? 'Out of Stock'
            : 'Add to Cart' }}
      </button>
    </div>
  </div>
</template>