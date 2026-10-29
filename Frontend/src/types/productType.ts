export interface Product {
  id: string
  name: string
  description: string
  price: number
  isActive: boolean
  categoryId: string
  categoryName: string
  imageUrl?: string | null
  createdAt: string
  updatedAt: string | null
}

export interface CreateProductRequest {
  name: string
  description: string
  price: number
  categoryId: string
  imageUrl?: string | null
  imagePublicId?: string | null
}

export interface UpdateProductRequest {
  name: string
  description: string
  price: number
  isActive?: boolean
  categoryId?: string
  imageUrl?: string | null
  imagePublicId?: string | null
}

export interface ImageUploadResponse {
  secureUrl: string
  publicId: string
}
