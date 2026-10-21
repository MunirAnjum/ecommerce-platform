<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import {
  FolderTree,
  Plus,
  Edit2,
  Trash2,
  RefreshCw,
  Search,
  AlertCircle,
  CheckCircle2,
  Package,
  Sun,
  Layers,
  AlertTriangle,
} from 'lucide-vue-next'
import {
  getCategories,
  createCategory,
  updateCategory,
  deleteCategory,
} from '../services/categoryService'
import { getProducts } from '../services/productService'
import type { CategoryResponse } from '../types/categoryType'
import type { Product } from '../types/productType'
import { getCategoryImageUrl, isNutCategory } from '../utils/productImages'
import { formatDate } from '../utils/formatters'
import Modal from '../components/Modal.vue'

const categories = ref<CategoryResponse[]>([])
const products = ref<Product[]>([])
const isLoading = ref(true)
const errorMessage = ref('')
const successMessage = ref('')
const searchQuery = ref('')

// Create / Edit modal state
const isFormModalOpen = ref(false)
const isSubmitting = ref(false)
const editingCategory = ref<CategoryResponse | null>(null)
const formName = ref('')
const formDescription = ref('')
const validationErrors = ref<{ name?: string; description?: string }>({})

// Delete modal state
const isDeleteModalOpen = ref(false)
const deletingCategory = ref<CategoryResponse | null>(null)
const isDeleting = ref(false)
const deleteErrorMessage = ref('')

async function loadData() {
  isLoading.value = true
  errorMessage.value = ''
  try {
    const [catList, prodList] = await Promise.all([
      getCategories(),
      getProducts().catch(() => [] as Product[]),
    ])
    categories.value = catList
    products.value = prodList
  } catch (error: any) {
    console.error('Failed to load categories for admin:', error)
    errorMessage.value = 'Failed to load categories from Product microservice. Please verify service availability.'
  } finally {
    isLoading.value = false
  }
}

// Compute accurate associated product counts per category
const categoryProductCounts = computed<Record<string, number>>(() => {
  const map: Record<string, number> = {}
  categories.value.forEach(cat => {
    map[cat.id] = 0
  })

  products.value.forEach(prod => {
    if (prod.categoryId && map[prod.categoryId] !== undefined) {
      map[prod.categoryId]++
    } else if (prod.categoryName) {
      const match = categories.value.find(
        c => c.name.toLowerCase() === prod.categoryName.toLowerCase()
      )
      if (match) {
        map[match.id] = (map[match.id] || 0) + 1
      }
    }
  })

  return map
})

const filteredCategories = computed(() => {
  const q = searchQuery.value.trim().toLowerCase()
  if (!q) return categories.value
  return categories.value.filter(
    c =>
      c.name.toLowerCase().includes(q) ||
      (c.description && c.description.toLowerCase().includes(q))
  )
})

const dryFruitsCount = computed(() =>
  categories.value.filter(c => !isNutCategory(c.name)).length
)

const nutsCount = computed(() =>
  categories.value.filter(c => isNutCategory(c.name)).length
)

function openCreateModal() {
  editingCategory.value = null
  formName.value = ''
  formDescription.value = ''
  validationErrors.value = {}
  isFormModalOpen.value = true
}

function openEditModal(cat: CategoryResponse) {
  editingCategory.value = cat
  formName.value = cat.name
  formDescription.value = cat.description || ''
  validationErrors.value = {}
  isFormModalOpen.value = true
}

function validateForm(): boolean {
  validationErrors.value = {}
  let valid = true

  const name = formName.value.trim()
  if (!name) {
    validationErrors.value.name = 'Category name is required.'
    valid = false
  } else if (name.length > 150) {
    validationErrors.value.name = 'Category name cannot exceed 150 characters.'
    valid = false
  }

  if (formDescription.value.length > 500) {
    validationErrors.value.description = 'Description cannot exceed 500 characters.'
    valid = false
  }

  return valid
}

