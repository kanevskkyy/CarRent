import i18n from '../../i18n';
import { NavLink } from 'react-router-dom';
import s from './Navigation.module.css';

export default function Navigation() {
    const items = [
        { to: '/', label: i18n.t("Головна"), end: true },
        { to: '/catalog', label: i18n.t("Каталог"), end: false },
        { to: '/favorites', label: i18n.t("Вподобані"), end: false },
    ];

    return (
        <nav className={s.nav}>
            {items.map((item) => (
                <NavLink
                    key={item.to}
                    to={item.to}
                    end={item.end}
                    className={({ isActive }) => `${s.link} ${isActive ? s.active : ''}`}
                >
                    {item.label}
                </NavLink>
            ))}
        </nav>
    );
}