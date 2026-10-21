import api from './api'

export interface InventoryResponse {
  id?: string
  productId: string
  availableQuantity: number
  reservedQuantity?: number
  createdAt?: string
  updatedAt?: string | null
}

export interface CreateInventoryRequest {
  productId: string
  initialQuantity: number
}

export interface UpdateStockQuantity {
  quantity: number
}

export async function getInventoryByProductId(
  productId: string
): Promise<InventoryResponse> {
  const response = await api.get<InventoryResponse>(
    `/api/Inventory/${productId}`
  )
  return response.data
}

export async function getAllInventory(): Promise<InventoryResponse[]> {
  const response = await api.get<InventoryResponse[]>('/api/Inventory')
  return response.data
}

export async function addStock(
  productId: string,
  quantity: number
): Promise<InventoryResponse> {
  const response = await api.post<InventoryResponse>(
    `/api/Inventory/${productId}/stock`,
    { quantity }
  )
  return response.data
}

export async function createInventory(
  request: CreateInventoryRequest
): Promise<InventoryResponse> {
  const response = await api.post<InventoryResponse>(
    '/api/Inventory',
    request
  )
  return response.data
}