async function handleSaveCategory() {
  if (!validateForm()) return

  isSubmitting.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    if (editingCategory.value) {
      // Update
      const updated = await updateCategory(editingCategory.value.id, {
        name: formName.value.trim(),
        description: formDescription.value.trim(),
      })
      const index = categories.value.findIndex(c => c.id === editingCategory.value?.id)
      if (index !== -1) {
        categories.value[index] = updated
      }
      successMessage.value = `Category "${updated.name}" updated successfully.`
    } else {
      // Create
      const created = await createCategory({
        name: formName.value.trim(),
        description: formDescription.value.trim(),
      })
      categories.value.push(created)
      successMessage.value = `Category "${created.name}" created successfully.`
    }

    isFormModalOpen.value = false
    // Reload full list to sync with backend
    await loadData()
  } catch (error: any) {
    console.error('Failed to save category:', error)
    if (error.response?.data?.errors) {
      errorMessage.value = Object.values(error.response.data.errors).flat().join(' ')
    } else if (error.response?.data?.message) {
      errorMessage.value = error.response.data.message
    } else {
      errorMessage.value = 'Failed to save category. Please check your inputs and try again.'
    }
  } finally {
    isSubmitting.value = false
  }
}

function openDeleteModal(cat: CategoryResponse) {
  deletingCategory.value = cat
  deleteErrorMessage.value = ''
  isDeleteModalOpen.value = true
}

async function handleConfirmDelete() {
  if (!deletingCategory.value) return

  const catId = deletingCategory.value.id
  const prodCount = categoryProductCounts.value[catId] || 0

  if (prodCount > 0) {
    deleteErrorMessage.value = `Cannot delete "${deletingCategory.value.name}": this category contains ${prodCount} product(s). The database restricts deletion of categories that have existing products. Please reassign or delete these products first.`
    return
  }

  isDeleting.value = true
  deleteErrorMessage.value = ''
  try {
    await deleteCategory(catId)
    categories.value = categories.value.filter(c => c.id !== catId)
    successMessage.value = `Category "${deletingCategory.value.name}" deleted successfully.`
    isDeleteModalOpen.value = false
    deletingCategory.value = null
  } catch (error: any) {
    console.error('Failed to delete category:', error)
    if (error.response?.data?.message) {
      deleteErrorMessage.value = error.response.data.message
    } else if (error.response?.status === 500 || error.response?.status === 400 || error.response?.status === 409) {
      deleteErrorMessage.value = `Cannot delete category: The backend prevented deletion due to active associations or database constraint rules.`
    } else {
      deleteErrorMessage.value = 'Failed to delete category. Verify your network connection and administrator privileges.'
    }
  } finally {
    isDeleting.value = false
  }
}

onMounted(loadData)
</script>

