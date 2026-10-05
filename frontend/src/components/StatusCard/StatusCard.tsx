import type { ReactNode } from 'react';
import s from './StatusCard.module.css';

interface Props { icon: ReactNode; title: string; text?: string; children?: ReactNode }

export default function StatusCard({ icon, title, text, children }: Props) {
    return (
        <main className="container">
            <div className={`panel ${s.card}`}>
                <div className={s.icon}>{icon}</div>
                <h2>{title}</h2>
                {text && <p className={s.text}>{text}</p>}
                {children && <div className={s.actions}>{children}</div>}
            </div>
        </main>
    );
}