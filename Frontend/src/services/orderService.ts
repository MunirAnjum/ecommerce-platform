import api from "./api";

export interface CreateOrderRequest{
    cartId: string
}

export async function CreateOrder(
    request:CreateOrderRequest,
):Promise<OrderResponse> {
    const response = await api.post<OrderResponse>('/api/order', request)
    return response.data
}

export interface OrderItemResponse {
  id: string
  productId: string
  productName: string
  unitPrice: number
  quantity: number
  totalPrice: number
}

export interface OrderResponse {
  id: string
  userId: string
  totalAmount: number
  status: string | number
  createdAt: string
  updatedAt: string
  items: OrderItemResponse[]
}

export async function getMyOrders(): Promise<OrderResponse[]> {
  const response = await api.get<OrderResponse[]>('/api/Order')
  return response.data
}

export async function getOrderById(
  orderId: string,
): Promise<OrderResponse> {
  const response = await api.get<OrderResponse>(
    `/api/Order/${orderId}`,
  )
  return response.data
}

export async function cancelOrder(
  orderId: string,
): Promise<OrderResponse> {
  const response = await api.put<OrderResponse>(
    `/api/Order/${orderId}/cancel`,
  )
  return response.data
}

export async function getAllOrders(): Promise<OrderResponse[]> {
  const response = await api.get<OrderResponse[]>('/api/Order/admin')
  return response.data
}

export async function updateOrderStatus(
  orderId: string,
  status: number,
): Promise<OrderResponse> {
  const response = await api.put<OrderResponse>(
    `/api/Order/${orderId}/status`,
    { status },
  )

  return response.data
}

