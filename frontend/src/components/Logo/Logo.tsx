import { Link } from 'react-router-dom';
import s from './Logo.module.css';

export default function Logo() {
    return (
        <Link to="/" className={s.logo}>
            <span className={s.mark}>C</span>
            <span>
        <span className={s.name}>CarRent</span>
        <span className={s.sub}>аеропорт · оренда авто</span>
      </span>
        </Link>
    );
}