<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { ArrowRight, Layers, Package, Sun } from 'lucide-vue-next'
import { getProducts } from '../services/productService'
import { getCategories } from '../services/categoryService'
import type { Product } from '../types/productType'
import type { CategoryResponse } from '../types/categoryType'
import { getCategoryImageUrl, isNutCategory } from '../utils/productImages'

type CategoryType = 'all' | 'dry-fruits' | 'nuts'
const props = defineProps<{
  categoryType: CategoryType
}>()

interface CategoryWithCount {
  id: string
  name: string
  description: string
  productCount: number
  image: string
  isNut: boolean
}

const products = ref<Product[]>([])
const rawCategories = ref<CategoryResponse[]>([])
const isLoading = ref(true)
const errorMessage = ref('')

const isNutsPage = computed(() => props.categoryType === 'nuts')
// const pageTitle = computed(() => isNutsPage.value ? 'Karakoram Mountain Nuts' : 'Mountain Dry Fruits')
const pageDescription = computed(() => isNutsPage.value
  ? 'Explore wholesome walnuts, almonds, chilgoza and other nuts from the Karakoram mountain region.'
  : 'Discover naturally sun-dried apricots, mulberries, figs and other traditional dry fruits from Baltistan.')
const pageEyebrow = computed(() => 'Baltistan Harvest')
const pageIcon = computed(() => isNutsPage.value ? Layers : Sun)

const processedCategories = computed<CategoryWithCount[]>(() => {
  const activeProducts = products.value.filter(product => product.isActive !== false)

  if (rawCategories.value.length > 0) {
    return rawCategories.value.map(category => {
      const productCount = activeProducts.filter(product =>
        product.categoryId === category.id ||
        (product.categoryName && product.categoryName.toLowerCase() === category.name.toLowerCase())
      ).length

      return {
        id: category.id,
        name: category.name,
        description: category.description || '',
        productCount,
        image: getCategoryImageUrl(category.name),
        isNut: isNutCategory(category.name),
      }
    })
  }

  // Fallback to categories represented by products if the category endpoint is unavailable or empty.
  const categoryMap = new Map<string, { id: string; name: string; count: number }>()
  activeProducts.forEach(product => {
    if (!product.categoryName) return
    const key = product.categoryName.toLowerCase()
    const existing = categoryMap.get(key)
    if (existing) {
      existing.count += 1
    } else {
      categoryMap.set(key, {
        id: product.categoryId,
        name: product.categoryName,
        count: 1,
      })
    }
  })

  return Array.from(categoryMap.values()).map(category => ({
    id: category.id,
    name: category.name,
    description: '',
    productCount: category.count,
    image: getCategoryImageUrl(category.name),
    isNut: isNutCategory(category.name),
  }))
})

// const visibleCategories = computed(() => processedCategories.value.filter(category =>
//   isNutsPage.value ? category.isNut : !category.isNut
// ))

const visibleCategories = computed(() => {
  if (props.categoryType === 'all') {
    return processedCategories.value
  }

  if (props.categoryType === 'nuts') {
    return processedCategories.value.filter(cat =>
      isNutCategory(cat.name)
    )
  }

  return processedCategories.value.filter(cat =>
    !isNutCategory(cat.name)
  )
})

async function loadData() {
  isLoading.value = true
  errorMessage.value = ''

  const [productResult, categoryResult] = await Promise.allSettled([
    getProducts(),
    getCategories(),
  ])

  if (productResult.status === 'fulfilled') {
    products.value = productResult.value
  } else {
    console.error('Failed to load products:', productResult.reason)
    errorMessage.value = 'Unable to load the catalogue right now. Please try again.'
  }

  if (categoryResult.status === 'fulfilled') {
    rawCategories.value = categoryResult.value
  } else {
    console.warn('Failed to load categories:', categoryResult.reason)
  }

  isLoading.value = false
}

onMounted(loadData)
</script>


