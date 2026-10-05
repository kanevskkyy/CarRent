import i18n from '../../i18n';
import { FiChevronLeft, FiChevronRight } from 'react-icons/fi';
import s from './Pagination.module.css';

interface Props { page: number; totalPages: number; onChange: (page: number) => void }

export default function Pagination({ page, totalPages, onChange }: Props) {
    if (totalPages <= 1) return null;
    const pages = Array.from({ length: totalPages }, (_, i) => i + 1);

    return (
        <div className={s.wrap}>
            <button className={s.btn} disabled={page <= 1} onClick={() => onChange(page - 1)} aria-label={i18n.t("Назад")}>
                <FiChevronLeft />
            </button>
            {pages.map((p) => (
                <button key={p} className={`${s.btn} ${p === page ? s.active : ''}`} onClick={() => onChange(p)}>
                    {p}
                </button>
            ))}
            <button className={s.btn} disabled={page >= totalPages} onClick={() => onChange(page + 1)} aria-label={i18n.t("Далі")}>
                <FiChevronRight />
            </button>
        </div>
    );
}