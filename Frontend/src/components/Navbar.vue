<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import {
  ShoppingBag,
  Menu,
  X,
  ChevronDown,
  ShieldAlert,
  Package,
  LogOut,
  Sparkles,
} from 'lucide-vue-next'
import { brandConfig } from '../config/brand'
import { useAuthStore } from '../stores/auth'
import { useCartStore } from '../stores/cart'
import BrandLogo from './BrandLogo.vue'

const router = useRouter()
const authStore = useAuthStore()
const cartStore = useCartStore()

const isMobileMenuOpen = ref(false)
const isUserMenuOpen = ref(false)
const showAnnouncement = ref(true)

function logout() {
  authStore.logout()
  isUserMenuOpen.value = false
  isMobileMenuOpen.value = false
  router.push('/login')
}

function closeMobileMenu() {
  isMobileMenuOpen.value = false
}
</script>

<template>
  <header class="sticky top-0 z-40 w-full border-b border-stone-200/80 bg-stone-50/95 backdrop-blur-md transition-all">
    <!-- Announcement Bar -->
    <div
      v-if="showAnnouncement"
      class="relative bg-stone-900 px-4 py-2 text-center text-xs font-medium text-amber-100/90 sm:px-6"
    >
      <div class="mx-auto flex max-w-7xl items-center justify-center gap-2">
        <Sparkles class="h-3.5 w-3.5 text-amber-400 shrink-0" />
        <span class="truncate">{{ brandConfig.announcement }}</span>
      </div>
      <button
        type="button"
        class="absolute right-3 top-1/2 -translate-y-1/2 text-stone-400 hover:text-white"
        aria-label="Dismiss announcement"
        @click="showAnnouncement = false"
      >
        <X class="h-3.5 w-3.5" />
      </button>
    </div>

    <!-- Main Navigation Bar -->
    <div class="mx-auto flex max-w-7xl items-center justify-between px-4 py-3 sm:px-6 lg:px-8">
      <!-- Brand Logo -->
      <router-link
        to="/"
        class="group flex items-center transition"
        aria-label="Karakoram Harvest Home"
      >
        <BrandLogo size="md" variant="light" />
      </router-link>

      <!-- Desktop Navigation Links -->
      <nav class="hidden md:flex items-center gap-8 text-sm font-semibold text-stone-700">
        <router-link
          to="/"
          class="transition hover:text-emerald-800"
          active-class="text-emerald-800 font-bold"
        >
          Home
        </router-link>

        <router-link
          to="/dry-fruits"
          class="transition hover:text-emerald-800"
          active-class="text-emerald-800 font-bold"
        >
          Dry Fruits
        </router-link>

        <router-link
          to="/nuts"
          class="transition hover:text-emerald-800"
          active-class="text-emerald-800 font-bold"
        >
          Nuts
        </router-link>

        <router-link
          to="/products"
          class="transition hover:text-emerald-800"
          active-class="text-emerald-800 font-bold"
        >
          All Products
        </router-link>

        <router-link
          to="/#about-us"
          class="transition hover:text-emerald-800"
        >
          About Us
        </router-link>
      </nav>

      <!-- Right Action Controls (Cart, User, Admin) -->
      <div class="flex items-center gap-3 sm:gap-4">
        <!-- Cart Button -->
        <router-link
          to="/cart"
          class="relative flex h-10 items-center gap-2 rounded-xl border border-stone-200 bg-white px-3 text-stone-800 shadow-2xs transition hover:border-amber-700/40 hover:bg-stone-50"
          aria-label="View shopping cart"
        >
          <ShoppingBag class="h-5 w-5 text-stone-700" />
          <span class="hidden text-xs font-bold text-stone-900 sm:inline">Cart</span>
          <span
            v-if="cartStore.totalItems > 0"
            class="flex h-5 w-5 items-center justify-center rounded-full bg-emerald-800 text-2xs font-extrabold text-white"
          >
            {{ cartStore.totalItems }}
          </span>
        </router-link>

        <!-- Authenticated User Menu -->
        <div v-if="authStore.isAuthenticated" class="relative">
          <button
            type="button"
            class="flex items-center gap-2 rounded-xl border border-stone-200 bg-white px-3 py-2 text-sm font-medium text-stone-800 shadow-2xs transition hover:bg-stone-50"
            @click="isUserMenuOpen = !isUserMenuOpen"
          >
            <div class="flex h-6 w-6 items-center justify-center rounded-full bg-emerald-100 text-xs font-bold text-emerald-900">
              {{ (authStore.email || 'U').charAt(0).toUpperCase() }}
            </div>
            <span class="hidden max-w-28 truncate text-xs font-semibold sm:inline">{{ authStore.email }}</span>
            <ChevronDown class="h-3.5 w-3.5 text-stone-400" />
          </button>

          <!-- Dropdown Popup -->
          <div
            v-if="isUserMenuOpen"
            class="absolute right-0 mt-2 w-56 rounded-2xl border border-stone-200 bg-white p-2 shadow-xl z-50 animate-in fade-in slide-in-from-top-2 duration-150"
          >
            <div class="border-b border-stone-100 px-3 py-2">
              <p class="text-xs font-medium text-stone-400">Signed in as</p>
              <p class="truncate text-sm font-bold text-stone-900">{{ authStore.email }}</p>
              <span
                v-if="authStore.isAdmin"
                class="mt-1 inline-flex items-center gap-1 rounded-full bg-purple-100 px-2 py-0.5 text-2xs font-bold text-purple-900"
              >
                Administrator
              </span>
            </div>

            <div class="py-1">
              <router-link
                to="/my-orders"
                class="flex items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium text-stone-700 hover:bg-stone-50"
                @click="isUserMenuOpen = false"
              >
                <Package class="h-4 w-4 text-stone-500" />
                My Orders
              </router-link>

              <router-link
                v-if="authStore.isAdmin"
                to="/admin"
                class="flex items-center gap-2 rounded-lg px-3 py-2 text-sm font-semibold text-purple-700 hover:bg-purple-50"
                @click="isUserMenuOpen = false"
              >
                <ShieldAlert class="h-4 w-4 text-purple-600" />
                Admin Dashboard
              </router-link>
            </div>

            <div class="border-t border-stone-100 pt-1">
              <button
                type="button"
                class="flex w-full items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium text-rose-600 hover:bg-rose-50"
                @click="logout"
              >
                <LogOut class="h-4 w-4 text-rose-500" />
                Sign Out
              </button>
            </div>
          </div>
        </div>

        <!-- Guest Links -->
        <div v-else class="flex items-center gap-2">
          <router-link
            to="/login"
            class="rounded-xl px-3.5 py-2 text-sm font-semibold text-stone-700 hover:text-stone-900 hover:bg-stone-100 transition"
          >
            Sign In
          </router-link>
          <router-link
            to="/register"
            class="hidden sm:inline-flex rounded-xl bg-stone-900 px-4 py-2 text-sm font-semibold text-white shadow-xs transition hover:bg-emerald-950"
          >
            Register
          </router-link>
        </div>

        <!-- Mobile Menu Toggle Button -->
        <button
          type="button"
          class="flex h-10 w-10 items-center justify-center rounded-xl border border-stone-200 text-stone-700 md:hidden hover:bg-stone-100"
          aria-label="Toggle menu"
          @click="isMobileMenuOpen = !isMobileMenuOpen"
        >
          <Menu v-if="!isMobileMenuOpen" class="h-5 w-5" />
          <X v-else class="h-5 w-5" />
        </button>
      </div>
    </div>

    <!-- Mobile Drawer Menu -->
    <div
      v-if="isMobileMenuOpen"
      class="border-t border-stone-200 bg-stone-50 px-6 py-6 md:hidden animate-in slide-in-from-top-4 duration-200"
    >
      <nav class="flex flex-col gap-4 text-base font-semibold text-stone-800">
        <router-link
          to="/"
          class="flex items-center justify-between py-2 border-b border-stone-200/60"
          @click="closeMobileMenu"
        >
          <span>Home</span>
          <span class="text-stone-400">→</span>
        </router-link>

        <router-link
          to="/dry-fruits"
          class="flex items-center justify-between py-2 border-b border-stone-200/60"
          @click="closeMobileMenu"
        >
          <span>Dry Fruits</span>
          <span class="text-stone-400">→</span>
        </router-link>

        <router-link
          to="/nuts"
          class="flex items-center justify-between py-2 border-b border-stone-200/60"
          @click="closeMobileMenu"
        >
          <span>Nuts</span>
          <span class="text-stone-400">→</span>
        </router-link>

        <router-link
          to="/products"
          class="flex items-center justify-between py-2 border-b border-stone-200/60"
          @click="closeMobileMenu"
        >
          <span>All Products</span>
          <span class="text-stone-400">→</span>
        </router-link>

        <router-link
          to="/#about-us"
          class="flex items-center justify-between py-2 border-b border-stone-200/60"
          @click="closeMobileMenu"
        >
          <span>About Us</span>
          <span class="text-stone-400">→</span>
        </router-link>

        <router-link
          v-if="authStore.isAuthenticated"
          to="/my-orders"
          class="flex items-center justify-between py-2 border-b border-stone-200/60 text-emerald-800"
          @click="closeMobileMenu"
        >
          <span>My Orders</span>
          <span class="text-emerald-800">→</span>
        </router-link>

        <router-link
          v-if="authStore.isAdmin"
          to="/admin"
          class="flex items-center justify-between py-2 border-b border-stone-200/60 text-purple-700 font-bold"
          @click="closeMobileMenu"
        >
          <span>Admin Portal</span>
          <span class="text-purple-700">→</span>
        </router-link>

        <div v-if="!authStore.isAuthenticated" class="pt-2 flex flex-col gap-2">
          <router-link
            to="/login"
            class="rounded-xl border border-stone-300 py-2.5 text-center font-semibold text-stone-800"
            @click="closeMobileMenu"
          >
            Sign In
          </router-link>
          <router-link
            to="/register"
            class="rounded-xl bg-stone-900 py-2.5 text-center font-semibold text-white"
            @click="closeMobileMenu"
          >
            Create an Account
          </router-link>
        </div>

        <div v-else class="pt-2">
          <button
            type="button"
            class="w-full rounded-xl bg-rose-50 py-2.5 text-center font-semibold text-rose-700"
            @click="logout"
          >
            Sign Out
          </button>
        </div>
      </nav>
    </div>
  </header>
</template>
