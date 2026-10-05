import type { ReactNode } from 'react';
import s from './CarGrid.module.css';

export default function CarGrid({ children }: { children: ReactNode }) {
    return <div className={s.grid}>{children}</div>;
}