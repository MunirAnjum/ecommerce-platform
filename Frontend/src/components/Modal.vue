<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'
import { X } from 'lucide-vue-next'

const props = withDefaults(
  defineProps<{
    isOpen: boolean
    title?: string
    maxWidthClass?: string
  }>(),
  {
    isOpen: false,
    title: '',
    maxWidthClass: 'max-w-lg',
  }
)

const emit = defineEmits<{
  (e: 'close'): void
}>()

function handleKeydown(event: KeyboardEvent) {
  if (event.key === 'Escape' && props.isOpen) {
    emit('close')
  }
}

onMounted(() => {
  window.addEventListener('keydown', handleKeydown)
})

onUnmounted(() => {
  window.removeEventListener('keydown', handleKeydown)
})
</script>

<template>
  <Teleport to="body">
    <Transition
      enter-active-class="transition ease-out duration-200"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition ease-in duration-150"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div
        v-if="isOpen"
        class="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-6"
        role="dialog"
        aria-modal="true"
      >
        <!-- Backdrop -->
        <div
          class="fixed inset-0 bg-stone-900/60 backdrop-blur-xs transition-opacity"
          @click="emit('close')"
        />

        <!-- Modal panel -->
        <div
          class="relative w-full rounded-2xl bg-white p-6 shadow-2xl transition-all sm:p-8"
          :class="maxWidthClass"
        >
          <div class="flex items-center justify-between border-b border-stone-100 pb-4">
            <h3 v-if="title" class="text-xl font-bold text-stone-900">
              {{ title }}
            </h3>
            <div v-else />

            <button
              type="button"
              class="rounded-lg p-1.5 text-stone-400 hover:bg-stone-100 hover:text-stone-700 transition"
              aria-label="Close dialog"
              @click="emit('close')"
            >
              <X class="h-5 w-5" />
            </button>
          </div>

          <div class="mt-4">
            <slot />
          </div>

          <div v-if="$slots.footer" class="mt-6 border-t border-stone-100 pt-4">
            <slot name="footer" />
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>