<template>
  <div class="space-y-6">
    <!-- Page Header -->
    <div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h1 class="font-serif text-2xl font-bold text-stone-900 sm:text-3xl">
          Categories Management
        </h1>
        <p class="mt-1 text-sm text-stone-500">
          Organize Karakoram Harvest dry fruit varieties and mountain nut classifications.
        </p>
      </div>

      <div class="flex items-center gap-3">
        <button
          type="button"
          class="inline-flex items-center gap-2 rounded-xl border border-stone-200 bg-white px-3.5 py-2 text-xs font-semibold text-stone-700 shadow-2xs hover:bg-stone-50"
          :disabled="isLoading"
          @click="loadData"
        >
          <RefreshCw class="h-3.5 w-3.5" :class="{ 'animate-spin': isLoading }" />
          <span>Refresh</span>
        </button>

        <button
          type="button"
          class="inline-flex items-center gap-2 rounded-xl bg-amber-500 px-4 py-2 text-xs font-bold text-stone-950 shadow-md shadow-amber-900/10 transition hover:bg-amber-400 active:scale-95"
          @click="openCreateModal"
        >
          <Plus class="h-4 w-4" />
          <span>Add Category</span>
        </button>
      </div>
    </div>

    <!-- Alert Banners -->
    <div
      v-if="errorMessage"
      class="flex items-start gap-3 rounded-2xl border border-rose-200 bg-rose-50 p-4 text-xs text-rose-800"
    >
      <AlertCircle class="h-4 w-4 shrink-0 text-rose-600 mt-0.5" />
      <div class="flex-1">
        <p class="font-semibold">Operation Error</p>
        <p class="mt-0.5">{{ errorMessage }}</p>
      </div>
      <button type="button" class="text-rose-600 hover:text-rose-800" @click="errorMessage = ''">✕</button>
    </div>

    <div
      v-if="successMessage"
      class="flex items-start gap-3 rounded-2xl border border-emerald-200 bg-emerald-50 p-4 text-xs text-emerald-800"
    >
      <CheckCircle2 class="h-4 w-4 shrink-0 text-emerald-600 mt-0.5" />
      <div class="flex-1">
        <p class="font-semibold">Success</p>
        <p class="mt-0.5">{{ successMessage }}</p>
      </div>
      <button type="button" class="text-emerald-600 hover:text-emerald-800" @click="successMessage = ''">✕</button>
    </div>

    <!-- Quick Stats Metric Cards -->
    <div class="grid grid-cols-2 gap-4 sm:grid-cols-4">
      <div class="rounded-2xl border border-stone-200 bg-white p-4 shadow-2xs">
        <span class="text-xs font-medium text-stone-500">Total Categories</span>
        <div class="mt-2 flex items-baseline gap-2">
          <span class="font-serif text-2xl font-bold text-stone-900">{{ categories.length }}</span>
        </div>
      </div>

      <div class="rounded-2xl border border-stone-200 bg-white p-4 shadow-2xs">
        <span class="text-xs font-medium text-stone-500">Mountain Dry Fruits</span>
        <div class="mt-2 flex items-baseline gap-2">
          <span class="font-serif text-2xl font-bold text-amber-700">{{ dryFruitsCount }}</span>
        </div>
      </div>

      <div class="rounded-2xl border border-stone-200 bg-white p-4 shadow-2xs">
        <span class="text-xs font-medium text-stone-500">Karakoram Nuts</span>
        <div class="mt-2 flex items-baseline gap-2">
          <span class="font-serif text-2xl font-bold text-emerald-800">{{ nutsCount }}</span>
        </div>
      </div>

      <div class="rounded-2xl border border-stone-200 bg-white p-4 shadow-2xs">
        <span class="text-xs font-medium text-stone-500">Associated Products</span>
        <div class="mt-2 flex items-baseline gap-2">
          <span class="font-serif text-2xl font-bold text-stone-900">{{ products.length }}</span>
        </div>
      </div>
    </div>

    <!-- Filter and Search Bar -->
    <div class="flex items-center gap-4 rounded-2xl border border-stone-200 bg-white p-3 shadow-2xs">
      <div class="relative flex-1">
        <Search class="absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-stone-400" />
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Filter categories by name or description..."
          class="w-full rounded-xl border-0 bg-transparent py-1.5 pl-10 pr-4 text-xs text-stone-900 placeholder:text-stone-400 focus:outline-none focus:ring-0"
        />
      </div>
    </div>

    <!-- Categories Table -->
    <div class="overflow-hidden rounded-2xl border border-stone-200 bg-white shadow-2xs">
      <div v-if="isLoading" class="p-8 text-center">
        <RefreshCw class="mx-auto h-6 w-6 animate-spin text-stone-400" />
        <p class="mt-2 text-xs text-stone-500">Loading categories from Product microservice...</p>
      </div>

      <div v-else-if="filteredCategories.length === 0" class="p-12 text-center">
        <FolderTree class="mx-auto h-12 w-12 text-stone-300" />
        <h3 class="mt-3 font-serif text-base font-bold text-stone-800">No categories found</h3>
        <p class="mt-1 text-xs text-stone-500">
          {{ searchQuery ? 'Try adjusting your search query.' : 'Create your first category to get started.' }}
        </p>
        <button
          v-if="!searchQuery"
          type="button"
          class="mt-4 inline-flex items-center gap-1.5 rounded-xl bg-amber-500 px-4 py-2 text-xs font-bold text-stone-950 hover:bg-amber-400"
          @click="openCreateModal"
        >
          <Plus class="h-4 w-4" />
          <span>Create Category</span>
        </button>
      </div>

      <div v-else class="overflow-x-auto">
        <table class="w-full text-left text-xs">
          <thead class="border-b border-stone-200 bg-stone-50/75 text-stone-500">
            <tr>
              <th class="px-5 py-3 font-semibold">Category</th>
              <th class="px-5 py-3 font-semibold">Classification</th>
              <th class="px-5 py-3 font-semibold">Description</th>
              <th class="px-5 py-3 font-semibold text-center">Associated Products</th>
              <th class="px-5 py-3 font-semibold">Status</th>
              <th class="px-5 py-3 font-semibold">Created</th>
              <th class="px-5 py-3 font-semibold text-right">Actions</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-stone-100">
            <tr
              v-for="cat in filteredCategories"
              :key="cat.id"
              class="hover:bg-stone-50/60 transition"
            >
              <!-- Category Name with visual avatar -->
              <td class="px-5 py-4">
                <div class="flex items-center gap-3">
                  <div class="h-10 w-10 shrink-0 overflow-hidden rounded-xl border border-stone-200 bg-stone-100">
                    <img
                      :src="getCategoryImageUrl(cat.name)"
                      :alt="cat.name"
                      class="h-full w-full object-cover"
                    />
                  </div>
                  <div>
                    <span class="font-bold text-stone-900 block text-sm">{{ cat.name }}</span>
                    <span class="text-[10px] text-stone-400 font-mono">{{ cat.id.slice(0, 8) }}...</span>
                  </div>
                </div>
              </td>

              <!-- Classification -->
              <td class="px-5 py-4">
                <span
                  class="inline-flex items-center gap-1 rounded-full px-2.5 py-1 text-[11px] font-semibold"
                  :class="[
                    isNutCategory(cat.name)
                      ? 'bg-emerald-50 text-emerald-800 border border-emerald-200'
                      : 'bg-amber-50 text-amber-800 border border-amber-200'
                  ]"
                >
                  <component :is="isNutCategory(cat.name) ? Layers : Sun" class="h-3 w-3" />
                  <span>{{ isNutCategory(cat.name) ? 'Karakoram Nut' : 'Dry Fruit' }}</span>
                </span>
              </td>

              <!-- Description -->
              <td class="px-5 py-4 max-w-xs">
                <p class="text-stone-600 line-clamp-2 leading-relaxed">
                  {{ cat.description || 'No description provided.' }}
                </p>
              </td>

              <!-- Associated Products count -->
              <td class="px-5 py-4 text-center">
                <span
                  class="inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-bold"
                  :class="[
                    (categoryProductCounts[cat.id] || 0) > 0
                      ? 'bg-stone-100 text-stone-900'
                      : 'bg-stone-50 text-stone-400'
                  ]"
                >
                  <Package class="h-3 w-3 text-amber-600" />
                  <span>{{ categoryProductCounts[cat.id] || 0 }}</span>
                </span>
              </td>

              <!-- Status -->
              <td class="px-5 py-4">
                <span
                  class="inline-flex items-center rounded-full px-2.5 py-0.5 text-[11px] font-semibold"
                  :class="cat.isActive ? 'bg-emerald-50 text-emerald-800' : 'bg-stone-100 text-stone-600'"
                >
                  {{ cat.isActive ? 'Active' : 'Inactive' }}
                </span>
              </td>

              <!-- Created At -->
              <td class="px-5 py-4 text-stone-500 whitespace-nowrap">
                {{ formatDate(cat.createAt) }}
              </td>

              <!-- Actions -->
              <td class="px-5 py-4 text-right">
                <div class="flex items-center justify-end gap-1.5">
                  <button
                    type="button"
                    class="rounded-lg p-1.5 text-stone-600 hover:bg-stone-100 hover:text-stone-900 transition"
                    title="Edit Category"
                    @click="openEditModal(cat)"
                  >
                    <Edit2 class="h-4 w-4" />
                  </button>

                  <button
                    type="button"
                    class="rounded-lg p-1.5 text-rose-600 hover:bg-rose-50 hover:text-rose-800 transition"
                    title="Delete Category"
                    @click="openDeleteModal(cat)"
                  >
                    <Trash2 class="h-4 w-4" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Create / Edit Category Modal -->
    <Modal
      :is-open="isFormModalOpen"
      :title="editingCategory ? 'Edit Category' : 'Create New Category'"
      @close="isFormModalOpen = false"
    >
      <form class="space-y-4" @submit.prevent="handleSaveCategory">
        <!-- Category Name -->
        <div>
          <div class="flex items-center justify-between">
            <label class="text-xs font-bold text-stone-800">
              Category Name <span class="text-rose-600">*</span>
            </label>
            <span class="text-[10px] text-stone-400">{{ formName.length }}/150</span>
          </div>
          <input
            v-model="formName"
            type="text"
            maxlength="150"
            placeholder="e.g. Baltistan Apricots, Kagzi Walnuts"
            class="mt-1.5 w-full rounded-xl border border-stone-200 px-3.5 py-2 text-xs text-stone-900 shadow-2xs focus:border-amber-700 focus:outline-none"
            :class="{ 'border-rose-400': validationErrors.name }"
            required
          />
          <p v-if="validationErrors.name" class="mt-1 text-[11px] text-rose-600">
            {{ validationErrors.name }}
          </p>
        </div>

        <!-- Description -->
        <div>
          <div class="flex items-center justify-between">
            <label class="text-xs font-bold text-stone-800">
              Description
            </label>
            <span class="text-[10px] text-stone-400">{{ formDescription.length }}/500</span>
          </div>
          <textarea
            v-model="formDescription"
            rows="3"
            maxlength="500"
            placeholder="Describe the regional origin, quality profile, and variety characteristics..."
            class="mt-1.5 w-full rounded-xl border border-stone-200 px-3.5 py-2 text-xs text-stone-900 shadow-2xs focus:border-amber-700 focus:outline-none"
            :class="{ 'border-rose-400': validationErrors.description }"
          ></textarea>
          <p v-if="validationErrors.description" class="mt-1 text-[11px] text-rose-600">
            {{ validationErrors.description }}
          </p>
        </div>

        <!-- Category Preview Callout -->
        <div v-if="formName" class="rounded-2xl border border-stone-200 bg-stone-50/75 p-3.5 flex items-center gap-3">
          <div class="h-10 w-10 shrink-0 overflow-hidden rounded-xl border border-stone-200 bg-white">
            <img
              :src="getCategoryImageUrl(formName)"
              :alt="formName"
              class="h-full w-full object-cover"
            />
          </div>
          <div>
            <span class="text-xs font-bold text-stone-900 block">{{ formName }}</span>
            <span class="text-[10px] text-stone-500">
              Categorized as: <strong class="text-stone-700">{{ isNutCategory(formName) ? 'Karakoram Nut' : 'Mountain Dry Fruit' }}</strong>
            </span>
          </div>
        </div>

        <!-- Modal Actions -->
        <div class="mt-6 flex items-center justify-end gap-3 border-t border-stone-100 pt-4">
          <button
            type="button"
            class="rounded-xl border border-stone-200 px-4 py-2 text-xs font-semibold text-stone-600 hover:bg-stone-50"
            @click="isFormModalOpen = false"
          >
            Cancel
          </button>

          <button
            type="submit"
            class="inline-flex items-center gap-2 rounded-xl bg-amber-500 px-5 py-2 text-xs font-bold text-stone-950 shadow-md shadow-amber-900/10 hover:bg-amber-400 disabled:opacity-50"
            :disabled="isSubmitting"
          >
            <RefreshCw v-if="isSubmitting" class="h-3.5 w-3.5 animate-spin" />
            <span>{{ isSubmitting ? 'Saving...' : editingCategory ? 'Save Changes' : 'Create Category' }}</span>
          </button>
        </div>
      </form>
    </Modal>

    <!-- Delete Confirmation Modal -->
    <Modal
      :is-open="isDeleteModalOpen"
      title="Delete Category"
      @close="isDeleteModalOpen = false"
    >
      <div v-if="deletingCategory" class="space-y-4">
        <!-- If category has associated products -->
        <div
          v-if="(categoryProductCounts[deletingCategory.id] || 0) > 0"
          class="rounded-2xl border border-amber-200 bg-amber-50 p-4 text-xs text-amber-900 space-y-2"
        >
          <div class="flex items-center gap-2 font-bold text-amber-800">
            <AlertTriangle class="h-4 w-4 shrink-0 text-amber-700" />
            <span>Deletion Restricted by Database Rules</span>
          </div>
          <p>
            Category <strong>"{{ deletingCategory.name }}"</strong> is currently assigned to
            <strong>{{ categoryProductCounts[deletingCategory.id] }} product(s)</strong>.
          </p>
          <p class="text-amber-800/80">
            The Product Service enforces foreign-key constraint restrictions (<code>DeleteBehavior.Restrict</code>). To prevent broken customer records, you must first reassign or remove all products belonging to this category before deleting it.
          </p>
        </div>

        <!-- Normal confirmation when product count is 0 -->
        <div v-else class="text-xs text-stone-600">
          <p>
            Are you sure you want to permanently delete category <strong>"{{ deletingCategory.name }}"</strong>?
          </p>
          <p class="mt-2 text-stone-500">
            This action cannot be undone.
          </p>
        </div>

        <!-- Delete Error Alert if any -->
        <div
          v-if="deleteErrorMessage"
          class="rounded-xl border border-rose-200 bg-rose-50 p-3 text-xs text-rose-800"
        >
          {{ deleteErrorMessage }}
        </div>

        <div class="mt-6 flex items-center justify-end gap-3 border-t border-stone-100 pt-4">
          <button
            type="button"
            class="rounded-xl border border-stone-200 px-4 py-2 text-xs font-semibold text-stone-600 hover:bg-stone-50"
            @click="isDeleteModalOpen = false"
          >
            Cancel
          </button>

          <button
            v-if="(categoryProductCounts[deletingCategory.id] || 0) === 0"
            type="button"
            class="inline-flex items-center gap-2 rounded-xl bg-rose-700 px-4 py-2 text-xs font-bold text-white shadow-md shadow-rose-900/10 hover:bg-rose-800 disabled:opacity-50"
            :disabled="isDeleting"
            @click="handleConfirmDelete"
          >
            <RefreshCw v-if="isDeleting" class="h-3.5 w-3.5 animate-spin" />
            <span>{{ isDeleting ? 'Deleting...' : 'Delete Category' }}</span>
          </button>

          <button
            v-else
            type="button"
            class="rounded-xl bg-stone-200 px-4 py-2 text-xs font-bold text-stone-500 cursor-not-allowed"
            disabled
          >
            Cannot Delete (Contains Products)
          </button>
        </div>
      </div>
    </Modal>
  </div>
</template>
