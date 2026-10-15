<script setup lang="ts">
import axios from 'axios'
import { onMounted, ref } from 'vue'
import { useCartStore } from '../stores/cart'
import { CreateOrder } from '../services/orderService'
import {
  createPayment,
  processPayment,
  confirmCashOnDelivery,
  PaymentMethod,
  PaymentStatus,
} from '../services/paymentService'

const cartStore = useCartStore()

const isLoading = ref(true)
const isSubmitting = ref(false)
const errorMessage = ref('')
const successMessage = ref('')
const isOrderPlaced = ref(false)

const selectedPaymentMethod = ref<PaymentMethod>(
  PaymentMethod.FakePayment,
)

const orderId = ref<string | null>(null)
const paymentId = ref<string | null>(null)

onMounted(async () => {
  try {
    await cartStore.loadCart()
  } catch (error) {
    console.error('Failed to load cart:', error)
    errorMessage.value =
      'Unable to load your cart. Please try again.'
  } finally {
    isLoading.value = false
  }
})

async function PlaceOrder() {
  if (isSubmitting.value || isOrderPlaced.value) {
    return
  }

  // A new order requires cart items. A payment retry does not.
  if (!orderId.value) {
    if (cartStore.items.length === 0) {
      errorMessage.value =
        'Your cart is empty. Add products before checking out.'
      return
    }

    if (!cartStore.cartId) {
      errorMessage.value =
        'Your cart could not be identified. Please return to your cart and try again.'
      return
    }
  }

  isSubmitting.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    // Create the order only once.
    if (!orderId.value) {
      const order = await CreateOrder({
        cartId: cartStore.cartId!,
      })

      orderId.value = order.id

      // Clear the cart only after the order has been created.
      cartStore.clearItemsAfterCheckout()
    }

    // Create the payment only once.
    if (!paymentId.value) {
      const payment = await createPayment({
        orderId: orderId.value!,
        method: selectedPaymentMethod.value,
      })

      paymentId.value = payment.id
    }

    if (selectedPaymentMethod.value === PaymentMethod.CashOnDelivery) {
      const payment = await confirmCashOnDelivery(paymentId.value!)

      if (payment.status === PaymentStatus.Pending) {
        isOrderPlaced.value = true
        successMessage.value =
          'Your COD order has been accepted. Payment is due when cash is collected.'
      } else {
        errorMessage.value =
          'Your COD order could not be completed. Please check your orders.'
      }

      return
    }

    const payment = await processPayment(paymentId.value!, {
      shouldSucceed: true,
    })

    if (payment.status === PaymentStatus.Completed) {
      isOrderPlaced.value = true
      successMessage.value =
        'Payment completed and your order was confirmed.'
    } else if (payment.status === PaymentStatus.Failed) {
      errorMessage.value =
        'Payment failed. You can retry payment for the same order.'
    } else {
      errorMessage.value =
        'Payment is not completed yet. Please check its status.'
    }
  } catch (error: unknown) {
    console.error('Checkout/payment error:', error)

    if (axios.isAxiosError(error)) {
      const message = error.response?.data?.message

      errorMessage.value =
        typeof message === 'string'
          ? message
          : 'We could not complete checkout. Please try again.'
    } else {
      errorMessage.value =
        'We could not complete checkout. Please try again.'
    }
  } finally {
    // Always re-enable the button, even when a request fails.
    isSubmitting.value = false
  }
}
</script>

