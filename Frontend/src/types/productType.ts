export interface Product {
  id: string
  name: string
  description: string
  price: number
  isActive: boolean
  categoryId: string
  categoryName: string
  createdAt: string
  updatedAt: string | null
}

export interface CreateProductRequest {
  name: string
  description: string
  price: number
  categoryId: string
}

export interface UpdateProductRequest {
  name: string
  description: string
  price: number
  isActive: boolean
  categoryId: string
}