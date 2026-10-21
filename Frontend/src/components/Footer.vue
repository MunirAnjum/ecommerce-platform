<script setup lang="ts">
import { ref } from 'vue'
import { Mail, Phone, MapPin, ShieldCheck, Sun, Heart, CheckCircle2 } from 'lucide-vue-next'
import { brandConfig } from '../config/brand'
import BrandLogo from './BrandLogo.vue'

const newsletterEmail = ref('')
const subscribed = ref(false)

function handleSubscribe(e: Event) {
  e.preventDefault()
  if (newsletterEmail.value && newsletterEmail.value.includes('@')) {
    subscribed.value = true
    newsletterEmail.value = ''
    setTimeout(() => {
      subscribed.value = false
    }, 4000)
  }
}
</script>

<template>
  <footer class="border-t border-stone-200 bg-stone-900 text-stone-300">
    <!-- Value proposition trust bar -->
    <div class="border-b border-stone-800 bg-stone-950/60 py-8">
      <div class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
          <div class="flex items-center gap-4">
            <div class="flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl bg-amber-900/30 text-amber-400 border border-amber-700/30">
              <Sun class="h-6 w-6" />
            </div>
            <div>
              <h4 class="font-bold text-white text-sm">Sun-Dried at 2,500m+</h4>
              <p class="text-xs text-stone-400">Naturally preserved by mountain sun</p>
            </div>
          </div>

          <div class="flex items-center gap-4">
            <div class="flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl bg-emerald-900/30 text-emerald-400 border border-emerald-700/30">
              <Mountain class="h-6 w-6" />
            </div>
            <div>
              <h4 class="font-bold text-white text-sm">Glacial Melt Nourished</h4>
              <p class="text-xs text-stone-400">Fed by pristine Karakoram waters</p>
            </div>
          </div>

          <div class="flex items-center gap-4">
            <div class="flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl bg-amber-900/30 text-amber-400 border border-amber-700/30">
              <ShieldCheck class="h-6 w-6" />
            </div>
            <div>
              <h4 class="font-bold text-white text-sm">Zero Sulfur & Additives</h4>
              <p class="text-xs text-stone-400">100% pure, chemical-free harvests</p>
            </div>
          </div>

          <div class="flex items-center gap-4">
            <div class="flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl bg-emerald-900/30 text-emerald-400 border border-emerald-700/30">
              <Heart class="h-6 w-6" />
            </div>
            <div>
              <h4 class="font-bold text-white text-sm">Fair Farmer Trade</h4>
              <p class="text-xs text-stone-400">Supporting indigenous Balti growers</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Main footer content -->
    <div class="mx-auto max-w-7xl px-4 py-16 sm:px-6 lg:px-8">
      <div class="grid grid-cols-1 gap-12 md:grid-cols-2 lg:grid-cols-5">
        <!-- Brand Summary -->
        <div class="lg:col-span-2">
          <BrandLogo size="md" variant="dark" />

          <p class="mt-4 max-w-md text-sm leading-relaxed text-stone-400">
            {{ brandConfig.description }}
          </p>

          <div class="mt-6 flex flex-col gap-2.5 text-xs text-stone-400">
            <div class="flex items-center gap-2">
              <MapPin class="h-4 w-4 text-amber-400 shrink-0" />
              <span>{{ brandConfig.origin }}</span>
            </div>
            <div class="flex items-center gap-2">
              <Mail class="h-4 w-4 text-amber-400 shrink-0" />
              <a :href="`mailto:${brandConfig.contactEmail}`" class="hover:text-white transition">
                {{ brandConfig.contactEmail }}
              </a>
            </div>
            <div class="flex items-center gap-2">
              <Phone class="h-4 w-4 text-amber-400 shrink-0" />
              <span>{{ brandConfig.contactPhone }}</span>
            </div>
          </div>
        </div>

        <!-- Quick Links -->
        <div>
          <h4 class="font-bold text-white text-sm tracking-wider uppercase">Storefront</h4>
          <ul class="mt-4 space-y-2.5 text-sm text-stone-400">
            <li>
              <router-link to="/" class="hover:text-white transition">Home</router-link>
            </li>
            <li>
              <router-link to="/products" class="hover:text-white transition">All Products</router-link>
            </li>
            <li>
              <router-link to="/cart" class="hover:text-white transition">Shopping Cart</router-link>
            </li>
            <li>
              <router-link to="/my-orders" class="hover:text-white transition">Track Orders</router-link>
            </li>
          </ul>
        </div>

        <!-- Sourcing Valleys -->
        <div>
          <h4 class="font-bold text-white text-sm tracking-wider uppercase">Origin Valleys</h4>
          <ul class="mt-4 space-y-2.5 text-sm text-stone-400">
            <li>
              <span class="hover:text-white transition cursor-default">Skardu Organic Belt</span>
            </li>
            <li>
              <span class="hover:text-white transition cursor-default">Shigar Valley Orchards</span>
            </li>
            <li>
              <span class="hover:text-white transition cursor-default">Khaplu Heritage Terraces</span>
            </li>
            <li>
              <span class="hover:text-white transition cursor-default">Hunza High-Altitude Groves</span>
            </li>
          </ul>
        </div>

        <!-- Newsletter Subscription -->
        <div>
          <h4 class="font-bold text-white text-sm tracking-wider uppercase">Seasonal Harvest</h4>
          <p class="mt-4 text-xs text-stone-400 leading-relaxed">
            Subscribe for notifications when fresh autumn harvests and rare Chilgoza batches arrive.
          </p>

          <form class="mt-4 space-y-2" @submit="handleSubscribe">
            <div class="relative">
              <input
                v-model="newsletterEmail"
                type="email"
                required
                placeholder="Enter your email"
                class="w-full rounded-xl border border-stone-700 bg-stone-800/80 px-3.5 py-2.5 text-sm text-white placeholder-stone-500 focus:border-amber-400 focus:outline-none focus:ring-1 focus:ring-amber-400"
              />
            </div>
            <button
              type="submit"
              class="w-full rounded-xl bg-amber-500 px-4 py-2 text-xs font-bold uppercase tracking-wider text-stone-950 transition hover:bg-amber-400"
            >
              Subscribe
            </button>
            <p v-if="subscribed" class="flex items-center gap-1.5 text-xs text-emerald-400 mt-2">
              <CheckCircle2 class="h-3.5 w-3.5" />
              <span>Thank you! You are now subscribed.</span>
            </p>
          </form>
        </div>
      </div>

      <!-- Copyright and disclaimer -->
      <div class="mt-16 border-t border-stone-800 pt-8 text-center text-xs text-stone-500">
        <p>
          &copy; {{ new Date().getFullYear() }} {{ brandConfig.name }}. All rights reserved. Sourced ethically from Gilgit-Baltistan, Pakistan.
        </p>
      </div>
    </div>
  </footer>
</template>
