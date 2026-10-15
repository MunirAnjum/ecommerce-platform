<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { getProducts } from '../services/productService'
import type { Product  } from '../types/productType'

const products = ref<Product[]>([])
const isLoading = ref(true)
const errorMessage = ref('')

async function loadProducts() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    products.value = await getProducts()
  } catch (error) {
    console.error(error)
    errorMessage.value = 'Failed to load products.'
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  loadProducts()
})
</script>

<template>
  <div class="min-h-screen bg-gray-100 p-8">

    <div class="mx-auto max-w-6xl">

      <h1 class="text-3xl font-bold text-gray-800">
        Products
      </h1>

      <p class="mt-2 text-gray-600">
        Browse our products
      </p>

      <div
        v-if="isLoading"
        class="mt-8 text-gray-600"
      >
        Loading products...
      </div>

      <div
        v-else-if="errorMessage"
        class="mt-8 rounded-lg bg-red-100 p-4 text-red-700"
      >
        {{ errorMessage }}
      </div>

      <div
        v-else-if="products.length === 0"
        class="mt-8 rounded-lg bg-white p-6 shadow"
      >
        No products found.
      </div>

      <div
        v-else
        class="mt-8 grid gap-6 sm:grid-cols-2 lg:grid-cols-3"
      >

        <router-link
          v-for="product in products"
          :key="product.id"
          :to="`/products/${product.id}`"
          class="rounded-xl bg-white p-6 shadow transition hover:-translate-y-1 hover:shadow-lg"
        >

          <h2 class="text-xl font-semibold text-gray-800">
            {{ product.name }}
          </h2>

          <p class="mt-2 text-gray-600">
            {{ product.description }}
          </p>

          <p class="mt-4 text-lg font-bold text-blue-600">
            ${{ product.price }}
          </p>
        </router-link>
      </div>
    </div>
  </div>
</template>