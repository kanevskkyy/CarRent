import i18n from '../../i18n';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { setLanguage, type Language } from '../../store/settingsSlice';
import s from './LanguageSwitch.module.css';

const LANGS: { code: Language; label: string }[] = [
    { code: 'uk', label: 'UA' },
    { code: 'en', label: 'EN' },
];

export default function LanguageSwitch() {
    const dispatch = useAppDispatch();
    const current = useAppSelector((state) => state.settings.language);

    const change = (code: Language) => {
        dispatch(setLanguage(code));
        i18n.changeLanguage(code);
    };

    return (
        <div className={s.wrap}>
            {LANGS.map((lang) => (
                <button
                    key={lang.code}
                    onClick={() => change(lang.code)}
                    className={`${s.btn} ${current === lang.code ? s.active : ''}`}
                >
                    {lang.label}
                </button>
            ))}
        </div>
    );
}