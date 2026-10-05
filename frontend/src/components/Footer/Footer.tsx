import i18n from '../../i18n';
import { Link } from 'react-router-dom';
import Logo from '../Logo/Logo';
import s from './Footer.module.css';

export default function Footer() {
    return (
        <footer className={`container ${s.footer}`}>
            <div className={`panel ${s.inner}`}>
                <Logo />
                <p className={s.text}>{i18n.t("Оренда авто в аеропорту Бориспіль та Києві.")}</p>
                <div className={s.links}>
                    <Link to="/catalog">{i18n.t("Каталог")}</Link>
                    <Link to="/favorites">{i18n.t("Вподобані")}</Link>
                    <Link to="/account">{i18n.t("Акаунт")}</Link>
                </div>
                <p className={s.text}>© {new Date().getFullYear()} CarRent. {i18n.t('Усі права захищено.')}</p>
            </div>
        </footer>
    );
}