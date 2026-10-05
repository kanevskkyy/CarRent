import { useEffect, useState } from 'react';
import { fetchCars, type CarFilter } from '../../api/carsApi';
import type { Car, PagedList } from '../../types';
import { BODY_OPTIONS, TRANSMISSION_OPTIONS, bodyLabel, transmissionLabel } from '../../utils/labels';
import CarCard from '../../components/CarCard/CarCard';
import CarGrid from '../../components/CarGrid/CarGrid';
import Pagination from '../../components/Pagination/Pagination';
import s from './CatalogPage.module.css';

const PRICE_MAX = 6000;
const EMPTY: CarFilter = { bodyType: null, transmissionType: null, maxPrice: null };

export default function CatalogPage() {
    const [draft, setDraft] = useState<CarFilter>(EMPTY);
    const [applied, setApplied] = useState<CarFilter>(EMPTY);
    const [page, setPage] = useState(1);
    const [data, setData] = useState<PagedList<Car> | null>(null);
    const [error, setError] = useState(false);

    useEffect(() => {
        let cancelled = false;
        async function load() {
            try {
                setError(false);
                const result = await fetchCars(applied, page);
                if (!cancelled) setData(result);
            } catch {
                if (!cancelled) setError(true);
            }
        }
        load();
        return () => { cancelled = true; };
    }, [applied, page]);

    // the backend accepts one value, so a second click on the same option clears it
    const pick = (key: 'bodyType' | 'transmissionType', value: string) =>
        setDraft({ ...draft, [key]: draft[key] === value ? null : value });

    const apply = () => {
        setApplied(draft);
        setPage(1);
    };

    return (
        <main className={`container ${s.layout}`}>
            <aside className={`panel ${s.filters}`}>
                <h3>Фільтри</h3>

                <div className={s.group}>
                    <small>Тип кузова</small>
                    {BODY_OPTIONS.map((v) => (
                        <label key={v}>
                            <input type="checkbox" checked={draft.bodyType === v} onChange={() => pick('bodyType', v)} />
                            {bodyLabel(v)}
                        </label>
                    ))}
                </div>

                <div className={s.group}>
                    <small>Коробка</small>
                    {TRANSMISSION_OPTIONS.map((v) => (
                        <label key={v}>
                            <input type="checkbox" checked={draft.transmissionType === v}
                                   onChange={() => pick('transmissionType', v)} />
                            {transmissionLabel(v)}
                        </label>
                    ))}
                </div>

                <div className={s.group}>
                    <small>Ціна / день</small>
                    <strong>до {(draft.maxPrice ?? PRICE_MAX).toLocaleString('uk-UA')} ₴</strong>
                    <input
                        type="range" min={500} max={PRICE_MAX} step={100}
                        value={draft.maxPrice ?? PRICE_MAX}
                        onChange={(e) => {
                            const v = Number(e.target.value);
                            setDraft({ ...draft, maxPrice: v >= PRICE_MAX ? null : v });
                        }}
                    />
                </div>

                <button className={s.apply} onClick={apply}>Застосувати</button>
            </aside>

            <section>
                {data && (
                    <div className={s.top}>
                        <p>{data.totalCount} авто · сторінка <strong>{data.currentPage} з {Math.max(data.totalPages, 1)}</strong></p>
                        <Pagination page={data.currentPage} totalPages={data.totalPages} onChange={setPage} />
                    </div>
                )}

                {error && <p className={s.empty}>Не вдалося завантажити каталог.</p>}
                {data && data.items.length === 0 && <p className={s.empty}>Нічого не знайдено.</p>}

                {data && (
                    <CarGrid>
                        {data.items.map((car) => <CarCard key={car.id} car={car} />)}
                    </CarGrid>
                )}
            </section>
        </main>
    );
}