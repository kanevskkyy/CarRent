import { http } from './http';
import type { Car, PagedList } from '../types';

export async function fetchFavorites(pageNumber: number, pageSize = 9): Promise<PagedList<Car>> {
    const response = await http.get<PagedList<Car>>('/api/favorites', { params: { pageNumber, pageSize } });
    return response.data;
}

export async function addFavorite(carId: string): Promise<void> {
    await http.post(`/api/favorites/${carId}`);
}

export async function removeFavorite(carId: string): Promise<void> {
    await http.delete(`/api/favorites/${carId}`);
}