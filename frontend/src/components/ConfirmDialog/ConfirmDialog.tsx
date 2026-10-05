import i18n from '../../i18n';
import { createPortal } from 'react-dom';
import s from './ConfirmDialog.module.css';

interface Props {
    title: string;
    text: string;
    confirmLabel: string;
    busy?: boolean;
    error?: string | null;
    onConfirm: () => void;
    onClose: () => void;
}

export default function ConfirmDialog({ title, text, confirmLabel, busy, error, onConfirm, onClose }: Props) {
    return createPortal(
        <div className={s.overlay} onClick={onClose}>
            <div className={`panel-solid ${s.dialog}`} onClick={(e) => e.stopPropagation()}>
                <h3>{title}</h3>
                <p>{text}</p>
                {error && <p className={s.error}>{error}</p>}
                <div className={s.actions}>
                    <button className={s.secondary} onClick={onClose} disabled={busy}>{i18n.t("Назад")}</button>
                    <button className={s.danger} onClick={onConfirm} disabled={busy}>
                        {busy ? i18n.t("Зачекайте...") : confirmLabel}
                    </button>
                </div>
            </div>
        </div>,
        document.body,
    );
}