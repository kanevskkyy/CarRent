import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { AuthResponse, LicenseStatus, User } from '../types';

export interface AuthState {
    user: User | null;
    accessToken: string | null;
    refreshToken: string | null;
}

const initialState: AuthState = { user: null, accessToken: null, refreshToken: null };

const authSlice = createSlice({
    name: 'auth',
    initialState,
    reducers: {
        setSession: (state, action: PayloadAction<AuthResponse>) => {
            state.user = action.payload.user;
            state.accessToken = action.payload.accessToken;
            state.refreshToken = action.payload.refreshToken;
        },
        setLicenseStatus: (state, action: PayloadAction<LicenseStatus>) => {
            if (state.user) state.user.licenseStatus = action.payload;
        },
        clearSession: () => initialState,
    },
});

export const { setSession, setLicenseStatus, clearSession } = authSlice.actions;
export default authSlice.reducer;