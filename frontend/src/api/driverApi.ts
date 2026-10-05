import { http } from './http';
import type { LicenseUploadResponse } from '../types';

export async function uploadLicense(licenseNumber: string, file: File): Promise<LicenseUploadResponse> {
    const form = new FormData();
    form.append('licenseNumber', licenseNumber);
    form.append('file', file);
    const response = await http.post<LicenseUploadResponse>('/api/driver-verification/submit', form);
    return response.data;
}