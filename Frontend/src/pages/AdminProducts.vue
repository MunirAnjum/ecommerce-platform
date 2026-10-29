<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import {
  Package,
  Plus,
  Edit2,
  Trash2,
  RefreshCw,
  Search,
  AlertCircle,
  CheckCircle2,
  UploadCloud,
  X,
  Loader2,
} from 'lucide-vue-next'
import {
  getProducts,
  createProduct,
  updateProduct,
  deleteProduct,
  uploadProductImage,
} from '../services/productService'
import { getCategories } from '../services/categoryService'
import type { CreateProductRequest, Product, UpdateProductRequest } from '../types/productType'
import type { CategoryResponse } from '../types/categoryType'
import { formatPrice } from '../utils/formatters'
import { getProductImageUrl } from '../utils/productImages'
import Modal from '../components/Modal.vue'

const products = ref<Product[]>([])
const availableCategories = ref<CategoryResponse[]>([])
const isLoading = ref(true)
const errorMessage = ref('')
const successMessage = ref('')
const searchQuery = ref('')

// Create / Edit modal state
const isFormModalOpen = ref(false)
const isSubmitting = ref(false)
const isUploadingImage = ref(false)
const editingProduct = ref<Product | null>(null)

// Form fields
const formName = ref('')
const formDescription = ref('')
const formPrice = ref<number | ''>('')
const formCategoryId = ref('')
const formIsActive = ref(true)

// Image upload state
const fileInputRef = ref<HTMLInputElement | null>(null)
const selectedFile = ref<File | null>(null)
const previewImageUrl = ref<string | null>(null)
const imageFileName = ref<string>('')
const imageError = ref<string>('')
const isDragging = ref(false)
const imageWasModified = ref(false)
const imageWasCleared = ref(false)

// Delete modal state
const isDeleteModalOpen = ref(false)
const deletingProduct = ref<Product | null>(null)
const isDeleting = ref(false)

const filteredProducts = computed(() => {
  const q = searchQuery.value.trim().toLowerCase()
  if (!q) return products.value
  return products.value.filter(
    p =>
      p.name.toLowerCase().includes(q) ||
      (p.description && p.description.toLowerCase().includes(q)) ||
      (p.categoryName && p.categoryName.toLowerCase().includes(q))
  )
})

async function loadData() {
  isLoading.value = true
  errorMessage.value = ''
  try {
    const [prodList, catList] = await Promise.all([
      getProducts(),
      getCategories().catch(() => [] as CategoryResponse[]),
    ])
    products.value = prodList
    availableCategories.value = catList
  } catch (error) {
    console.error('Failed to load products for admin:', error)
    errorMessage.value = 'Failed to load products list from Product microservice.'
  } finally {
    isLoading.value = false
  }
}

function processImageFile(file: File) {
  imageError.value = ''

  // Validate format
  const validTypes = ['image/jpeg', 'image/png', 'image/webp']
  if (!validTypes.includes(file.type)) {
    imageError.value = 'Invalid format. Only JPEG, PNG, and WebP images are supported.'
    return
  }

  // Validate size (< 5 MB)
  const maxSize = 5 * 1024 * 1024
  if (file.size > maxSize) {
    imageError.value = `File is too large (${(file.size / (1024 * 1024)).toFixed(1)}MB). Maximum supported size is 5MB.`
    return
  }

  selectedFile.value = file
  previewImageUrl.value = URL.createObjectURL(file)
  imageFileName.value = file.name
  imageWasModified.value = true
  imageWasCleared.value = false
}

function handleFileSelect(e: Event) {
  const target = e.target as HTMLInputElement
  const file = target.files?.[0]
  if (file) {
    processImageFile(file)
  }
}

function handleFileDrop(e: DragEvent) {
  isDragging.value = false
  const file = e.dataTransfer?.files?.[0]
  if (file) {
    processImageFile(file)
  }
}

