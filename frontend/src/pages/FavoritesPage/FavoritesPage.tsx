import i18n from '../../i18n';
import { useEffect, useState } from 'react';
import { fetchFavorites } from '../../api/favoritesApi';
import type { Car, PagedList } from '../../types';
import CarCard from '../../components/CarCard/CarCard';
import CarGrid from '../../components/CarGrid/CarGrid';
import Pagination from '../../components/Pagination/Pagination';
import s from '../CatalogPage/CatalogPage.module.css';

export default function FavoritesPage() {
    const [page, setPage] = useState(1);
    const [reload, setReload] = useState(0);
    const [data, setData] = useState<PagedList<Car> | null>(null);
    const [error, setError] = useState(false);

    useEffect(() => {
        let cancelled = false;
        async function load() {
            try {
                setError(false);
                const result = await fetchFavorites(page);
                if (cancelled) return;
                if (result.items.length === 0 && page > 1) setPage(page - 1);
                else setData(result);
            } catch {
                if (!cancelled) setError(true);
            }
        }
        load();
        return () => { cancelled = true; };
    }, [page, reload]);

    return (
        <main className="container">
            <div className={s.top}>
                <h2>{i18n.t("Вподобані")}</h2>
                {data && <Pagination page={data.currentPage} totalPages={data.totalPages} onChange={setPage} />}
            </div>

            {error && <p className={s.empty}>{i18n.t("Не вдалося завантажити список.")}</p>}
            {data && data.items.length === 0 && <p className={s.empty}>{i18n.t("Ви ще нічого не вподобали.")}</p>}

            {data && (
                <CarGrid>
                    {data.items.map((car) => (
                        <CarCard
                            key={car.id}
                            car={{ ...car, isFavorite: true }}
                            onFavoriteChange={(_, isFavorite) => !isFavorite && setReload((n) => n + 1)}
                        />
                    ))}
                </CarGrid>
            )}
        </main>
    );
}