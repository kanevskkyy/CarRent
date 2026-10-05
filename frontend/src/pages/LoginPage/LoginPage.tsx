import i18n from '../../i18n';
import { useEffect, useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { GoogleLogin, type CredentialResponse } from '@react-oauth/google';
import { FaGithub } from 'react-icons/fa';
import { login, loginWithGoogle, register } from '../../api/authApi';
import { useCompleteLogin } from '../../hooks/useCompleteLogin';
import { getErrorMessage } from '../../utils/errors';
import Logo from '../../components/Logo/Logo';
import s from './LoginPage.module.css';

const GITHUB_URL =
    `https://github.com/login/oauth/authorize?client_id=${import.meta.env.VITE_GITHUB_CLIENT_ID}` +
    `&scope=${encodeURIComponent('read:user user:email')}`;

export default function LoginPage({ mode }: { mode: 'login' | 'register' }) {
    const completeLogin = useCompleteLogin();
    const isRegister = mode === 'register';

    const [fullName, setFullName] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState<string | null>(null);
    const [busy, setBusy] = useState(false);
    const [googleButtonWidth, setGoogleButtonWidth] = useState(() => Math.max(220, Math.min(360, window.innerWidth - 72)));

    useEffect(() => {
        const updateWidth = () => setGoogleButtonWidth(Math.max(220, Math.min(360, window.innerWidth - 72)));
        window.addEventListener('resize', updateWidth);
        return () => window.removeEventListener('resize', updateWidth);
    }, []);

    async function handleSubmit(e: FormEvent) {
        e.preventDefault();
        setError(null);

        if (isRegister && password.length < 8) {
            setError(i18n.t("Пароль має містити щонайменше 8 символів."));
            return;
        }

        setBusy(true);
        try {
            const data = isRegister ? await register(fullName, email, password) : await login(email, password);
            completeLogin(data);
        } catch (err) {
            setError(getErrorMessage(err));
        } finally {
            setBusy(false);
        }
    }

    async function handleGoogle(response: CredentialResponse) {
        if (!response.credential) return;
        setError(null);
        try {
            completeLogin(await loginWithGoogle(response.credential));
        } catch (err) {
            setError(getErrorMessage(err));
        }
    }

    return (
        <div className={s.page}>
            <Logo />

            <div className={`panel ${s.card}`}>
                <div className={s.tabs}>
                    <Link to="/login" replace className={!isRegister ? s.activeTab : ''}>{i18n.t("Вхід")}</Link>
                    <Link to="/register" replace className={isRegister ? s.activeTab : ''}>{i18n.t("Реєстрація")}</Link>
                </div>

                <form className={s.form} onSubmit={handleSubmit}>
                    {isRegister && (
                        <input placeholder={i18n.t("Ім'я та прізвище" )} value={fullName} required
                               onChange={(e) => setFullName(e.target.value)} />
                    )}
                    <input type="email" placeholder={i18n.t("Пошта")} value={email} required
                           onChange={(e) => setEmail(e.target.value)} />
                    <input type="password" placeholder={i18n.t("Пароль")} value={password} required
                           onChange={(e) => setPassword(e.target.value)} />

                    {error && <p className={s.error}>{error}</p>}

                    <button className={s.submit} type="submit" disabled={busy}>
                        {busy ? i18n.t("Зачекайте...") : isRegister ? i18n.t("Створити акаунт") : i18n.t("Увійти")}
                    </button>
                </form>

                <div className={s.divider}><span>{i18n.t("або")}</span></div>

                <div className={s.social}>
                    <GoogleLogin onSuccess={handleGoogle} onError={() => setError(i18n.t("Не вдалося увійти через Google."))}
                                 text="continue_with" shape="pill" width={String(googleButtonWidth)} />
                    <a className={s.github} href={GITHUB_URL}><FaGithub />{i18n.t("Продовжити з GitHub")}</a>
                </div>

                <p className={s.note}>
                    {i18n.t('Після входу потрібно завантажити посвідчення водія — без нього оренда недоступна.')}
                </p>
            </div>

            <Link to="/" className={s.back}>{i18n.t("← На головну")}</Link>
        </div>
    );
}