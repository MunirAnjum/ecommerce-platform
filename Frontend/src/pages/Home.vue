<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import {
  ArrowRight,
  Sun,
  Mountain,
  ShieldCheck,
  HeartHandshake,
  Sparkles,
  ShoppingBag,
  CheckCircle2,
  Package,
} from 'lucide-vue-next'
import { getProducts } from '../services/productService'
import { getCategories } from '../services/categoryService'
import type { Product } from '../types/productType'
import type { CategoryResponse } from '../types/categoryType'
import { getCategoryImageUrl, isNutCategory } from '../utils/productImages'
import ProductCard from '../components/ProductCard.vue'
import Image from '../../public/images/dry_fruits.jpg'
//import Apricot from '../../public/images/Apricot.jpg'

interface CategoryWithCount {
  id: string
  name: string
  description?: string
  productCount: number
  image: string
  isNut: boolean
}

const products = ref<Product[]>([])
const rawCategories = ref<CategoryResponse[]>([])
const isLoading = ref(true)
const isCategoryLoading = ref(true)
const errorMessage = ref('')

async function loadData() {
  isLoading.value = true
  isCategoryLoading.value = true
  errorMessage.value = ''

  try {
    const [prodRes, catRes] = await Promise.allSettled([
      getProducts(),
      getCategories(),
    ])

    if (prodRes.status === 'fulfilled') {
      products.value = prodRes.value
    } else {
      console.error('Failed to load products:', prodRes.reason)
      errorMessage.value = 'Unable to load products at the moment.'
    }

    if (catRes.status === 'fulfilled') {
      rawCategories.value = catRes.value
    } else {
      console.warn('Failed to load categories directly:', catRes.reason)
    }
  } finally {
    isLoading.value = false
    isCategoryLoading.value = false
  }
}

// Compute live active categories with real customer-facing active product counts
const processedCategories = computed<CategoryWithCount[]>(() => {
  const activeProducts = products.value.filter(p => p.isActive !== false)

  if (rawCategories.value.length > 0) {
    return rawCategories.value.map(cat => {
      const count = activeProducts.filter(
        p =>
          p.categoryId === cat.id ||
          (p.categoryName && p.categoryName.toLowerCase() === cat.name.toLowerCase())
      ).length

      return {
        id: cat.id,
        name: cat.name,
        description: cat.description || '',
        productCount: count,
        image: getCategoryImageUrl(cat.name),
        isNut: isNutCategory(cat.name),
      }
    })
  }

  // Graceful fallback from products list if category endpoint returned empty
  const map = new Map<string, { id: string; name: string; count: number }>()
  activeProducts.forEach(p => {
    if (p.categoryName) {
      const existing = map.get(p.categoryName)
      if (existing) {
        existing.count++
      } else {
        map.set(p.categoryName, {
          id: p.categoryId,
          name: p.categoryName,
          count: 1,
        })
      }
    }
  })

  return Array.from(map.values()).map(c => ({
    id: c.id,
    name: c.name,
    description: '',
    productCount: c.count,
    image: getCategoryImageUrl(c.name),
    isNut: isNutCategory(c.name),
  }))
})

// Distinct classification: Dry Fruits vs Nuts
const dryFruitCategories = computed(() => processedCategories.value.filter(c => !c.isNut))
const nutCategories = computed(() => processedCategories.value.filter(c => c.isNut))
const homeCategories = computed(() =>
  [...dryFruitCategories.value, ...nutCategories.value].slice(0, 4)
)

onMounted(loadData)
</script>

