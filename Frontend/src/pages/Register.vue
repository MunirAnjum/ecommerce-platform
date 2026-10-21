<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import {
  Mail,
  Lock,
  Eye,
  EyeOff,
  ArrowRight,
  AlertCircle,
  CheckCircle2,
} from 'lucide-vue-next'
import { useAuthStore } from '../stores/auth'
import { brandConfig } from '../config/brand'
import BrandLogo from '../components/BrandLogo.vue'

const router = useRouter()
const authStore = useAuthStore()

const firstName = ref('')
const lastName = ref('')
const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const showPassword = ref(false)

const errorMessage = ref('')
const successMessage = ref('')
const isLoading = ref(false)

async function handleRegister() {
  errorMessage.value = ''
  successMessage.value = ''

  if (password.value !== confirmPassword.value) {
    errorMessage.value = 'Passwords do not match.'
    return
  }

  if (password.value.length < 6) {
    errorMessage.value = 'Password must be at least 6 characters.'
    return
  }

  isLoading.value = true

  try {
    await authStore.register({
      firstName: firstName.value,
      lastName: lastName.value,
      email: email.value,
      password: password.value,
    })

    successMessage.value = 'Account created successfully! Signing in...'

    // Automatically log in
    await authStore.login({
      email: email.value,
      password: password.value,
    })

    setTimeout(() => {
      router.push('/')
    }, 1000)
  } catch (error: any) {
    console.error('Registration error:', error)
    const backendMessage = error.response?.data?.message || error.response?.data?.[0]?.errorMessage
    errorMessage.value = backendMessage || 'Registration failed. Please verify your information and try again.'
  } finally {
    isLoading.value = false
  }
}
</script>

<template>
  <div class="flex min-h-[85vh] items-center justify-center px-4 py-12 sm:px-6 lg:px-8">
    <div class="w-full max-w-md">
      <div class="rounded-3xl border border-stone-200/90 bg-white p-8 shadow-xl shadow-stone-900/5 sm:p-10">
        <!-- Logo & Header -->
        <div class="text-center">
          <router-link to="/" class="inline-flex items-center justify-center mb-2" aria-label="Karakoram Harvest Home">
            <BrandLogo size="lg" :show-text="false" />
          </router-link>

          <h2 class="mt-3 font-serif text-2xl font-bold tracking-tight text-stone-900 sm:text-3xl">
            Create an Account
          </h2>
          <p class="mt-1 text-xs text-amber-800 font-semibold tracking-wider uppercase">
            {{ brandConfig.tagline }}
          </p>
          <p class="mt-2 text-xs text-stone-500">
            Join {{ brandConfig.name }} for fresh harvest updates and seamless tracking.
          </p>
        </div>

        <!-- Form -->
        <form class="mt-8 space-y-4" @submit.prevent="handleRegister">
          <!-- Name fields -->
          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-xs font-bold uppercase tracking-wider text-stone-700">
                First Name
              </label>
              <div class="relative mt-1.5">
                <input
                  v-model="firstName"
                  type="text"
                  required
                  placeholder="Ali"
                  class="w-full rounded-2xl border border-stone-200 bg-stone-50/50 px-3.5 py-2.5 text-sm text-stone-900 placeholder-stone-400 focus:border-amber-700 focus:bg-white focus:outline-none focus:ring-1 focus:ring-amber-700"
                />
              </div>
            </div>

            <div>
              <label class="block text-xs font-bold uppercase tracking-wider text-stone-700">
                Last Name
              </label>
              <div class="relative mt-1.5">
                <input
                  v-model="lastName"
                  type="text"
                  required
                  placeholder="Khan"
                  class="w-full rounded-2xl border border-stone-200 bg-stone-50/50 px-3.5 py-2.5 text-sm text-stone-900 placeholder-stone-400 focus:border-amber-700 focus:bg-white focus:outline-none focus:ring-1 focus:ring-amber-700"
                />
              </div>
            </div>
          </div>

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
                class="w-full rounded-2xl border border-stone-200 bg-stone-50/50 pl-10 pr-4 py-2.5 text-sm text-stone-900 placeholder-stone-400 focus:border-amber-700 focus:bg-white focus:outline-none focus:ring-1 focus:ring-amber-700"
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
                autocomplete="new-password"
                placeholder="••••••••"
                class="w-full rounded-2xl border border-stone-200 bg-stone-50/50 pl-10 pr-11 py-2.5 text-sm text-stone-900 placeholder-stone-400 focus:border-amber-700 focus:bg-white focus:outline-none focus:ring-1 focus:ring-amber-700"
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

          <!-- Confirm Password Input -->
          <div>
            <label class="block text-xs font-bold uppercase tracking-wider text-stone-700">
              Confirm Password
            </label>
            <div class="relative mt-1.5">
              <Lock class="absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-stone-400" />
              <input
                v-model="confirmPassword"
                :type="showPassword ? 'text' : 'password'"
                required
                autocomplete="new-password"
                placeholder="••••••••"
                class="w-full rounded-2xl border border-stone-200 bg-stone-50/50 pl-10 pr-4 py-2.5 text-sm text-stone-900 placeholder-stone-400 focus:border-amber-700 focus:bg-white focus:outline-none focus:ring-1 focus:ring-amber-700"
              />
            </div>
          </div>

          <!-- Error Alert -->
          <div
            v-if="errorMessage"
            class="flex items-center gap-2 rounded-2xl border border-rose-200 bg-rose-50 p-3 text-xs font-bold text-rose-800"
          >
            <AlertCircle class="h-4 w-4 shrink-0" />
            <span>{{ errorMessage }}</span>
          </div>

          <!-- Success Alert -->
          <div
            v-if="successMessage"
            class="flex items-center gap-2 rounded-2xl border border-emerald-200 bg-emerald-50 p-3 text-xs font-bold text-emerald-800"
          >
            <CheckCircle2 class="h-4 w-4 shrink-0" />
            <span>{{ successMessage }}</span>
          </div>

          <!-- Submit Button -->
          <button
            type="submit"
            :disabled="isLoading"
            class="mt-2 flex w-full items-center justify-center gap-2 rounded-2xl bg-stone-900 py-3.5 text-sm font-bold text-white shadow-md hover:bg-emerald-950 disabled:opacity-50 transition active:scale-98"
          >
            <span v-if="isLoading" class="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
            <span>{{ isLoading ? 'Creating Account...' : 'Register' }}</span>
            <ArrowRight v-if="!isLoading" class="h-4 w-4" />
          </button>
        </form>

        <div class="mt-8 border-t border-stone-100 pt-6 text-center text-xs text-stone-500">
          <span>Already have an account? </span>
          <router-link to="/login" class="font-bold text-emerald-800 hover:underline">
            Sign In here
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>