function clearImage() {
  selectedFile.value = null
  previewImageUrl.value = null
  imageFileName.value = ''
  imageError.value = ''
  imageWasModified.value = true
  imageWasCleared.value = true
  if (fileInputRef.value) {
    fileInputRef.value.value = ''
  }
}

function openCreateModal() {
  editingProduct.value = null
  formName.value = ''
  formDescription.value = ''
  formPrice.value = ''
  selectedFile.value = null
  previewImageUrl.value = null
  imageFileName.value = ''
  imageError.value = ''
  imageWasModified.value = false
  imageWasCleared.value = false

  // Pick first available category or existing product's category
  const defaultCatId =
    availableCategories.value[0]?.id ||
    products.value.find(p => p.categoryId)?.categoryId ||
    '31962613-7bf8-450d-a5ac-339cf0a6cae0'

  formCategoryId.value = defaultCatId
  formIsActive.value = true
  isFormModalOpen.value = true
}

function openEditModal(prod: Product) {
  editingProduct.value = prod
  formName.value = prod.name
  formDescription.value = prod.description
  formPrice.value = prod.price
  formCategoryId.value = prod.categoryId
  formIsActive.value = prod.isActive
  selectedFile.value = null
  imageError.value = ''
  imageWasModified.value = false
  imageWasCleared.value = false

  if (prod.imageUrl) {
    previewImageUrl.value = prod.imageUrl
    imageFileName.value = 'Current Cloudinary Image'
  } else {
    previewImageUrl.value = null
    imageFileName.value = ''
  }

  isFormModalOpen.value = true
}

async function handleSaveProduct() {
  if (!formName.value || formPrice.value === '' || Number(formPrice.value) <= 0) {
    errorMessage.value = 'Please provide a valid product name and positive price.'
    return
  }

  isSubmitting.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    let finalImageUrl: string | null = null
    let finalImagePublicId: string | null = null

    // 1. If a new image was selected, upload directly to Cloudinary through the backend
    if (selectedFile.value) {
      isUploadingImage.value = true
      try {
        const uploadResult = await uploadProductImage(selectedFile.value)
        finalImageUrl = uploadResult.secureUrl
        finalImagePublicId = uploadResult.publicId
      } catch (uploadErr: any) {
        console.error('Cloudinary upload failed:', uploadErr)
        const msg =
          uploadErr.response?.data?.message ||
          'Failed to upload image to Cloudinary. Please verify backend Cloudinary configuration.'
        imageError.value = msg
        errorMessage.value = msg
        isSubmitting.value = false
        isUploadingImage.value = false
        return
      } finally {
        isUploadingImage.value = false
      }
    } else if (imageWasCleared.value) {
      finalImageUrl = null
      finalImagePublicId = null
    } else if (editingProduct.value) {
      // Retain existing image if not changed
      finalImageUrl = editingProduct.value.imageUrl ?? null
    }

    if (editingProduct.value) {
      // Update
      const req: UpdateProductRequest = {
        name: formName.value.trim(),
        description: formDescription.value.trim(),
        price: Number(formPrice.value),
        isActive: formIsActive.value,
        categoryId: formCategoryId.value,
        imageUrl: finalImageUrl,
        imagePublicId: finalImagePublicId ?? undefined,
      }
      await updateProduct(editingProduct.value.id, req)
      successMessage.value = `Product "${formName.value}" updated successfully.`
    } else {
      // Create
      const req: CreateProductRequest = {
        name: formName.value.trim(),
        description: formDescription.value.trim(),
        price: Number(formPrice.value),
        categoryId: formCategoryId.value,
        imageUrl: finalImageUrl,
        imagePublicId: finalImagePublicId,
      }
      await createProduct(req)
      successMessage.value = `New harvest product "${formName.value}" created successfully.`
    }

    isFormModalOpen.value = false
    await loadData()
  } catch (error: any) {
    console.error('Save product failed:', error)
    errorMessage.value = error.response?.data?.message || 'Failed to save product in backend database.'
  } finally {
    isSubmitting.value = false
  }
}

