import axios, { type InternalAxiosRequestConfig } from 'axios';
import type { EnhancedStore } from '@reduxjs/toolkit';
import { clearSession, setSession, type AuthState } from '../store/authSlice';
import type { AuthResponse } from '../types';

const baseURL = import.meta.env.VITE_API_URL;

export const http = axios.create({ baseURL });

let refreshing: Promise<AuthResponse> | null = null;

export function setupHttp(store: EnhancedStore) {
    const auth = () => (store.getState() as { auth: AuthState }).auth;

    http.interceptors.request.use((config) => {
        const { accessToken } = auth();
        if (accessToken) config.headers.Authorization = `Bearer ${accessToken}`;
        return config;
    });

    http.interceptors.response.use(
        (response) => response,
        async (error) => {
            const original = error.config as (InternalAxiosRequestConfig & { _retry?: boolean }) | undefined;
            const { refreshToken } = auth();
            const isAuthCall = original?.url?.includes('/api/auth/');

            if (error.response?.status === 401 && original && !original._retry && !isAuthCall && refreshToken) {
                original._retry = true;
                try {
                    refreshing ??= axios
                        .post<AuthResponse>(`${baseURL}/api/auth/refresh`, { refreshToken })
                        .then((r) => r.data)
                        .finally(() => { refreshing = null; });

                    const data = await refreshing;
                    store.dispatch(setSession(data));
                    original.headers.Authorization = `Bearer ${data.accessToken}`;
                    return http(original);
                } catch {
                    store.dispatch(clearSession());
                }
            }
            return Promise.reject(error);
        },
    );
}