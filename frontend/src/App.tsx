import { useTranslation } from 'react-i18next';
import { Route, Routes } from 'react-router-dom';
import Layout from './components/Layout/Layout';
import RequireAuth from './components/RequireAuth/RequireAuth';
import HomePage from './pages/HomePage/HomePage';
import CatalogPage from './pages/CatalogPage/CatalogPage';
import FavoritesPage from './pages/FavoritesPage/FavoritesPage';
import AccountPage from './pages/AccountPage/AccountPage';
import LoginPage from './pages/LoginPage/LoginPage';
import GitHubCallbackPage from './pages/GitHubCallbackPage/GitHubCallbackPage';
import PaymentSuccessPage from './pages/PaymentSuccessPage/PaymentSuccessPage';
import PaymentCancelPage from './pages/PaymentCancelPage/PaymentCancelPage';

export default function App() {
    useTranslation();
    return (
        <Routes>
            <Route path="/login" element={<LoginPage mode="login" />} />
            <Route path="/register" element={<LoginPage mode="register" />} />
            <Route path="/auth/github/callback" element={<GitHubCallbackPage />} />

            <Route element={<Layout />}>
                <Route path="/" element={<HomePage />} />
                <Route path="/catalog" element={<CatalogPage />} />
                <Route path="/favorites" element={<RequireAuth><FavoritesPage /></RequireAuth>} />
                <Route path="/account" element={<RequireAuth><AccountPage /></RequireAuth>} />
                <Route path="/payment/success" element={<PaymentSuccessPage />} />
                <Route path="/payment/cancel" element={<PaymentCancelPage />} />
            </Route>
        </Routes>
    );
}