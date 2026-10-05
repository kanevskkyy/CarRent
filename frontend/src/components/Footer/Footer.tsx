import { Link } from 'react-router-dom';
import Logo from '../Logo/Logo';
import s from './Footer.module.css';

export default function Footer() {
    return (
        <footer className={`container ${s.footer}`}>
            <div className={`panel ${s.inner}`}>
                <Logo />
                <p className={s.text}>Оренда авто в аеропорту Бориспіль та Києві.</p>
                <div className={s.links}>
                    <Link to="/catalog">Каталог</Link>
                    <Link to="/favorites">Вподобані</Link>
                    <Link to="/account">Акаунт</Link>
                </div>
                <p className={s.text}>© {new Date().getFullYear()} CarRent. Усі права захищено.</p>
            </div>
        </footer>
    );
}