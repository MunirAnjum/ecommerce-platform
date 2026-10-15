
import api from './api'

export const PaymentMethod = {
  FakePayment: 1,
  CashOnDelivery: 2,
} as const

export type PaymentMethod =
  (typeof PaymentMethod)[keyof typeof PaymentMethod]

export const PaymentStatus = {
  Pending: 1,
  Processing: 2,
  Completed: 3,
  Failed: 4,
  Cancelled: 5,
} as const

export type PaymentStatus =
  (typeof PaymentStatus)[keyof typeof PaymentStatus]

export interface CreatePaymentRequest {
  orderId: string
  method: PaymentMethod
}

export interface ProcessPaymentRequest {
  shouldSucceed: boolean
}

export interface PaymentResponse {
  id: string
  orderId: string
  userId: string
  amount: number
  method: PaymentMethod
  status: PaymentStatus
  transactionId: string | null
  createdAt: string
  updatedAt: string | null
}

export async function createPayment(
  request: CreatePaymentRequest,
): Promise<PaymentResponse> {
  const response = await api.post<PaymentResponse>(
    '/api/Payment',
    request,
  )

  return response.data
}

export async function processPayment(
  paymentId: string,
  request: ProcessPaymentRequest,
): Promise<PaymentResponse> {
  const response = await api.post<PaymentResponse>(
    `/api/Payment/${paymentId}/process`,
    request,
  )

  return response.data
}

export async function getPaymentById(
  paymentId: string,
): Promise<PaymentResponse> {
  const response = await api.get<PaymentResponse>(
    `/api/Payment/${paymentId}`,
  )

  return response.data
}

export async function confirmCashOnDelivery(
  paymentId: string,
): Promise<PaymentResponse> {
  const response = await api.post<PaymentResponse>(
    `/api/Payment/${paymentId}/confirm-cod`,
  )

  return response.data
}

