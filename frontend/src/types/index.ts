export interface DailyBookingCount {
    day: string;
    count: number
}

export interface Car {
    id: string;
    name: string;
    dailyPrice: number;
    bodyType: string;
    transmissionType: string;
    engineType: string;
    seatsCount: number;
    year: number;
    isFavorite: boolean;
    photoUrl: string;
}

export interface CarBooking {
    id: string;
    startDate: string;
    endDate: string
}

export interface CarDetail {
    id: string;
    name: string;
    year: number;
    dailyPrice: number;
    bodyType: string;
    engineType: string;
    driveType: string;
    transmissionType: string;
    horsePower: number;
    photoUrl: string;
    seatsCount: number;
    trunkVolumeLiters: number;
    bookings: CarBooking[];
    isDriverLicenseVerified: boolean;
}

export type LicenseStatus = 'NotSubmitted' | 'Pending' | 'Verified' | 'Rejected';

export interface User {
    id: string;
    email: string;
    fullName: string;
    driverLicenseNumber: string | null;
    licenseStatus: LicenseStatus;
    roles: string[];
}

export interface AuthResponse {
    accessToken: string;
    refreshToken: string;
    refreshTokenExpiresOn: string;
    user: User;
}

export interface PagedList<T> {
    currentPage: number;
    totalPages: number;
    pageSize: number;
    totalCount: number;
    hasPrevious: boolean;
    hasNext: boolean;
    items: T[];
}

export interface CreatePaymentResponse {
    bookingId: string;
    orderId: string;
    approvalUrl: string
}

export interface PaymentStatusResponse {
    bookingId: string;
    bookingStatus: 'Pending' | 'Confirmed' | 'Completed' | 'Cancelled';
    paymentStatus: string;
    amount: number;
    paidAt: string | null;
}

export type BookingStatus = 'Pending' | 'Confirmed' | 'Completed' | 'Cancelled';

export interface Booking {
    id: string;
    startDate: string;
    endDate: string;
    status: BookingStatus;
    totalPrice: number;
    car: { id: string; name: string; photoUrl: string };
}

export interface LicenseUploadResponse {
    message: string;
    status: LicenseStatus
}