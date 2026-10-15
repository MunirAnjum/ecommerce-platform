import api from './api'
import type {
  AddToCartRequest,
  Cart,
  UpdateCartItemRequest,
} from '../types/cartType'

export async function getCart(): Promise<Cart> {
  const response = await api.get<Cart>('/api/Cart')

  return response.data
}

export async function addToCart(
  request: AddToCartRequest
): Promise<Cart> {
  const response = await api.post<Cart>(
    '/api/Cart/items',
    request
  )

  return response.data
}

export async function updateCartItem(
  productId: string,
  request: UpdateCartItemRequest
): Promise<Cart> {
  const response = await api.put<Cart>(
    `/api/Cart/item/${productId}`,
    request
  )

  return response.data
}

export async function removeCartItem(
  productId: string
): Promise<void> {
  await api.delete(
    `/api/Cart/item/${productId}`
  )
}