<template>
  <div class="space-y-16 sm:space-y-24">
    <!-- Hero Section -->
    <section class="relative overflow-hidden bg-stone-900 text-white">
      <!-- Background Mountain Graphic / Photo -->
      <div class="absolute inset-0 z-0">
        <img
          :src="Image"
          alt="Karakoram Mountain Peaks in Baltistan"
          class="w-full h-full object-cover object-[center_20%] scale-x-[-1]"
        />
        <div class="absolute inset-0 bg-gradient-to-t from-stone-950 via-stone-900/80 to-transparent"></div>
      </div>

      <div class="relative z-10 mx-auto max-w-7xl px-4 py-24 sm:px-6 sm:py-32 lg:px-8">
        <div class="max-w-2xl">
          <div class="inline-flex items-center gap-2 rounded-full border border-amber-400/30 bg-amber-950/40 px-4 py-1.5 text-xs font-semibold text-amber-300 backdrop-blur-xs">
            <Sparkles class="h-3.5 w-3.5 text-amber-400" />
            <span>Direct from Skardu, Shigar & Hunza Valley</span>
          </div>

          <h1 class="mt-6 font-serif text-4xl font-extrabold tracking-tight text-white sm:text-5xl lg:text-6xl">
            Pure Mountain Sunshine in Every Harvest.
          </h1>

          <p class="mt-6 text-lg leading-relaxed text-stone-300">
            Hand-picked wild dry fruits from ancient mountain orchards at 2,500m+ elevation. Glacial-melt fed, naturally sun-cured, and unblemished by chemicals or preservatives.
          </p>

          <div class="mt-10 flex flex-wrap items-center gap-4">
            <router-link
              to="/products"
              class="inline-flex items-center gap-2 rounded-2xl bg-amber-500 px-7 py-3.5 text-base font-bold text-stone-950 shadow-lg shadow-amber-900/20 transition hover:bg-amber-400 active:scale-95"
            >
              <ShoppingBag class="h-5 w-5" />
              Shop The Harvest
            </router-link>

            <a
              href="#about-us"
              class="inline-flex items-center gap-2 rounded-2xl border border-stone-600 bg-stone-800/80 px-6 py-3.5 text-base font-semibold text-white backdrop-blur-xs transition hover:bg-stone-700"
            >
              <span>Our Heritage</span>
              <ArrowRight class="h-4 w-4" />
            </a>
          </div>
        </div>
      </div>
    </section>

    <!-- Value Proposition Pillars -->
    <section class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 -mt-8 sm:-mt-16 relative z-20">
      <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <div class="rounded-2xl border border-stone-200/80 bg-white p-6 shadow-lg shadow-stone-900/5">
          <div class="flex h-12 w-12 items-center justify-center rounded-xl bg-amber-50 text-amber-700">
            <Sun class="h-6 w-6" />
          </div>
          <h3 class="mt-4 font-bold text-stone-900">100% Sun-Dried</h3>
          <p class="mt-2 text-sm text-stone-600">
            Naturally sun-cured on high mountain rooftops under intense high-altitude sunlight.
          </p>
        </div>

        <div class="rounded-2xl border border-stone-200/80 bg-white p-6 shadow-lg shadow-stone-900/5">
          <div class="flex h-12 w-12 items-center justify-center rounded-xl bg-emerald-50 text-emerald-800">
            <Mountain class="h-6 w-6" />
          </div>
          <h3 class="mt-4 font-bold text-stone-900">Glacial Terroir</h3>
          <p class="mt-2 text-sm text-stone-600">
            Nourished by pristine mineral-rich glacial meltwater flowing from the Karakoram peaks.
          </p>
        </div>

        <div class="rounded-2xl border border-stone-200/80 bg-white p-6 shadow-lg shadow-stone-900/5">
          <div class="flex h-12 w-12 items-center justify-center rounded-xl bg-amber-50 text-amber-700">
            <ShieldCheck class="h-6 w-6" />
          </div>
          <h3 class="mt-4 font-bold text-stone-900">Zero Preservatives</h3>
          <p class="mt-2 text-sm text-stone-600">
            Free of sulfur dioxide, artificial glazes, color enhancers, and synthetic preservatives.
          </p>
        </div>

        <div class="rounded-2xl border border-stone-200/80 bg-white p-6 shadow-lg shadow-stone-900/5">
          <div class="flex h-12 w-12 items-center justify-center rounded-xl bg-emerald-50 text-emerald-800">
            <HeartHandshake class="h-6 w-6" />
          </div>
          <h3 class="mt-4 font-bold text-stone-900">Direct From Farmers</h3>
          <p class="mt-2 text-sm text-stone-600">
            Partnering directly with indigenous smallholder farming families across Baltistan.
          </p>
        </div>
      </div>
    </section>
  
    <!-- SHOP BY CATEGORY SECTION -->
    <section
      id="shop-by-category"
      class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8"
    >
      <!-- Section Header -->
      <div
        class="flex flex-col gap-4 border-b border-stone-200 pb-5 sm:flex-row sm:items-end sm:justify-between"
      >
        <div>
          <span
            class="text-xs font-bold uppercase tracking-widest text-amber-800"
          >
            Curated Baltistan Terroir
          </span>

          <h2
            class="mt-2 font-serif text-3xl font-extrabold text-stone-900 sm:text-4xl"
          >
            Shop by Category
          </h2>

          <p class="mt-1.5 text-sm text-stone-600">
            Explore authentic dry fruits and nuts from the mountains.
          </p>
        </div>

        <RouterLink
          to="/categories"
          class="inline-flex shrink-0 items-center gap-2 text-sm font-bold text-emerald-900 transition hover:text-emerald-700"
        >
          <span>View All Categories</span>
          <ArrowRight class="h-4 w-4" />
        </RouterLink>
      </div>

      <!-- Loading State -->
      <div
        v-if="isCategoryLoading"
        class="mt-6 grid grid-cols-2 gap-4 md:grid-cols-4"
      >
        <div
          v-for="i in 4"
          :key="i"
          class="animate-pulse overflow-hidden rounded-2xl border border-stone-200 bg-white p-3"
        >
          <div class="aspect-[4/3] rounded-xl bg-stone-200"></div>
          <div class="mt-3 h-5 w-2/3 rounded bg-stone-200"></div>
          <div class="mt-2 h-3 w-1/3 rounded bg-stone-200"></div>
        </div>
      </div>

      <!-- Four-Category Preview -->
      <div
        v-else-if="homeCategories.length > 0"
        class="mt-6 grid grid-cols-2 gap-4 md:grid-cols-4"
      >
        <RouterLink
          v-for="cat in homeCategories"
          :key="cat.id"
          :to="{
            path: '/products',
            query: { category: cat.name }
          }"
          class="group overflow-hidden rounded-2xl border border-stone-200/90 bg-white shadow-sm transition-all duration-300 hover:-translate-y-1 hover:border-amber-700/40 hover:shadow-lg"
        >
          <!-- Category Image -->
          <div class="relative aspect-[4/3] overflow-hidden bg-stone-100">
            <img
              :src="cat.image"
              :alt="cat.name"
              class="h-full w-full object-cover transition-transform duration-500 group-hover:scale-105"
              loading="lazy"
            />

            <div
              class="absolute inset-0 bg-gradient-to-t from-stone-950/60 to-transparent"
            ></div>

            <span
              class="absolute bottom-3 left-3 right-3 truncate font-serif text-lg font-bold text-white transition-colors group-hover:text-amber-300 sm:text-xl"
            >
              {{ cat.name }}
            </span>
          </div>

          <!-- Category Details -->
          <div class="flex items-center justify-between gap-2 p-3 sm:p-4">
            <span class="text-xs text-stone-600 sm:text-sm">
              {{ cat.productCount }}
              {{ cat.productCount === 1 ? 'product' : 'products' }}
            </span>

            <ArrowRight
              class="h-4 w-4 shrink-0 text-emerald-900 transition-transform group-hover:translate-x-1 group-hover:text-amber-700"
            />
          </div>
        </RouterLink>
      </div>

      <!-- Empty State -->
      <div
        v-else
        class="mt-6 rounded-2xl border border-stone-200 bg-white p-8 text-center"
      >
        <Package class="mx-auto h-8 w-8 text-stone-400" />

        <h3 class="mt-3 font-bold text-stone-900">
          No Categories Available
        </h3>

        <p class="mt-1 text-sm text-stone-500">
          Please check back later to explore our mountain harvest.
        </p>
      </div>
    </section>


    <!-- Featured Products from API -->
    <section class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
      <div class="flex flex-col sm:flex-row sm:items-end sm:justify-between gap-4">
        <div>
          <span class="text-xs font-bold uppercase tracking-widest text-amber-800">Fresh Stock</span>
          <h2 class="mt-2 font-serif text-3xl font-extrabold text-stone-900 sm:text-4xl">
            Featured Mountain Dry Fruits
          </h2>
        </div>

        <router-link
          to="/products"
          class="inline-flex items-center gap-1.5 text-sm font-bold text-emerald-900 hover:text-emerald-700 transition"
        >
          <span>Explore All Items</span>
          <ArrowRight class="h-4 w-4" />
        </router-link>
      </div>

      <!-- Loading skeleton -->
      <div v-if="isLoading" class="mt-8 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
        <div v-for="i in 3" :key="i" class="animate-pulse rounded-2xl border border-stone-200 bg-white p-5">
          <div class="aspect-4/3 rounded-xl bg-stone-200" />
          <div class="mt-4 h-5 w-2/3 rounded-md bg-stone-200" />
          <div class="mt-2 h-4 w-full rounded-md bg-stone-200" />
          <div class="mt-4 h-6 w-1/3 rounded-md bg-stone-200" />
        </div>
      </div>

      <!-- Error State -->
      <div
        v-else-if="errorMessage"
        class="mt-8 rounded-2xl border border-rose-200 bg-rose-50 p-6 text-center text-rose-800"
      >
        <p>{{ errorMessage }}</p>
        <button
          type="button"
          class="mt-3 rounded-xl bg-rose-800 px-4 py-2 text-xs font-semibold text-white hover:bg-rose-900"
          @click="loadData"
        >
          Try Again
        </button>
      </div>

      <!-- Products Grid -->
      <div
        v-else-if="products.length > 0"
        class="mt-8 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3"
      >
        <ProductCard
          v-for="product in products.slice(0, 6)"
          :key="product.id"
          :product="product"
        />
      </div>

      <div
        v-else
        class="mt-8 rounded-2xl border border-stone-200 bg-white p-12 text-center"
      >
        <p class="text-stone-600">No products currently available in the catalogue.</p>
      </div>
    </section>

    <!-- Heritage & Brand Story Section (Supports both #about-us and #heritage) -->
    <section id="about-us" class="relative overflow-hidden bg-stone-900 py-20 text-white scroll-mt-20">
      <div id="heritage" class="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div class="grid grid-cols-1 items-center gap-12 lg:grid-cols-2">
          <div>
            <span class="text-xs font-bold uppercase tracking-widest text-amber-400">Centuries of Tradition</span>
            <h2 class="mt-3 font-serif text-3xl font-extrabold sm:text-4xl">
              Grown Where Earth Touches The Heavens
            </h2>

            <p class="mt-6 text-base leading-relaxed text-stone-300">
              For generations, the people of Baltistan have cultivated sweet apricots, hearty almonds, and rich walnuts in terraced orchards carved into steep Karakoram granite.
            </p>

            <p class="mt-4 text-base leading-relaxed text-stone-300">
              At altitudes exceeding 2,500 meters, crisp mountain air and intense ultraviolet rays naturally concentrate vitamins, minerals, and rich natural sugars. Harvested each autumn, the fruit is spread atop clean stone rooftops to dry under the brilliant mountain sun.
            </p>

            <div class="mt-8 grid grid-cols-2 gap-6 border-t border-stone-800 pt-8">
              <div>
                <span class="block text-2xl font-extrabold text-amber-400">2,500m+</span>
                <span class="text-xs text-stone-400">Average Orchard Altitude</span>
              </div>
              <div>
                <span class="block text-2xl font-extrabold text-amber-400">100%</span>
                <span class="text-xs text-stone-400">Glacial Water Fed</span>
              </div>
            </div>
          </div>

          <div class="relative overflow-hidden rounded-3xl shadow-2xl">
            <img
              :src="Image"
              alt="Traditional sun-dried apricots of Baltistan"
              class="h-full w-full object-cover"
            />
          </div>
        </div>
      </div>
    </section>

    <!-- Packaging & Purity Commitment -->
    <section id="why-baltistan" class="mx-auto max-w-7xl px-4 pb-12 sm:px-6 lg:px-8">
      <div class="rounded-3xl border border-stone-200/90 bg-stone-50 p-8 sm:p-12 shadow-sm">
        <div class="max-w-3xl mx-auto text-center">
          <span class="text-xs font-bold uppercase tracking-widest text-amber-800">Our Quality Promise</span>
          <h2 class="mt-2 font-serif text-3xl font-extrabold text-stone-900">
            Airtight Barrier Pouches. Uncompromised Freshness.
          </h2>
          <p class="mt-4 text-stone-600 leading-relaxed">
            Every batch is double-sorted by hand, vacuum-packed in multi-layer foil craft pouches to lock in moisture, crunch, and authentic flavor from the Karakoram mountains directly to your home.
          </p>

          <div class="mt-8 flex flex-wrap justify-center gap-6 text-sm font-semibold text-stone-800">
            <div class="flex items-center gap-2">
              <CheckCircle2 class="h-4 w-4 text-emerald-700" />
              <span>Multi-layer moisture barrier</span>
            </div>
            <div class="flex items-center gap-2">
              <CheckCircle2 class="h-4 w-4 text-emerald-700" />
              <span>Zero chemical preservatives</span>
            </div>
            <div class="flex items-center gap-2">
              <CheckCircle2 class="h-4 w-4 text-emerald-700" />
              <span>Hand-sorted for quality</span>
            </div>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>
