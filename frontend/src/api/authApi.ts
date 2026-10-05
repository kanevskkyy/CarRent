import { http } from './http';
import type { AuthResponse } from '../types';

export async function login(email: string, password: string): Promise<AuthResponse> {
    return (await http.post<AuthResponse>('/api/auth/login', { email, password })).data;
}

export async function register(fullName: string, email: string, password: string): Promise<AuthResponse> {
    return (await http.post<AuthResponse>('/api/auth/register', { fullName, email, password })).data;
}

export async function loginWithGoogle(idToken: string): Promise<AuthResponse> {
    return (await http.post<AuthResponse>('/api/auth/google', { idToken })).data;
}

export async function loginWithGitHub(code: string): Promise<AuthResponse> {
    return (await http.post<AuthResponse>('/api/auth/github', { idToken: code })).data;
}

export async function logout(refreshToken: string): Promise<void> {
    await http.post('/api/auth/logout', { refreshToken });
}