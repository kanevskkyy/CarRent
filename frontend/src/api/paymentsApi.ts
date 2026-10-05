import { http } from './http';
import type { CreatePaymentResponse, PaymentStatusResponse } from '../types';

export async function createPayment(bookingId: string): Promise<CreatePaymentResponse> {
    return (await http.post<CreatePaymentResponse>(`/api/payments/bookings/${bookingId}/pay`)).data;
}

export async function fetchPaymentStatus(bookingId: string): Promise<PaymentStatusResponse> {
    return (await http.get<PaymentStatusResponse>(`/api/payments/bookings/${bookingId}/status`)).data;
}