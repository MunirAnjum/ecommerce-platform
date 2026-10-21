<script setup lang="ts">
import axios from 'axios'
import { onMounted, ref } from 'vue'
import {
  CreditCard,
  Banknote,
  ShieldCheck,
  CheckCircle2,
  AlertCircle,
  ArrowRight,
  ArrowLeft,
  Package,
} from 'lucide-vue-next'
import { useCartStore } from '../stores/cart'
import { CreateOrder } from '../services/orderService'
import {
  createPayment,
  processPayment,
  confirmCashOnDelivery,
  PaymentMethod,
  PaymentStatus,
} from '../services/paymentService'
import { formatPrice } from '../utils/formatters'
import { getProductImageUrl } from '../utils/productImages'

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
    errorMessage.value = 'Unable to load your cart. Please try again.'
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
  <div class="mx-auto max-w-4xl px-4 py-8 sm:px-6 lg:px-8">
    <div class="mb-8">
      <span class="text-xs font-bold uppercase tracking-widest text-amber-800">Final Step</span>
      <h1 class="mt-1 font-serif text-3xl font-extrabold text-stone-900 sm:text-4xl">
        Checkout & Payment
      </h1>
      <p class="mt-2 text-sm text-stone-500">
        Review your order and select your preferred payment method.
      </p>
    </div>

    <!-- Loading State -->
    <div
      v-if="isLoading"
      class="rounded-3xl border border-stone-200 bg-white p-12 text-center text-stone-500"
    >
      <span class="h-6 w-6 inline-block animate-spin rounded-full border-2 border-stone-600 border-t-transparent" />
      <p class="mt-3 text-sm font-medium">Preparing your order summary...</p>
    </div>

    <!-- Success Screen (Order Placed) -->
    <div
      v-else-if="isOrderPlaced"
      class="rounded-3xl border border-emerald-200 bg-white p-8 text-center shadow-lg sm:p-12"
    >
      <div class="mx-auto flex h-16 w-16 items-center justify-center rounded-2xl bg-emerald-100 text-emerald-800">
        <CheckCircle2 class="h-8 w-8" />
      </div>

      <h2 class="mt-5 font-serif text-3xl font-extrabold text-stone-900">
        Order Placed Successfully!
      </h2>

      <p class="mt-3 text-base text-stone-600 max-w-md mx-auto">
        {{ successMessage }}
      </p>

      <div v-if="orderId" class="mt-6 inline-block rounded-2xl bg-stone-50 border border-stone-200 px-6 py-3 text-xs text-stone-600">
        <span>Order Reference: </span>
        <strong class="font-mono text-stone-900">{{ orderId }}</strong>
      </div>

      <div class="mt-8 flex flex-wrap justify-center gap-4">
        <router-link
          to="/my-orders"
          class="inline-flex items-center gap-2 rounded-2xl bg-stone-900 px-6 py-3.5 text-sm font-bold text-white shadow-md hover:bg-emerald-950 transition"
        >
          <Package class="h-4 w-4" />
          <span>View My Orders</span>
        </router-link>

        <router-link
          to="/products"
          class="inline-flex items-center gap-2 rounded-2xl border border-stone-200 bg-white px-6 py-3.5 text-sm font-semibold text-stone-700 hover:bg-stone-50 transition"
        >
          <span>Continue Shopping</span>
        </router-link>
      </div>
    </div>

    <!-- Payment Retry Screen (Order exists, retry payment) -->
    <div
      v-else-if="orderId"
      class="rounded-3xl border border-amber-200 bg-white p-8 shadow-sm sm:p-10"
    >
      <div class="flex items-center gap-3 text-amber-800">
        <AlertCircle class="h-6 w-6" />
        <h2 class="font-serif text-2xl font-bold">Complete Your Payment</h2>
      </div>

      <p class="mt-3 text-sm text-stone-600">
        Your order (#<span class="font-mono font-bold">{{ orderId }}</span>) has been generated in the system. Please retry processing your payment below.
      </p>

      <div
        v-if="errorMessage"
        class="mt-4 rounded-2xl border border-rose-200 bg-rose-50 p-4 text-xs font-bold text-rose-800"
      >
        {{ errorMessage }}
      </div>

      <button
        type="button"
        class="mt-6 flex w-full items-center justify-center gap-2 rounded-2xl bg-emerald-800 px-6 py-4 font-bold text-white shadow-md hover:bg-emerald-900 disabled:opacity-50 transition"
        :disabled="isSubmitting"
        @click="PlaceOrder"
      >
        <span v-if="isSubmitting" class="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
        <span>{{ isSubmitting ? 'Processing Payment...' : 'Retry Payment' }}</span>
      </button>
    </div>

    <!-- Empty Cart Screen -->
    <div
      v-else-if="cartStore.items.length === 0"
      class="rounded-3xl border border-stone-200 bg-white p-12 text-center shadow-xs"
    >
      <h2 class="font-serif text-xl font-bold text-stone-900">Your cart is empty</h2>
      <p class="mt-2 text-sm text-stone-500">Add products before proceeding to checkout.</p>
      <router-link
        to="/products"
        class="mt-6 inline-flex items-center gap-2 rounded-2xl bg-stone-900 px-6 py-3 text-sm font-bold text-white hover:bg-stone-800"
      >
        <span>Browse Products</span>
      </router-link>
    </div>

    <!-- Active Checkout Form -->
    <div
      v-else
      class="grid grid-cols-1 gap-8 lg:grid-cols-12"
    >
      <!-- Payment Options & Actions (7 cols) -->
      <div class="lg:col-span-7 space-y-6">
        <!-- Payment Methods Card -->
        <div class="rounded-3xl border border-stone-200 bg-white p-6 shadow-xs">
          <h2 class="font-serif text-xl font-bold text-stone-900">
            Select Payment Method
          </h2>
          <p class="mt-1 text-xs text-stone-500">
            Choose how you would like to settle your order.
          </p>

          <div class="mt-5 space-y-3">
            <!-- Simulated Payment Option -->
            <label
              class="flex cursor-pointer items-start gap-4 rounded-2xl border p-4 transition"
              :class="[
                selectedPaymentMethod === PaymentMethod.FakePayment
                  ? 'border-emerald-800 bg-emerald-50/40 ring-1 ring-emerald-800'
                  : 'border-stone-200 hover:border-stone-300'
              ]"
            >
              <input
                v-model="selectedPaymentMethod"
                type="radio"
                :value="PaymentMethod.FakePayment"
                :disabled="isSubmitting"
                class="mt-1 h-4 w-4 border-stone-300 text-emerald-800 focus:ring-emerald-700"
              />
              <div class="flex-1">
                <div class="flex items-center gap-2">
                  <CreditCard class="h-4 w-4 text-emerald-800" />
                  <span class="font-bold text-stone-900 text-sm">Online Payment (Simulated)</span>
                </div>
                <p class="mt-1 text-xs text-stone-500 leading-relaxed">
                  Fast, instant simulated card payment for instant order confirmation.
                </p>
              </div>
            </label>

            <!-- Cash on Delivery Option -->
            <label
              class="flex cursor-pointer items-start gap-4 rounded-2xl border p-4 transition"
              :class="[
                selectedPaymentMethod === PaymentMethod.CashOnDelivery
                  ? 'border-emerald-800 bg-emerald-50/40 ring-1 ring-emerald-800'
                  : 'border-stone-200 hover:border-stone-300'
              ]"
            >
              <input
                v-model="selectedPaymentMethod"
                type="radio"
                :value="PaymentMethod.CashOnDelivery"
                :disabled="isSubmitting"
                class="mt-1 h-4 w-4 border-stone-300 text-emerald-800 focus:ring-emerald-700"
              />
              <div class="flex-1">
                <div class="flex items-center gap-2">
                  <Banknote class="h-4 w-4 text-amber-800" />
                  <span class="font-bold text-stone-900 text-sm">Cash on Delivery (COD)</span>
                </div>
                <p class="mt-1 text-xs text-stone-500 leading-relaxed">
                  Pay with physical cash upon delivery directly to the courier agent.
                </p>
              </div>
            </label>
          </div>
        </div>

        <!-- Error feedback -->
        <div
          v-if="errorMessage"
          class="flex items-start gap-3 rounded-2xl border border-rose-200 bg-rose-50 p-4 text-xs font-bold text-rose-800"
        >
          <AlertCircle class="h-4 w-4 shrink-0 mt-0.5" />
          <span>{{ errorMessage }}</span>
        </div>

        <!-- Place Order Button -->
        <div class="space-y-3">
          <button
            type="button"
            class="flex w-full items-center justify-center gap-2 rounded-2xl bg-emerald-800 px-8 py-4 font-bold text-white shadow-lg shadow-emerald-900/10 hover:bg-emerald-900 disabled:opacity-50 disabled:cursor-not-allowed transition active:scale-98"
            :disabled="isSubmitting || isLoading"
            @click="PlaceOrder"
          >
            <span v-if="isSubmitting" class="h-5 w-5 animate-spin rounded-full border-2 border-white border-t-transparent" />
            <span>{{ isSubmitting ? 'Confirming Order & Payment...' : 'Place Order Now' }}</span>
            <ArrowRight v-if="!isSubmitting" class="h-4 w-4" />
          </button>

          <router-link
            to="/cart"
            class="flex items-center justify-center gap-1.5 text-xs font-semibold text-stone-500 hover:text-stone-800 transition"
          >
            <ArrowLeft class="h-3.5 w-3.5" />
            <span>Return to Shopping Cart</span>
          </router-link>
        </div>
      </div>

      <!-- Order Summary Card (5 cols) -->
      <div class="lg:col-span-5">
        <div class="sticky top-24 rounded-3xl border border-stone-200 bg-white p-6 shadow-xs">
          <h2 class="font-serif text-lg font-bold text-stone-900 border-b border-stone-100 pb-4">
            Items in Your Order ({{ cartStore.totalItems }})
          </h2>

          <div class="mt-4 divide-y divide-stone-100 max-h-72 overflow-y-auto pr-1">
            <div
              v-for="item in cartStore.items"
              :key="item.productId"
              class="flex items-center justify-between gap-3 py-3"
            >
              <div class="flex items-center gap-3 min-w-0">
                <img
                  :src="getProductImageUrl(item.productName)"
                  :alt="item.productName"
                  class="h-12 w-12 rounded-xl object-cover bg-stone-100 shrink-0"
                />
                <div class="min-w-0">
                  <p class="text-sm font-bold text-stone-900 truncate">{{ item.productName }}</p>
                  <p class="text-xs text-stone-500">
                    {{ item.quantity }} × {{ formatPrice(item.unitPrice) }}
                  </p>
                </div>
              </div>

              <span class="text-sm font-bold text-stone-900 shrink-0">
                {{ formatPrice(item.totalPrice) }}
              </span>
            </div>
          </div>

          <div class="mt-6 border-t border-stone-200 pt-4 space-y-2 text-sm">
            <div class="flex justify-between text-stone-600">
              <span>Subtotal</span>
              <span class="font-semibold text-stone-900">{{ formatPrice(cartStore.totalPrice) }}</span>
            </div>
            <div class="flex justify-between text-stone-600">
              <span>Shipping</span>
              <span class="font-semibold text-emerald-800">FREE</span>
            </div>
          </div>

          <div class="mt-4 flex items-baseline justify-between border-t border-stone-200 pt-4">
            <span class="font-bold text-stone-900">Total Due</span>
            <span class="text-2xl font-extrabold text-stone-900">
              {{ formatPrice(cartStore.totalPrice) }}
            </span>
          </div>

          <div class="mt-6 flex items-center justify-center gap-2 text-2xs text-stone-400">
            <ShieldCheck class="h-4 w-4 text-emerald-700" />
            <span>Encrypted & secure order transmission</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>