function openDeleteModal(prod: Product) {
  deletingProduct.value = prod
  isDeleteModalOpen.value = true
}

async function confirmDeleteProduct() {
  if (!deletingProduct.value) return

  isDeleting.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    await deleteProduct(deletingProduct.value.id)
    successMessage.value = `Product "${deletingProduct.value.name}" deleted from catalogue.`
    isDeleteModalOpen.value = false
    deletingProduct.value = null
    await loadData()
  } catch (error: any) {
    console.error('Delete failed:', error)
    errorMessage.value =
      error.response?.data?.message ||
      'Unable to delete product. It may be referenced by existing customer orders.'
  } finally {
    isDeleting.value = false
  }
}

onMounted(loadData)
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <div>
        <h2 class="font-serif text-2xl font-bold text-stone-900 sm:text-3xl">
          Product Catalogue Management
        </h2>
        <p class="mt-1 text-xs text-stone-500">
          Create, edit pricing, upload Cloudinary photography, or manage harvest items.
        </p>
      </div>

      <div class="flex items-center gap-3">
        <button
          type="button"
          class="inline-flex items-center gap-2 rounded-xl border border-stone-200 bg-white px-3.5 py-2 text-xs font-semibold text-stone-700 shadow-2xs hover:bg-stone-50 transition"
          :disabled="isLoading"
          @click="loadData"
        >
          <RefreshCw class="h-3.5 w-3.5" :class="{ 'animate-spin': isLoading }" />
          <span>Refresh</span>
        </button>

        <button
          type="button"
          class="inline-flex items-center gap-2 rounded-xl bg-amber-500 px-4 py-2 text-xs font-bold text-stone-950 shadow-md shadow-amber-900/10 hover:bg-amber-400 active:scale-95 transition"
          @click="openCreateModal"
        >
          <Plus class="h-4 w-4" />
          <span>Add Harvest Product</span>
        </button>
      </div>
    </div>

    <!-- Notifications -->
    <div
      v-if="errorMessage"
      class="flex items-center gap-2 rounded-2xl border border-rose-200 bg-rose-50 p-4 text-xs font-bold text-rose-800"
    >
      <AlertCircle class="h-4 w-4 shrink-0" />
      <span>{{ errorMessage }}</span>
    </div>

    <div
      v-if="successMessage"
      class="flex items-center gap-2 rounded-2xl border border-emerald-200 bg-emerald-50 p-4 text-xs font-bold text-emerald-800"
    >
      <CheckCircle2 class="h-4 w-4 shrink-0" />
      <span>{{ successMessage }}</span>
    </div>

    <!-- Search Toolbar -->
    <div class="rounded-3xl border border-stone-200 bg-white p-4 shadow-2xs">
      <div class="relative max-w-sm">
        <Search class="absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-stone-400" />
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Filter products by name or category..."
          class="w-full rounded-2xl border border-stone-200 bg-stone-50/50 pl-10 pr-4 py-2 text-xs text-stone-900 placeholder-stone-400 focus:border-amber-700 focus:bg-white focus:outline-none"
        />
      </div>
    </div>

    <!-- Products Table -->
    <div class="overflow-hidden rounded-3xl border border-stone-200 bg-white shadow-2xs">
      <div v-if="isLoading" class="p-12 text-center text-stone-500">
        <span class="h-6 w-6 inline-block animate-spin rounded-full border-2 border-stone-600 border-t-transparent" />
        <p class="mt-3 text-sm font-medium">Loading products catalogue...</p>
      </div>

      <div v-else-if="filteredProducts.length === 0" class="p-12 text-center text-stone-500">
        <Package class="mx-auto h-10 w-10 text-stone-300" />
        <p class="mt-3 text-sm font-medium">No products found matching query.</p>
      </div>

      <div v-else class="overflow-x-auto">
        <table class="w-full text-left border-collapse text-xs">
          <thead>
            <tr class="border-b border-stone-200 bg-stone-50/80 font-bold uppercase tracking-wider text-stone-500">
              <th class="px-6 py-4">Product</th>
              <th class="px-6 py-4">Category</th>
              <th class="px-6 py-4">Price</th>
              <th class="px-6 py-4">Status</th>
              <th class="px-6 py-4 text-right">Actions</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-stone-100">
            <tr
              v-for="prod in filteredProducts"
              :key="prod.id"
              class="hover:bg-stone-50/50 transition"
            >
              <!-- Product with Cloudinary Image Avatar -->
              <td class="px-6 py-4">
                <div class="flex items-center gap-3">
                  <div class="h-11 w-11 shrink-0 overflow-hidden rounded-xl border border-stone-200 bg-stone-100">
                    <img
                      :src="getProductImageUrl(prod)"
                      :alt="prod.name"
                      class="h-full w-full object-cover"
                      loading="lazy"
                    />
                  </div>
                  <div>
                    <span class="font-bold text-stone-900 text-sm block">{{ prod.name }}</span>
                    <span class="text-stone-400 font-mono text-2xs truncate max-w-xs block">ID: {{ prod.id }}</span>
                  </div>
                </div>
              </td>

              <!-- Category -->
              <td class="px-6 py-4 text-stone-600 font-medium">
                <span class="inline-flex items-center rounded-full bg-stone-100 px-2.5 py-0.5 text-xs font-semibold text-stone-800">
                  {{ prod.categoryName || 'Unassigned' }}
                </span>
              </td>

              <!-- Price -->
              <td class="px-6 py-4 font-bold text-stone-900 text-sm">
                {{ formatPrice(prod.price) }}
              </td>

              <!-- Status -->
              <td class="px-6 py-4">
                <span
                  class="inline-block rounded-full px-2.5 py-0.5 text-2xs font-bold"
                  :class="prod.isActive !== false ? 'bg-emerald-50 text-emerald-800' : 'bg-stone-100 text-stone-500'"
                >
                  {{ prod.isActive !== false ? 'Active' : 'Archived' }}
                </span>
              </td>

              <!-- Actions -->
              <td class="px-6 py-4 text-right">
                <div class="inline-flex items-center gap-2">
                  <button
                    type="button"
                    class="rounded-lg p-1.5 text-stone-600 hover:bg-stone-100 hover:text-stone-900 transition"
                    title="Edit Product"
                    @click="openEditModal(prod)"
                  >
                    <Edit2 class="h-4 w-4" />
                  </button>
                  <button
                    type="button"
                    class="rounded-lg p-1.5 text-stone-400 hover:bg-rose-50 hover:text-rose-600 transition"
                    title="Delete Product"
                    @click="openDeleteModal(prod)"
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

    <!-- Create / Edit Product Modal -->
    <Modal
      :is-open="isFormModalOpen"
      :title="editingProduct ? 'Edit Harvest Product' : 'Add New Harvest Product'"
      @close="isFormModalOpen = false"
    >
      <form class="space-y-4" @submit.prevent="handleSaveProduct">
        <!-- Product Name -->
        <div>
          <label class="block text-xs font-bold uppercase tracking-wider text-stone-700">
            Product Name <span class="text-rose-600">*</span>
          </label>
          <input
            v-model="formName"
            type="text"
            required
            placeholder="e.g. Organic Skardu Halman Apricots"
            class="mt-1.5 w-full rounded-2xl border border-stone-200 bg-stone-50/50 px-4 py-2.5 text-sm text-stone-900 focus:border-amber-700 focus:bg-white focus:outline-none"
          />
        </div>

        <!-- Description -->
        <div>
          <label class="block text-xs font-bold uppercase tracking-wider text-stone-700">
            Description
          </label>
          <textarea
            v-model="formDescription"
            rows="3"
            placeholder="Brief origin, flavor profile, and harvest altitude..."
            class="mt-1.5 w-full rounded-2xl border border-stone-200 bg-stone-50/50 px-4 py-2.5 text-sm text-stone-900 focus:border-amber-700 focus:bg-white focus:outline-none"
          />
        </div>

        <!-- Price & Category Grid -->
        <div class="grid grid-cols-2 gap-3">
          <div>
            <label class="block text-xs font-bold uppercase tracking-wider text-stone-700">
              Price (PKR) <span class="text-rose-600">*</span>
            </label>
            <input
              v-model.number="formPrice"
              type="number"
              min="1"
              step="1"
              required
              placeholder="e.g. 1850"
              class="mt-1.5 w-full rounded-2xl border border-stone-200 bg-stone-50/50 px-4 py-2.5 text-sm text-stone-900 focus:border-amber-700 focus:bg-white focus:outline-none"
            />
          </div>

          <div>
            <label class="block text-xs font-bold uppercase tracking-wider text-stone-700">
              Category <span class="text-rose-600">*</span>
            </label>
            <select
              v-model="formCategoryId"
              required
              class="mt-1.5 w-full rounded-2xl border border-stone-200 bg-stone-50/50 px-4 py-2.5 text-xs text-stone-900 focus:border-amber-700 focus:bg-white focus:outline-none"
            >
              <option
                v-for="cat in availableCategories"
                :key="cat.id"
                :value="cat.id"
              >
                {{ cat.name }}
              </option>
              <option v-if="availableCategories.length === 0" :value="formCategoryId">
                Current Category ({{ formCategoryId.slice(0, 8) }}...)
              </option>
            </select>
          </div>
        </div>

        <!-- Cloudinary Product Image Upload / Assignment Component -->
        <div>
          <label class="block text-xs font-bold uppercase tracking-wider text-stone-700">
            Product Photography (Cloudinary)
          </label>

          <!-- Preview & Replace State -->
          <div
            v-if="previewImageUrl"
            class="mt-2 flex items-center gap-3 rounded-2xl border border-stone-200 bg-stone-50/75 p-3"
          >
            <div class="h-14 w-14 shrink-0 overflow-hidden rounded-xl border border-stone-200 bg-white">
              <img :src="previewImageUrl" alt="Product preview" class="h-full w-full object-cover" />
            </div>
            <div class="flex-1 min-w-0">
              <span class="text-xs font-bold text-stone-900 block truncate">
                {{ imageFileName || 'Product Image' }}
              </span>
              <span class="text-[10px] text-emerald-700 font-semibold flex items-center gap-1">
                <CheckCircle2 class="h-3 w-3" />
                {{ selectedFile ? 'Ready to upload to Cloudinary' : 'Stored in Cloudinary' }}
              </span>
            </div>
            <div class="flex items-center gap-2">
              <label class="cursor-pointer rounded-xl border border-stone-200 bg-white px-3 py-1.5 text-xs font-semibold text-stone-700 hover:bg-stone-100 transition shadow-2xs">
                Replace
                <input
                  type="file"
                  accept="image/jpeg,image/png,image/webp"
                  class="hidden"
                  @change="handleFileSelect"
                />
              </label>
              <button
                type="button"
                class="rounded-xl p-1.5 text-stone-400 hover:bg-rose-50 hover:text-rose-600 transition"
                title="Remove Image"
                @click="clearImage"
              >
                <X class="h-4 w-4" />
              </button>
            </div>
          </div>

          <!-- Drag and Drop Dropzone -->
          <div
            v-else
            class="mt-2 flex flex-col items-center justify-center rounded-2xl border-2 border-dashed border-stone-200 bg-stone-50/50 p-6 text-center hover:border-amber-600 hover:bg-amber-50/20 transition cursor-pointer"
            :class="{ 'border-amber-600 bg-amber-50/30': isDragging }"
            @dragover.prevent="isDragging = true"
            @dragleave.prevent="isDragging = false"
            @drop.prevent="handleFileDrop"
            @click="fileInputRef?.click()"
          >
            <UploadCloud class="h-7 w-7 text-stone-400" />
            <p class="mt-2 text-xs font-semibold text-stone-700">
              <span class="text-amber-700 font-bold">Upload image</span> or drag & drop
            </p>
            <p class="mt-1 text-[10px] text-stone-400">
              JPEG, PNG or WebP up to 5MB. Uploaded securely to Cloudinary CDN.
            </p>
            <input
              ref="fileInputRef"
              type="file"
              accept="image/jpeg,image/png,image/webp"
              class="hidden"
              @change="handleFileSelect"
            />
          </div>

          <p v-if="imageError" class="mt-1.5 text-[11px] font-semibold text-rose-600">
            {{ imageError }}
          </p>
        </div>

        <!-- Active Toggle -->
        <div v-if="editingProduct" class="pt-2">
          <label class="flex items-center gap-2 cursor-pointer">
            <input
              v-model="formIsActive"
              type="checkbox"
              class="h-4 w-4 rounded-md border-stone-300 text-emerald-800 focus:ring-emerald-700"
            />
            <span class="text-xs font-semibold text-stone-800">
              Product is active and visible in customer storefront
            </span>
          </label>
        </div>

        <!-- Footer Actions -->
        <div class="mt-6 flex justify-end gap-3 border-t border-stone-100 pt-4">
          <button
            type="button"
            class="rounded-xl border border-stone-200 px-4 py-2 text-xs font-semibold text-stone-700 hover:bg-stone-100"
            :disabled="isSubmitting"
            @click="isFormModalOpen = false"
          >
            Cancel
          </button>
          <button
            type="submit"
            :disabled="isSubmitting"
            class="inline-flex items-center gap-2 rounded-xl bg-amber-500 px-5 py-2 text-xs font-bold text-stone-950 shadow-md shadow-amber-900/10 hover:bg-amber-400 disabled:opacity-50"
          >
            <Loader2 v-if="isSubmitting" class="h-3.5 w-3.5 animate-spin" />
            <span>{{ isUploadingImage ? 'Uploading to Cloudinary...' : isSubmitting ? 'Saving...' : 'Save Product' }}</span>
          </button>
        </div>
      </form>
    </Modal>

    <!-- Delete Product Confirmation Modal -->
    <Modal
      :is-open="isDeleteModalOpen"
      title="Delete Product Confirmation"
      @close="isDeleteModalOpen = false"
    >
      <div v-if="deletingProduct" class="space-y-3">
        <p class="text-sm text-stone-600 leading-relaxed">
          Are you sure you want to permanently delete <strong>"{{ deletingProduct.name }}"</strong> from the database?
        </p>
        <p class="text-2xs text-rose-700 font-semibold">
          Warning: If this product is part of previous customer orders, the backend database will protect relational integrity and reject the deletion.
        </p>
      </div>

      <template #footer>
        <div class="flex justify-end gap-3">
          <button
            type="button"
            class="rounded-xl border border-stone-200 px-4 py-2 text-xs font-semibold text-stone-700 hover:bg-stone-100"
            @click="isDeleteModalOpen = false"
          >
            Cancel
          </button>
          <button
            type="button"
            :disabled="isDeleting"
            class="rounded-xl bg-rose-700 px-4 py-2 text-xs font-bold text-white hover:bg-rose-800 disabled:opacity-50"
            @click="confirmDeleteProduct"
          >
            {{ isDeleting ? 'Deleting...' : 'Delete Product' }}
          </button>
        </div>
      </template>
    </Modal>
  </div>
</template>
