import api from './api'

export interface InventoryResponse {
  productId: string
  availableQuantity: number
}

export async function getInventoryByProductId(
  productId: string
): Promise<InventoryResponse> {
  const response = await api.get<InventoryResponse>(
    `/api/Inventory/${productId}`
  )

  return response.data
}