import { Link, useNavigate } from 'react-router-dom';
import { FiLogOut, FiUser } from 'react-icons/fi';
import { logout } from '../../api/authApi';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { clearSession } from '../../store/authSlice';
import { invalidatePopular } from '../../store/popularSlice';
import s from './AuthButtons.module.css';

export default function AuthButtons() {
    const user = useAppSelector((state) => state.auth.user);
    const refreshToken = useAppSelector((state) => state.auth.refreshToken);
    const dispatch = useAppDispatch();
    const navigate = useNavigate();

    async function handleLogout() {
        try {
            if (refreshToken) await logout(refreshToken);
        } catch {
            // the local session is cleared anyway
        } finally {
            dispatch(clearSession());
            dispatch(invalidatePopular());
            navigate('/');
        }
    }

    if (!user) {
        return (
            <div className={s.wrap}>
                <Link to="/login" className={s.login}>Вхід</Link>
                <Link to="/register" className={s.register}>Реєстрація</Link>
            </div>
        );
    }

    return (
        <div className={s.wrap}>
            <Link to="/account" className={s.account}><FiUser /> <span>Акаунт</span></Link>
            <button className={s.logout} onClick={handleLogout} aria-label="Вийти"><FiLogOut /></button>
        </div>
    );
}