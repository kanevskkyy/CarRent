import { http } from './http';
import type {Booking, Car, DailyBookingCount, PagedList} from '../types';

export async function fetchDailyStats(): Promise<DailyBookingCount[]> {
  const response = await http.get<DailyBookingCount[]>('/api/bookings/stats/daily');
  return response.data;
}

export async function fetchPopularCars(count = 3): Promise<Car[]> {
  const response = await http.get<Car[]>('/api/cars/popular-cars', { params: { count } });
  return response.data;
}

export async function createBooking(carId: string, startDate: string, endDate: string): Promise<{ id: string }> {
  const response = await http.post<{ id: string }>('/api/bookings', { carId, startDate, endDate });
  return response.data;
}

export async function fetchMyBookings(pageNumber: number, pageSize = 5): Promise<PagedList<Booking>> {
  const response = await http.get<PagedList<Booking>>('/api/bookings', { params: { pageNumber, pageSize } });
  return response.data;
}

export async function cancelBooking(id: string): Promise<void> {
  await http.post(`/api/bookings/${id}/cancel`);
}