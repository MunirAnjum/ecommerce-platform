
<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useCartStore } from '../stores/cart'
import { getInventoryByProductId } from '../services/inventoryService'

const cartStore = useCartStore()

const availableStock = ref<Record<string, number>>({})
const inventoryErrors = ref<Record<string, string>>({})
const isCheckingStock = ref(false)

async function checkStock() {
  isCheckingStock.value = true

  await Promise.all(
    cartStore.items.map(async (item) => {
      try {
        const inventory = await getInventoryByProductId(item.productId)

        availableStock.value[item.productId] =
          inventory.availableQuantity

        delete inventoryErrors.value[item.productId]
      } catch (error) {
        console.error(
          `Failed to check stock for ${item.productName}:`,
          error,
        )

        inventoryErrors.value[item.productId] =
          'Unable to verify stock. Please try again.'
      }
    }),
  )

  isCheckingStock.value = false
}

async function increaseQuantity(productId: string) {
  const item = cartStore.items.find(
    item => item.productId === productId,
  )

  if (!item) return

  const stock = availableStock.value[productId]

  if (stock === undefined) {
    inventoryErrors.value[productId] =
      'Stock has not been verified yet.'
    return
  }

  if (item.quantity >= stock) {
    inventoryErrors.value[productId] =
      `Only ${stock} unit(s) are available.`
    return
  }

  try {
    await cartStore.increaseQuantity(productId)
    delete inventoryErrors.value[productId]
  } catch (error) {
    console.error('Failed to increase quantity:', error)
    inventoryErrors.value[productId] =
      'Unable to update quantity. Please try again.'
  }
}

async function decreaseQuantity(productId: string) {
  try {
    await cartStore.decreaseQuantity(productId)
    delete inventoryErrors.value[productId]
  } catch (error) {
    console.error('Failed to decrease quantity:', error)
    inventoryErrors.value[productId] =
      'Unable to update quantity. Please try again.'
  }
}

onMounted(async () => {
  try {
    await cartStore.loadCart()
    await checkStock()
  } catch (error) {
    console.error('Failed to load cart:', error)
  }
})
</script>


<template>
  <div class="mx-auto max-w-5xl px-6 py-10">
    <h1 class="text-3xl font-bold text-gray-900">
      Shopping Cart
    </h1>

    <div
      v-if="cartStore.items.length === 0"
      class="mt-8 rounded-lg bg-white p-6 shadow"
    >
      Your cart is empty.
    </div>

      <div v-else class="mt-8 space-y-4">
      <div
        v-for="item in cartStore.items"
        :key="item.productId"
        class="flex items-center justify-between rounded-lg bg-white p-6 shadow"
      >
        <div>
          <h2 class="text-xl font-semibold">
            {{ item.productName }}
          </h2>

          <p class="mt-2 text-gray-600">
            Price: ${{ item.unitPrice }}
          </p>

          <div class="mt-3 flex items-center gap-3">
            <button
              type="button"
              class="rounded bg-gray-200 px-3 py-1 hover:bg-gray-300"
              @click="decreaseQuantity(item.productId)"
            >
              -
            </button>

            <span class="font-semibold">
              {{ item.quantity }}
            </span>
  
            <button
              type="button"
              class="rounded bg-gray-200 px-3 py-1 hover:bg-gray-300 disabled:cursor-not-allowed disabled:opacity-50"
              :disabled="
                isCheckingStock ||
                availableStock[item.productId] === undefined ||
                item.quantity >= availableStock[item.productId]
              "
              @click="increaseQuantity(item.productId)"
            >
              +
            </button>
            
            <p
              v-if="availableStock[item.productId] !== undefined"
              class="mt-2 text-sm text-gray-500"
            >
              Available stock: {{ availableStock[item.productId] }}
            </p>

            <p
              v-if="
                availableStock[item.productId] !== undefined &&
                item.quantity > availableStock[item.productId]
              "
              class="mt-2 text-sm text-red-600"
            >
              Only {{ availableStock[item.productId] }} unit(s) are available.
              Reduce the quantity before checkout.
            </p>

            <p
              v-if="inventoryErrors[item.productId]"
              class="mt-2 text-sm text-red-600"
            >
              {{ inventoryErrors[item.productId] }}
            </p>

          </div>
        </div>

        <div class="text-right">
          <p class="text-lg font-bold text-blue-600">
            ${{ item.totalPrice }}
          </p>

          <button
            type="button"
            class="mt-3 text-red-600 hover:text-red-700"
            @click="cartStore.removeFromCart(item.productId)"
          >
            Remove
          </button>
        </div>
      </div>

      <div class="rounded-lg bg-white p-6 shadow">
        <div class="flex justify-between text-lg">
          <span>Total Items:</span>

          <span class="font-semibold">
            {{ cartStore.totalItems }}
          </span>
        </div>

        <div class="mt-3 flex justify-between text-xl">
          <span>Total:</span>

          <span class="font-bold text-blue-600">
            ${{ cartStore.totalPrice }}
          </span>
        </div>
      
        <RouterLink
          v-if="
            !isCheckingStock &&
            cartStore.items.every(item =>
              availableStock[item.productId] !== undefined &&
              item.quantity <= availableStock[item.productId]
            ) &&
            Object.keys(inventoryErrors).length === 0
          "
          to="/checkout"
          class="mt-6 inline-block rounded-lg bg-green-600 px-6 py-3 text-white hover:bg-green-700"
        >
          Proceed to Checkout
        </RouterLink>

        <button
          v-else
          type="button"
          disabled
          class="mt-6 inline-block cursor-not-allowed rounded-lg bg-gray-400 px-6 py-3 text-white"
        >
          Verify Stock Before Checkout
        </button>

      </div>
    </div>
  </div>
</template>