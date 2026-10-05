import { http } from './http';
import type { Car, CarDetail, PagedList } from '../types';

export interface CarFilter {
    bodyType: string | null;
    transmissionType: string | null;
    maxPrice: number | null;
}

export async function fetchCars(filter: CarFilter, pageNumber: number, pageSize = 9): Promise<PagedList<Car>> {
    const response = await http.get<PagedList<Car>>('/api/cars', {
        params: {
            pageNumber,
            pageSize,
            bodyType: filter.bodyType ?? undefined,
            transmissionType: filter.transmissionType ?? undefined,
            maxPrice: filter.maxPrice ?? undefined,
        },
    });
    return response.data;
}

export async function fetchCar(id: string): Promise<CarDetail> {
    return (await http.get<CarDetail>(`/api/cars/${id}`)).data;
}