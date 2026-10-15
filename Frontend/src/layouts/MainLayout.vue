<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useCartStore } from '../stores/cart'

const router = useRouter()
const authStore = useAuthStore()
const cartStore = useCartStore()

function logout() {
  authStore.logout()
  router.push('/login')
}
</script>

<template>
  <div class="min-h-screen bg-gray-100">

    <header class="bg-white shadow">
      <div class="mx-auto flex max-w-7xl items-center justify-between px-6 py-4">

        <router-link
          to="/"
          class="text-xl font-bold text-blue-600"
        >
          E-Commerce
        </router-link>

        <nav class="flex items-center gap-6">

            <router-link
              to="/"
              class="text-gray-700 hover:text-blue-600"
            >
              Home
            </router-link>  

            <router-link
              to="/products"
              class="text-gray-700 hover:text-blue-600"
            >
              Products
            </router-link>

            <router-link
              to="/cart"
              class="text-gray-700 hover:text-blue-600"
              >
              Cart
              <span
                v-if="cartStore.totalItems > 0"
                class="ml-1 rounded-full bg-blue-600 px-2 py-1 text-xs text-white"
              >
                {{ cartStore.totalItems }}
              </span>
            </router-link>

            <router-link
              to="/my-orders"
              class="text-gray-700 hover:text-blue-600"
              >
              Orders
            </router-link>

            <template v-if="authStore.isAuthenticated"> 
              <button
                type="button"
                class="text-red-600 hover:text-red-700"
                @click="logout"
              >
                Logout
              </button> 
            </template> 
            
            <router-link
              v-else
              to="/login"
              class="text-blue-600 hover:text-blue-700"
            >
              Login
            </router-link>
        </nav>
      </div>
    </header>

    <main>
      <router-view />
    </main>

  </div>
</template>