import api from './api'
import type { CreateProductRequest, Product, UpdateProductRequest } from '../types/productType'

export async function getProducts(): Promise<Product[]> {
  const response = await api.get<Product[]>('/api/products')
  return response.data
}

export async function getProductById(id: string): Promise<Product> {
  const response = await api.get<Product>(`/api/products/${id}`)
  return response.data
}

export async function createProduct(request: CreateProductRequest): Promise<Product> {
  const response = await api.post<Product>('/api/products', request)
  return response.data
}

export async function updateProduct(id: string, request: UpdateProductRequest): Promise<Product> {
  const response = await api.put<Product>(`/api/products/${id}`, request)
  return response.data
}

export async function deleteProduct(id: string): Promise<void> {
  await api.delete(`/api/products/${id}`)
}