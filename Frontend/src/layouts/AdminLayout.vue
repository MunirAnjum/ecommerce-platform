<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import {
  LayoutDashboard,
  Package,
  Layers,
  ShoppingBag,
  FolderTree,
  ExternalLink,
  LogOut,
  Menu,
  X,
} from 'lucide-vue-next'
import { useAuthStore } from '../stores/auth'
import BrandLogo from '../components/BrandLogo.vue'

const router = useRouter()
const authStore = useAuthStore()
const isMobileSidebarOpen = ref(false)

function handleLogout() {
  authStore.logout()
  router.push('/login')
}
</script>

<template>
  <div class="flex min-h-screen bg-stone-100 text-stone-900">
    <!-- Desktop Sidebar -->
    <aside class="hidden w-64 flex-col border-r border-stone-200 bg-stone-900 text-stone-300 lg:flex">
      <!-- Admin Header / Brand -->
      <div class="flex h-16 items-center border-b border-stone-800 px-5">
        <BrandLogo size="sm" variant="dark" />
      </div>

      <!-- Navigation Links -->
      <nav class="flex-1 space-y-1.5 px-4 py-6">
        <router-link
          to="/admin"
          exact-active-class="bg-stone-800 text-amber-400 font-semibold"
          class="flex items-center gap-3 rounded-xl px-3.5 py-2.5 text-sm font-medium text-stone-300 hover:bg-stone-800 hover:text-white transition"
        >
          <LayoutDashboard class="h-5 w-5 shrink-0" />
          Dashboard
        </router-link>

        <router-link
          to="/admin/products"
          active-class="bg-stone-800 text-amber-400 font-semibold"
          class="flex items-center gap-3 rounded-xl px-3.5 py-2.5 text-sm font-medium text-stone-300 hover:bg-stone-800 hover:text-white transition"
        >
          <Package class="h-5 w-5 shrink-0" />
          Products
        </router-link>

        <router-link
          to="/admin/categories"
          active-class="bg-stone-800 text-amber-400 font-semibold"
          class="flex items-center gap-3 rounded-xl px-3.5 py-2.5 text-sm font-medium text-stone-300 hover:bg-stone-800 hover:text-white transition"
        >
          <FolderTree class="h-5 w-5 shrink-0" />
          Categories
        </router-link>

        <router-link
          to="/admin/inventory"
          active-class="bg-stone-800 text-amber-400 font-semibold"
          class="flex items-center gap-3 rounded-xl px-3.5 py-2.5 text-sm font-medium text-stone-300 hover:bg-stone-800 hover:text-white transition"
        >
          <Layers class="h-5 w-5 shrink-0" />
          Inventory
        </router-link>

        <router-link
          to="/admin/orders"
          active-class="bg-stone-800 text-amber-400 font-semibold"
          class="flex items-center gap-3 rounded-xl px-3.5 py-2.5 text-sm font-medium text-stone-300 hover:bg-stone-800 hover:text-white transition"
        >
          <ShoppingBag class="h-5 w-5 shrink-0" />
          Orders
        </router-link>
      </nav>

      <!-- Bottom User / Logout Area -->
      <div class="border-t border-stone-800 p-4">
        <router-link
          to="/"
          class="flex items-center justify-between rounded-xl px-3 py-2 text-xs font-semibold text-stone-400 hover:bg-stone-800 hover:text-white transition"
        >
          <span>Storefront View</span>
          <ExternalLink class="h-3.5 w-3.5" />
        </router-link>

        <div class="mt-3 flex items-center justify-between border-t border-stone-800 pt-3">
          <div class="truncate">
            <p class="truncate text-xs font-semibold text-white">{{ authStore.email }}</p>
            <p class="text-2xs text-amber-400">System Admin</p>
          </div>
          <button
            type="button"
            class="rounded-lg p-1.5 text-stone-400 hover:bg-stone-800 hover:text-rose-400 transition"
            title="Sign Out"
            @click="handleLogout"
          >
            <LogOut class="h-4 w-4" />
          </button>
        </div>
      </div>
    </aside>

    <!-- Mobile Sidebar Backdrop & Drawer -->
    <div
      v-if="isMobileSidebarOpen"
      class="fixed inset-0 z-50 flex lg:hidden"
    >
      <div
        class="fixed inset-0 bg-stone-900/60 backdrop-blur-xs"
        @click="isMobileSidebarOpen = false"
      />
      <div class="relative flex w-64 flex-col bg-stone-900 p-6 text-stone-300">
        <div class="flex items-center justify-between pb-4 border-b border-stone-800">
          <BrandLogo size="sm" variant="dark" />
          <button
            type="button"
            class="rounded-lg p-1 text-stone-400 hover:text-white"
            @click="isMobileSidebarOpen = false"
          >
            <X class="h-5 w-5" />
          </button>
        </div>

        <nav class="mt-6 flex-1 space-y-2">
          <router-link
            to="/admin"
            class="flex items-center gap-3 rounded-xl px-3 py-2 text-sm font-medium hover:bg-stone-800"
            @click="isMobileSidebarOpen = false"
          >
            <LayoutDashboard class="h-5 w-5" />
            Dashboard
          </router-link>
          <router-link
            to="/admin/products"
            class="flex items-center gap-3 rounded-xl px-3 py-2 text-sm font-medium hover:bg-stone-800"
            @click="isMobileSidebarOpen = false"
          >
            <Package class="h-5 w-5" />
            Products
          </router-link>
          <router-link
            to="/admin/categories"
            class="flex items-center gap-3 rounded-xl px-3 py-2 text-sm font-medium hover:bg-stone-800"
            @click="isMobileSidebarOpen = false"
          >
            <FolderTree class="h-5 w-5" />
            Categories
          </router-link>
          <router-link
            to="/admin/inventory"
            class="flex items-center gap-3 rounded-xl px-3 py-2 text-sm font-medium hover:bg-stone-800"
            @click="isMobileSidebarOpen = false"
          >
            <Layers class="h-5 w-5" />
            Inventory
          </router-link>
          <router-link
            to="/admin/orders"
            class="flex items-center gap-3 rounded-xl px-3 py-2 text-sm font-medium hover:bg-stone-800"
            @click="isMobileSidebarOpen = false"
          >
            <ShoppingBag class="h-5 w-5" />
            Orders
          </router-link>
        </nav>

        <div class="border-t border-stone-800 pt-4">
          <router-link
            to="/"
            class="flex items-center justify-between py-2 text-xs font-semibold text-stone-400"
            @click="isMobileSidebarOpen = false"
          >
            <span>Back to Storefront</span>
            <ExternalLink class="h-3.5 w-3.5" />
          </router-link>
          <button
            type="button"
            class="mt-2 flex w-full items-center gap-2 py-2 text-xs font-semibold text-rose-400"
            @click="handleLogout"
          >
            <LogOut class="h-4 w-4" />
            Sign Out
          </button>
        </div>
      </div>
    </div>

    <!-- Main Content Area -->
    <div class="flex flex-1 flex-col overflow-hidden">
      <!-- Admin Topbar -->
      <header class="flex h-16 items-center justify-between border-b border-stone-200 bg-white px-4 sm:px-6 lg:px-8">
        <div class="flex items-center gap-3">
          <button
            type="button"
            class="rounded-lg p-2 text-stone-600 hover:bg-stone-100 lg:hidden"
            @click="isMobileSidebarOpen = true"
          >
            <Menu class="h-6 w-6" />
          </button>
          <h1 class="font-serif text-lg font-bold text-stone-900 sm:text-xl">
            Management Console
          </h1>
        </div>

        <div class="flex items-center gap-4">
          <router-link
            to="/"
            class="hidden items-center gap-1.5 rounded-xl border border-stone-200 bg-stone-50 px-3.5 py-1.5 text-xs font-semibold text-stone-700 hover:bg-stone-100 transition sm:flex"
          >
            <ExternalLink class="h-3.5 w-3.5" />
            Storefront
          </router-link>

          <div class="flex items-center gap-2">
            <div class="flex h-8 w-8 items-center justify-center rounded-full bg-purple-100 text-xs font-bold text-purple-900">
              A
            </div>
            <span class="hidden text-xs font-medium text-stone-700 sm:inline">{{ authStore.email }}</span>
          </div>
        </div>
      </header>

      <!-- Page Content Slot -->
      <main class="flex-1 overflow-y-auto p-4 sm:p-6 lg:p-8">
        <router-view />
      </main>
    </div>
  </div>
</template>
