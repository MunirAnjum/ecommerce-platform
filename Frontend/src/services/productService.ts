import api from './api'
import type { Product } from '../types/productType'

export async function getProducts(): Promise<Product[]> {
  const response = await api.get<Product[]>('/api/products')

  return response.data
}

export async function getProductById(
  id: string
): Promise<Product> {
  const response = await api.get<Product>(
    `/api/products/${id}`
  )

  return response.data
}