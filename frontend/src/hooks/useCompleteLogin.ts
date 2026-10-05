import { useLocation, useNavigate } from 'react-router-dom';
import { useAppDispatch } from '../store/hooks';
import { setSession } from '../store/authSlice';
import { invalidatePopular } from '../store/popularSlice';
import type { AuthResponse } from '../types';

export function useCompleteLogin() {
    const dispatch = useAppDispatch();
    const navigate = useNavigate();
    const location = useLocation();

    return (data: AuthResponse) => {
        dispatch(setSession(data));
        dispatch(invalidatePopular());
        const from = (location.state as { from?: string } | null)?.from ?? '/';
        navigate(data.user.licenseStatus === 'Verified' ? from : '/account', { replace: true });
    };
}