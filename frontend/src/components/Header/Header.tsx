import Logo from '../Logo/Logo';
import Navigation from '../Navigation/Navigation';
import LanguageSwitch from '../LanguageSwitch/LanguageSwitch';
import AuthButtons from '../AuthButtons/AuthButtons';
import s from './Header.module.css';

export default function Header() {
    return (
        <header className={`container ${s.header}`}>
            <Logo />
            <div className={s.nav}><Navigation /></div>
            <div className={s.right}>
                <LanguageSwitch />
                <AuthButtons />
            </div>
        </header>
    );
}