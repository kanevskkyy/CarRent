import { NavLink } from 'react-router-dom';
import s from './Navigation.module.css';

const ITEMS = [
    { to: '/', label: 'Головна', end: true },
    { to: '/catalog', label: 'Каталог', end: false },
    { to: '/favorites', label: 'Вподобані', end: false },
];

export default function Navigation() {
    return (
        <nav className={s.nav}>
            {ITEMS.map((item) => (
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