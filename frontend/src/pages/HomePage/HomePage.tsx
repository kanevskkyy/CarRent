import i18n from '../../i18n';
import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { loadStats } from '../../store/statsSlice';
import { loadPopular } from '../../store/popularSlice';
import StatsChart from '../../components/StatsChart/StatsChart';
import CarCard from '../../components/CarCard/CarCard';
import s from './HomePage.module.css';

export default function HomePage() {
    const dispatch = useAppDispatch();
    const navigate = useNavigate();
    const stats = useAppSelector((state) => state.stats.items);
    const cars = useAppSelector((state) => state.popular.items);
    const [error, setError] = useState(false);

    useEffect(() => {
        async function load() {
            try {
                await Promise.all([
                    dispatch(loadStats()).unwrap(),
                    dispatch(loadPopular()).unwrap(),
                ]);
            } catch {
                setError(true);
            }
        }

        load();
    }, [dispatch]);

    return (
        <main className="container">
            <section className={s.top}>
                <div className={`panel ${s.hero}`}>
                    <span className={s.badge}>{i18n.t("Доступно сьогодні")}</span>
                    <h1>{i18n.t("Візьми ключ.")}<br />{i18n.t("Лети далі.")}</h1>
                    <p className={s.lead}>
                        {i18n.t('Точні авто, чистий аеропортовий термінал, нічого зайвого. Обери, забронюй і отримай ключ за 4 хвилини.')}
                    </p>

                    <div className={s.search}>
                        <div className={s.field}><small>{i18n.t("Місто")}</small><span>{i18n.t("Бориспіль, Київ")}</span></div>
                        <div className={s.field}><small>{i18n.t("Період")}</small><span>{i18n.t("Обираєте при бронюванні")}</span></div>
                        <button className={s.find} onClick={() => navigate('/catalog')}>{i18n.t("Знайти")}</button>
                    </div>
                </div>

                <aside className={`panel ${s.stats}`}>
                    <div className={s.statsHead}>
                        <h3>{i18n.t("Останні 48 годин")}</h3>
                        <small>{i18n.t("оновлено щодня")}</small>
                    </div>
                    <StatsChart data={stats} />
                </aside>
            </section>

            <section className={s.popular}>
                <div className={s.popularHead}>
                    <h2>{i18n.t("Популярні авто")}</h2>
                    <Link to="/catalog" className={s.all}>{i18n.t("Весь каталог →")}</Link>
                </div>
                {error && cars.length === 0 && <p className={s.error}>{i18n.t("Не вдалося завантажити дані.")}</p>}
                <div className={s.grid}>
                    {cars.map((car) => <CarCard key={car.id} car={car} />)}
                </div>
            </section>
        </main>
    );
}