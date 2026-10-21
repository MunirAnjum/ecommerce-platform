<script setup lang="ts">
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  Mail,
  Lock,
  Eye,
  EyeOff,
  ArrowRight,
  AlertCircle,
} from 'lucide-vue-next'
import { useAuthStore } from '../stores/auth'
import { brandConfig } from '../config/brand'
import BrandLogo from '../components/BrandLogo.vue'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const email = ref('')
const password = ref('')
const showPassword = ref(false)

const errorMessage = ref('')
const isLoading = ref(false)

function fillDemo(demoEmail: string, demoPass: string) {
  email.value = demoEmail
  password.value = demoPass
}

async function handleLogin() {
  errorMessage.value = ''
  isLoading.value = true

  try {
    await authStore.login({
      email: email.value,
      password: password.value,
    })

    const redirectPath = (route.query.redirect as string) || '/'
    await router.push(redirectPath)
  } catch (error) {
    console.error('Login error:', error)
    errorMessage.value = 'Invalid email or password. Please check your credentials.'
  } finally {
    isLoading.value = false
  }
}
</script>

<template>
  <div class="flex min-h-[80vh] items-center justify-center px-4 py-12 sm:px-6 lg:px-8">
    <div class="w-full max-w-md">
      <!-- Card Container -->
      <div class="rounded-3xl border border-stone-200/90 bg-white p-8 shadow-xl shadow-stone-900/5 sm:p-10">
        <!-- Logo & Title -->
        <div class="text-center">
          <router-link to="/" class="inline-flex items-center justify-center mb-2" aria-label="Karakoram Harvest Home">
            <BrandLogo size="lg" :show-text="false" />
          </router-link>

          <h2 class="mt-3 font-serif text-2xl font-bold tracking-tight text-stone-900 sm:text-3xl">
            Welcome to {{ brandConfig.name }}
          </h2>
          <p class="mt-1 text-xs text-amber-800 font-semibold tracking-wider uppercase">
            {{ brandConfig.tagline }}
          </p>
          <p class="mt-2 text-xs text-stone-500">
            Sign in to track orders, manage your cart, and access wholesale pricing.
          </p>
        </div>

        <!-- Form -->
        <form class="mt-8 space-y-4" @submit.prevent="handleLogin">
          <!-- Email Input -->
          <div>
            <label class="block text-xs font-bold uppercase tracking-wider text-stone-700">
              Email Address
            </label>
            <div class="relative mt-1.5">
              <Mail class="absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-stone-400" />
              <input
                v-model="email"
                type="email"
                required
                autocomplete="email"
                placeholder="name@example.com"
                class="w-full rounded-2xl border border-stone-200 bg-stone-50/50 pl-10 pr-4 py-3 text-sm text-stone-900 placeholder-stone-400 focus:border-amber-700 focus:bg-white focus:outline-none focus:ring-1 focus:ring-amber-700"
              />
            </div>
          </div>

          <!-- Password Input -->
          <div>
            <label class="block text-xs font-bold uppercase tracking-wider text-stone-700">
              Password
            </label>
            <div class="relative mt-1.5">
              <Lock class="absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-stone-400" />
              <input
                v-model="password"
                :type="showPassword ? 'text' : 'password'"
                required
                autocomplete="current-password"
                placeholder="••••••••"
                class="w-full rounded-2xl border border-stone-200 bg-stone-50/50 pl-10 pr-11 py-3 text-sm text-stone-900 placeholder-stone-400 focus:border-amber-700 focus:bg-white focus:outline-none focus:ring-1 focus:ring-amber-700"
              />
              <button
                type="button"
                class="absolute right-3.5 top-1/2 -translate-y-1/2 text-stone-400 hover:text-stone-700"
                aria-label="Toggle password visibility"
                @click="showPassword = !showPassword"
              >
                <EyeOff v-if="showPassword" class="h-4 w-4" />
                <Eye v-else class="h-4 w-4" />
              </button>
            </div>
          </div>

          <!-- Error Alert -->
          <div
            v-if="errorMessage"
            class="flex items-center gap-2 rounded-2xl border border-rose-200 bg-rose-50 p-3.5 text-xs font-bold text-rose-800"
          >
            <AlertCircle class="h-4 w-4 shrink-0" />
            <span>{{ errorMessage }}</span>
          </div>

          <!-- Submit Button -->
          <button
            type="submit"
            :disabled="isLoading"
            class="mt-2 flex w-full items-center justify-center gap-2 rounded-2xl bg-stone-900 py-3.5 text-sm font-bold text-white shadow-md hover:bg-emerald-950 disabled:opacity-50 transition active:scale-98"
          >
            <span v-if="isLoading" class="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
            <span>{{ isLoading ? 'Signing in...' : 'Sign In' }}</span>
            <ArrowRight v-if="!isLoading" class="h-4 w-4" />
          </button>
        </form>

        <!-- Development Quick-Fill Helper -->
        <div class="mt-6 rounded-2xl bg-stone-50 p-4 border border-stone-200/60 text-2xs text-stone-600">
          <p class="font-bold uppercase tracking-wider text-stone-500 mb-2">Development Accounts:</p>
          <div class="flex flex-wrap gap-2">
            <button
              type="button"
              class="rounded-lg bg-white px-2.5 py-1 font-mono font-semibold border border-stone-200 hover:bg-stone-100 transition"
              @click="fillDemo('admin@ecommerce.local', 'Admin123!')"
            >
              Fill Admin (admin@ecommerce.local)
            </button>
          </div>
        </div>

        <!-- Registration link -->
        <div class="mt-8 border-t border-stone-100 pt-6 text-center text-xs text-stone-500">
          <span>Don't have an account yet? </span>
          <router-link to="/register" class="font-bold text-emerald-800 hover:underline">
            Register here
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>