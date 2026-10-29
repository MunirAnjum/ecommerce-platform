export interface CartItem {
  id: string
  productId: string
  productName: string
  weightInGrams: number
  unitPrice: number
  quantity: number
  totalPrice: number
}

export interface Cart {
  id: string
  userId: string
  items: CartItem[]
  totalAmount: number
}

export interface AddToCartRequest {
  productId: string
  weightInGrams: number
  quantity: number
}

export interface UpdateCartItemRequest {
  weightInGrams: number
  quantity: number
}