<template>
  <div class="mx-auto max-w-3xl px-6 py-10">
    <h1 class="text-3xl font-bold text-gray-900">
      Checkout
    </h1>

    <div
      v-if="isLoading"
      class="mt-8 text-gray-600"
    >
      Loading your order summary...
    </div>

    <div
      v-else-if="isOrderPlaced"
      class="mt-8 rounded-xl bg-white p-8 text-center shadow"
    >
      <h2 class="text-2xl font-bold text-green-700">
        Order placed successfully!
      </h2>

      <p class="mt-3 text-gray-600">
        {{ successMessage }}
      </p>

      <RouterLink
        to="/my-orders"
        class="mt-6 inline-block rounded-lg bg-blue-600 px-6 py-3 text-white hover:bg-blue-700"
      >
        View my orders
      </RouterLink>
    </div>

    <div
      v-else-if="orderId"
      class="mt-8 rounded-xl bg-white p-6 shadow"
    >
      <h2 class="text-xl font-semibold">Complete your order</h2>

      <p class="mt-3 text-gray-600">
        Your order has been created. Complete or retry the payment below.
      </p>

      <p v-if="errorMessage" class="mt-4 text-red-700">
        {{ errorMessage }}
      </p>

      <button
        type="button"
        class="mt-6 w-full rounded-lg bg-green-600 px-6 py-3 font-semibold text-white disabled:opacity-60"
        :disabled="isSubmitting"
        @click="PlaceOrder"
      >
        {{ isSubmitting ? 'Processing payment...' : 'Retry' }}
      </button>
    </div>

    <div
      v-else-if="cartStore.items.length === 0"
      class="mt-8 rounded-lg bg-white p-6 shadow"
    >
      <p class="text-gray-600">
        Your cart is empty. Add products before checking out.
      </p>

      <RouterLink
        to="/products"
        class="mt-4 inline-block font-semibold text-blue-600 hover:text-blue-700"
      >
        Browse products
      </RouterLink>
    </div>

    <div
      v-else
      class="mt-8 rounded-xl bg-white p-6 shadow"
    >
      <h2 class="text-xl font-semibold text-gray-900">
        Order summary
      </h2>

      <div class="mt-6 divide-y">
        <div
          v-for="item in cartStore.items"
          :key="item.productId"
          class="flex justify-between gap-4 py-4"
        >
          <div>
            <p class="font-medium text-gray-800">
              {{ item.productName }}
            </p>

            <p class="mt-1 text-sm text-gray-500">
              {{ item.quantity }} × USD{{ item.unitPrice.toFixed(2) }}
            </p>
          </div>

          <p class="font-semibold text-gray-900">
            USD{{ item.totalPrice.toFixed(2) }}
          </p>
        </div>
      </div>

      <div class="mt-6 flex justify-between border-t pt-4 text-xl">
        <span class="font-semibold">Total</span>

        <span class="font-bold text-blue-600">
          USD{{ cartStore.totalPrice.toFixed(2) }}
        </span>
      </div>

      <div
        v-if="errorMessage"
        class="mt-5 rounded-lg bg-red-100 p-4 text-red-700"
      >
        {{ errorMessage }}
      </div>
            
      <div class="mt-6">
        <h3 class="font-semibold text-gray-900">Payment method</h3>

        <label class="mt-3 flex cursor-pointer items-center gap-3 rounded-lg border p-4">
          <input
            v-model="selectedPaymentMethod"
            type="radio"
            :value="PaymentMethod.FakePayment"
            :disabled="isSubmitting"
          />
          <span>
            <span class="block font-medium">Online payment (test)</span>
            <span class="text-sm text-gray-500">Simulate a payment.</span>
          </span>
        </label>

        <label class="mt-3 flex cursor-pointer items-center gap-3 rounded-lg border p-4">
          <input
            v-model="selectedPaymentMethod"
            type="radio"
            :value="PaymentMethod.CashOnDelivery"
            :disabled="isSubmitting"
          />
          <span>
            <span class="block font-medium">Cash on Delivery</span>
            <span class="text-sm text-gray-500">Pay when cash is collected.</span>
          </span>
        </label>
      </div>

      <button
        type="button"
        class="mt-6 w-full rounded-lg bg-green-600 px-6 py-3 font-semibold text-white hover:bg-green-700 disabled:cursor-not-allowed disabled:opacity-60"
        :disabled="isSubmitting || isLoading"
        @click="PlaceOrder"
      >
        {{ isSubmitting ? 'Processing payment...' : 'Place Order & Pay' }}
      </button>

      <RouterLink
        to="/cart"
        class="mt-4 block text-center text-gray-600 hover:text-gray-900"
      >
        Return to cart
      </RouterLink>
    </div>
  </div>
</template>