<template>
  <main class="mx-auto max-w-7xl px-4 py-10 sm:px-6 sm:py-14 lg:px-8">
    <!-- Hero Banner -->
    <section
      class="relative mb-10 overflow-hidden rounded-3xl bg-stone-900 px-6 py-10 text-white shadow-lg sm:px-10 sm:py-14"
    >
      <div
        class="absolute inset-0 bg-gradient-to-br from-stone-900 via-stone-900 to-emerald-950"
      ></div>

      <div class="relative max-w-3xl">
        <div
          class="inline-flex items-center gap-2 rounded-full border border-amber-400/30 bg-amber-950/40 px-3 py-1.5 text-xs font-bold uppercase tracking-widest text-amber-300"
        >
          <component :is="pageIcon" class="h-4 w-4" />
          {{ pageEyebrow }}
        </div>

        <h1 class="mt-4 font-serif text-3xl font-extrabold sm:text-4xl lg:text-5xl">
          {{
            props.categoryType === 'all'
              ? 'All Categories'
              : props.categoryType === 'nuts'
                ? 'Nuts'
                : 'Dry Fruits'
          }}
        </h1>

        <p class="mt-4 max-w-2xl text-sm leading-relaxed text-stone-300 sm:text-base">
          {{ pageDescription }}
        </p>

        <router-link
          to="/products"
          class="mt-6 inline-flex items-center gap-2 rounded-xl bg-amber-500 px-5 py-3 text-sm font-bold text-stone-950 transition hover:bg-amber-400"
        >
          Browse all products
          <ArrowRight class="h-4 w-4" />
        </router-link>
      </div>
    </section>

    <!-- Category Heading -->
    <div
      class="mb-6 flex flex-col gap-2 border-b border-stone-200 pb-5 sm:flex-row sm:items-end sm:justify-between"
    >
      <div>
        <p class="text-xs font-bold uppercase tracking-widest text-amber-800">
          Choose a category
        </p>

        <h2 class="mt-2 font-serif text-2xl font-extrabold text-stone-900 sm:text-3xl">
          {{
            props.categoryType === 'all'
              ? 'Explore All Categories'
              : props.categoryType === 'nuts'
                ? 'Nuts'
                : 'Dry Fruits'
          }}
        </h2>
      </div>

      <p class="text-sm text-stone-500">
        {{ visibleCategories.length }}
        {{ visibleCategories.length === 1 ? 'category' : 'categories' }}
      </p>
    </div>

    <!-- Loading State -->
    <div
      v-if="isLoading"
      class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4"
    >
      <div
        v-for="item in 4"
        :key="item"
        class="animate-pulse overflow-hidden rounded-3xl border border-stone-200 bg-white p-4"
      >
        <div class="aspect-[4/3] rounded-2xl bg-stone-200"></div>
        <div class="mt-4 h-5 w-2/3 rounded bg-stone-200"></div>
        <div class="mt-3 h-4 w-1/3 rounded bg-stone-200"></div>
      </div>
    </div>

    <!-- Error State -->
    <div
      v-else-if="errorMessage"
      class="rounded-2xl border border-rose-200 bg-rose-50 p-8 text-center text-rose-800"
    >
      <p class="font-semibold">{{ errorMessage }}</p>

      <button
        type="button"
        class="mt-4 rounded-xl bg-rose-800 px-5 py-2.5 text-sm font-bold text-white hover:bg-rose-900"
        @click="loadData"
      >
        Try again
      </button>
    </div>

    <!-- All Categories / Dry Fruits / Nuts -->
    <div
      v-else-if="visibleCategories.length"
      class="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4"
    >
      <router-link
        v-for="cat in visibleCategories"
        :key="cat.id || cat.name"
        :to="{
          path: '/products',
          query: { category: cat.name }
        }"
        class="group flex flex-col overflow-hidden rounded-3xl border border-stone-200/90 bg-white shadow-sm transition duration-300 hover:-translate-y-1 hover:border-amber-700/40 hover:shadow-xl"
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
            class="absolute inset-0 bg-gradient-to-t from-stone-950/85 via-stone-950/20 to-transparent"
          ></div>

          <!-- Category Badge -->
          <span
            class="absolute left-3 top-3 inline-flex items-center gap-1 rounded-full px-3 py-1 text-xs font-bold shadow-sm backdrop-blur-sm"
            :class="
              isNutCategory(cat.name)
                ? 'bg-emerald-600/90 text-white'
                : 'bg-amber-500/90 text-stone-950'
            "
          >
            <component
              :is="isNutCategory(cat.name) ? Layers : Sun"
              class="h-3 w-3"
            />

            {{ isNutCategory(cat.name) ? 'Mountain Nut' : 'Dry Fruit' }}
          </span>

          <!-- Category Name and Product Count -->
          <div class="absolute bottom-3 left-3 right-3 flex items-end justify-between gap-2 text-white">
            <span
              class="font-serif text-2xl font-bold transition-colors group-hover:text-amber-300"
            >
              {{ cat.name }}
            </span>

            <span
              class="inline-flex shrink-0 items-center gap-1 rounded-full border border-stone-700 bg-stone-900/80 px-2.5 py-1 text-xs font-semibold text-stone-200"
            >
              <Package class="h-3 w-3 text-amber-400" />
              {{ cat.productCount }}
            </span>
          </div>
        </div>

        <!-- Category Description and Action -->
        <div class="flex flex-1 flex-col justify-between p-5">
          <p class="line-clamp-2 text-xs leading-relaxed text-stone-600">
            {{
              cat.description ||
              (
                isNutCategory(cat.name)
                  ? `Wholesome mountain ${cat.name.toLowerCase()} from the Karakoram region.`
                  : `Naturally sun-dried ${cat.name.toLowerCase()} harvested in the mountain valleys of Baltistan.`
              )
            }}
          </p>

          <div
            class="mt-4 flex items-center justify-between border-t border-stone-100 pt-3 text-xs font-bold text-emerald-900 transition-colors group-hover:text-amber-700"
          >
            <span>Explore {{ cat.name }}</span>

            <ArrowRight
              class="h-4 w-4 transition-transform group-hover:translate-x-1"
            />
          </div>
        </div>
      </router-link>
    </div>

    <!-- Empty State -->
    <div
      v-else
      class="rounded-3xl border border-stone-200 bg-white px-6 py-14 text-center"
    >
      <Package class="mx-auto h-10 w-10 text-stone-300" />

      <h3 class="mt-4 font-serif text-xl font-bold text-stone-900">
        No categories available yet
      </h3>

      <p class="mt-2 text-sm text-stone-500">
        Please check back later or browse the full catalogue.
      </p>

      <router-link
        to="/products"
        class="mt-5 inline-flex items-center gap-2 rounded-xl bg-stone-900 px-5 py-2.5 text-sm font-semibold text-white hover:bg-stone-800"
      >
        View all products
        <ArrowRight class="h-4 w-4" />
      </router-link>
    </div>
  </main>
</template>
