
import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import {
  getCart,
  addToCart as addToCartApi,
  updateCartItem,
  removeCartItem,
} from '../services/cartService'
import type { Cart, CartItem } from '../types/cartType'

export const useCartStore = defineStore('cart', () => {
  const cartId = ref<string | null>(null)
  const items = ref<CartItem[]>([])

  const totalItems = computed(() =>
    items.value.reduce((total, item) => total + item.quantity, 0)
  )

  const totalPrice = computed(() =>
    items.value.reduce((total, item) => total + item.totalPrice, 0)
  )

  function applyCart(cart: Cart) {
    cartId.value = cart.id
    items.value = cart.items
  }

  async function loadCart() {
    const cart = await getCart()
    applyCart(cart)
  }

  async function addToCart(productId: string) {
    const cart = await addToCartApi({
      productId,
      quantity: 1,
    })

    applyCart(cart)
  }

  async function increaseQuantity(productId: string) {
    const item = items.value.find(item => item.productId === productId)

    if (!item) return

    const cart = await updateCartItem(productId, {
      quantity: item.quantity + 1,
    })

    applyCart(cart)
  }

  async function decreaseQuantity(productId: string) {
    const item = items.value.find(item => item.productId === productId)

    if (!item) return

    if (item.quantity === 1) {
      await removeCartItem(productId)
      items.value = items.value.filter(item => item.productId !== productId)
      return
    }

    const cart = await updateCartItem(productId, {
      quantity: item.quantity - 1,
    })

    applyCart(cart)
  }

  async function removeFromCart(productId: string) {
    await removeCartItem(productId)
    items.value = items.value.filter(item => item.productId !== productId)
  }

  // Call only after the Order Service confirms successful order creation.
  function clearItemsAfterCheckout() {
    items.value = []
  }

  return {
    cartId,
    items,
    totalItems,
    totalPrice,
    loadCart,
    addToCart,
    increaseQuantity,
    decreaseQuantity,
    removeFromCart,
    clearItemsAfterCheckout,
  }
})
