import axios from 'axios'
import api from './api'
import { useAuthStore } from '../stores/auth'
import type {
  CategoryResponse,
  CreateCategoryRequest,
  UpdateCategoryRequest,
} from '../types/categoryType'

// Direct or fallback client for ProductService categories (routes through Vite dev proxy or port 5008)
const productServiceClient = axios.create({
  baseURL: import.meta.env.VITE_PRODUCT_SERVICE_URL || '',
  headers: {
    'Content-Type': 'application/json',
  },
})

productServiceClient.interceptors.request.use((config) => {
  const authStore = useAuthStore()
  if (authStore.accessToken) {
    config.headers.Authorization = `Bearer ${authStore.accessToken}`
  }
  return config
})

export async function getCategories(): Promise<CategoryResponse[]> {
  try {
    const res = await api.get<CategoryResponse[]>('/api/Catogories')
    return res.data
  } catch (err: any) {
    if (err.response?.status === 404) {
      const res = await productServiceClient.get<CategoryResponse[]>('/api/Catogories')
      return res.data
    }
    throw err
  }
}

export async function getCategoryById(id: string): Promise<CategoryResponse> {
  try {
    const res = await api.get<CategoryResponse>(`/api/Catogories/${id}`)
    return res.data
  } catch (err: any) {
    if (err.response?.status === 404) {
      const res = await productServiceClient.get<CategoryResponse>(`/api/Catogories/${id}`)
      return res.data
    }
    throw err
  }
}

export async function createCategory(request: CreateCategoryRequest): Promise<CategoryResponse> {
  try {
    const res = await api.post<CategoryResponse>('/api/Catogories', request)
    return res.data
  } catch (err: any) {
    if (err.response?.status === 404) {
      const res = await productServiceClient.post<CategoryResponse>('/api/Catogories', request)
      return res.data
    }
    throw err
  }
}

export async function updateCategory(
  id: string,
  request: UpdateCategoryRequest
): Promise<CategoryResponse> {
  try {
    const res = await api.put<CategoryResponse>(`/api/Catogories/${id}`, request)
    return res.data
  } catch (err: any) {
    if (err.response?.status === 404) {
      const res = await productServiceClient.put<CategoryResponse>(`/api/Catogories/${id}`, request)
      return res.data
    }
    throw err
  }
}

export async function deleteCategory(id: string): Promise<void> {
  try {
    await api.delete(`/api/Catogories/${id}`)
  } catch (err: any) {
    if (err.response?.status === 404) {
      await productServiceClient.delete(`/api/Catogories/${id}`)
      return
    }
    throw err
  }
}
