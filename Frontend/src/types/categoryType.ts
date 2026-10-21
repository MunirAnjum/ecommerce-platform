export interface CategoryResponse {
  id: string
  name: string
  description: string
  isActive: boolean
  createAt: string
  updateAt?: string | null
}

export interface CreateCategoryRequest {
  name: string
  description: string
}

export interface UpdateCategoryRequest {
  name: string
  description: string
}
