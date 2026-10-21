<script setup lang="ts">
import { computed } from 'vue'
import { brandConfig } from '../config/brand'

const props = withDefaults(
  defineProps<{
    size?: 'sm' | 'md' | 'lg'
    variant?: 'dark' | 'light' // 'dark' = for dark backgrounds (footer, admin sidebar), 'light' = for light backgrounds (navbar)
    showText?: boolean
  }>(),
  {
    size: 'md',
    variant: 'light',
    showText: true,
  }
)

const symbolSizeClass = computed(() => {
  switch (props.size) {
    case 'sm':
      return 'h-8 w-8'
    case 'lg':
      return 'h-14 w-14'
    default:
      return 'h-10 w-10 sm:h-11 sm:w-11'
  }
})

const titleSizeClass = computed(() => {
  switch (props.size) {
    case 'sm':
      return 'text-base font-bold'
    case 'lg':
      return 'text-2xl sm:text-3xl font-extrabold'
    default:
      return 'text-lg sm:text-xl font-bold'
  }
})

const subtitleSizeClass = computed(() => {
  switch (props.size) {
    case 'sm':
      return 'text-[9px] tracking-wider'
    case 'lg':
      return 'text-xs tracking-widest'
    default:
      return 'text-[10px] tracking-widest'
  }
})
</script>

<template>
  <div class="inline-flex items-center gap-3 select-none">
    <!-- Emblem SVG Symbol: Karakoram Peaks + Apricot & Nut Motif -->
    <div
      class="relative shrink-0 rounded-2xl shadow-sm transition-transform duration-300 group-hover:scale-105"
      :class="[
        symbolSizeClass,
        variant === 'dark' ? 'bg-[#183E2C] ring-1 ring-amber-400/30' : 'bg-[#12281D] ring-1 ring-stone-900/10'
      ]"
    >
      <svg
        viewBox="0 0 48 48"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
        class="h-full w-full p-1.5"
      >
        <!-- Defs / Gradients -->
        <defs>
          <linearGradient id="kh-mountain-grad" x1="0" y1="0" x2="48" y2="48" gradientUnits="userSpaceOnUse">
            <stop offset="0%" stop-color="#E0B84C" />
            <stop offset="100%" stop-color="#C59B27" />
          </linearGradient>
          <linearGradient id="kh-apricot-grad" x1="16" y1="20" x2="32" y2="38" gradientUnits="userSpaceOnUse">
            <stop offset="0%" stop-color="#F59E0B" />
            <stop offset="50%" stop-color="#D97706" />
            <stop offset="100%" stop-color="#B45309" />
          </linearGradient>
          <linearGradient id="kh-leaf-grad" x1="22" y1="12" x2="30" y2="24" gradientUnits="userSpaceOnUse">
            <stop offset="0%" stop-color="#4ADE80" />
            <stop offset="100%" stop-color="#15803D" />
          </linearGradient>
        </defs>

        <!-- Background Mountain Silhouette (Karakoram Ridges) -->
        <path
          d="M8 38L20 18L26 27L32 17L42 38H8Z"
          fill="#0B1A13"
          opacity="0.85"
        />
        
        <!-- Foreground Sharp Mountain Crest (K2-inspired Peak) -->
        <path
          d="M12 38L22 21L27 29L34 19L40 38"
          stroke="url(#kh-mountain-grad)"
          stroke-width="1.75"
          stroke-linecap="round"
          stroke-linejoin="round"
        />

        <!-- Mountain Snowcap Highlights -->
        <path
          d="M22 21L20.5 24L22.5 25.5L24 23.5L22 21Z"
          fill="#FDFBF7"
          opacity="0.9"
        />
        <path
          d="M34 19L32.5 22L34.5 23.5L35.5 21.5L34 19Z"
          fill="#FDFBF7"
          opacity="0.9"
        />

        <!-- Apricot / Nut Motif Nestled at Valley Base -->
        <!-- Apricot / Nut Body -->
        <path
          d="M24 26C20 26 17 29 17 33C17 37 20.5 40 24 40C27.5 40 31 37 31 33C31 29 28 26 24 26Z"
          fill="url(#kh-apricot-grad)"
        />
        
        <!-- Walnut / Fruit Suture Line -->
        <path
          d="M24 27C23.5 30 23.5 36 24 39.5"
          stroke="#78350F"
          stroke-width="1"
          stroke-linecap="round"
          opacity="0.65"
        />

        <!-- Little Organic Leaf Stem Atop -->
        <path
          d="M24 26C24 23.5 26.5 22 28.5 22.5C28.5 24.5 26.5 26 24 26Z"
          fill="url(#kh-leaf-grad)"
        />

        <!-- Radiant Sun / Glacial Sparkle -->
        <circle cx="28" cy="14" r="2.25" fill="#FDE68A" />
        <path d="M28 10V11M28 17V18M24 14H25M31 14H32" stroke="#FDE68A" stroke-width="1" stroke-linecap="round" />
      </svg>
    </div>

    <!-- Typography (Readable & Refined) -->
    <div v-if="showText" class="flex flex-col">
      <span
        class="font-serif tracking-tight leading-tight transition-colors"
        :class="[
          titleSizeClass,
          variant === 'dark' ? 'text-white' : 'text-stone-900 group-hover:text-emerald-900'
        ]"
      >
        {{ brandConfig.name }}
      </span>
      <span
        class="font-semibold uppercase transition-colors"
        :class="[
          subtitleSizeClass,
          variant === 'dark' ? 'text-amber-400/90' : 'text-amber-800'
        ]"
      >
        {{ brandConfig.tagline }}
      </span>
    </div>
  </div>
</template>
