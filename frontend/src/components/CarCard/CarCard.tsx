import i18n from '../../i18n';
import {useEffect, useState, type MouseEvent} from 'react';
import {useLocation, useNavigate} from 'react-router-dom';
import {FaHeart, FaRegHeart} from 'react-icons/fa';
import type {Car} from '../../types';
import {bodyLabel, engineLabel, transmissionLabel} from '../../utils/labels';
import {addFavorite, removeFavorite} from '../../api/favoritesApi';
import {useAppDispatch, useAppSelector} from '../../store/hooks';
import {invalidatePopular} from '../../store/popularSlice';
import CarModal from '../CarModal/CarModal';
import s from './CarCard.module.css';

interface Props {
    car: Car;
    onFavoriteChange?: (carId: string, isFavorite: boolean) => void;
}

export default function CarCard({car, onFavoriteChange}: Props) {
    const user = useAppSelector((state) => state.auth.user);
    const dispatch = useAppDispatch();
    const navigate = useNavigate();
    const location = useLocation();
    const [liked, setLiked] = useState(car.isFavorite);
    const [open, setOpen] = useState(false);

    useEffect(() => setLiked(car.isFavorite), [car.isFavorite]);

    async function toggleLike(e: MouseEvent) {
        e.stopPropagation();
        if (!user) {
            navigate('/login', {state: {from: location.pathname}});
            return;
        }

        const next = !liked;
        setLiked(next); // optimistic
        try {
            if (next) await addFavorite(car.id);
            else await removeFavorite(car.id);
            dispatch(invalidatePopular());
            onFavoriteChange?.(car.id, next);
        } catch {
            setLiked(!next);
        }
    }

    return (
        <>
            <article
                className={`panel ${s.card}`}
                onClick={() => setOpen(true)}
                onKeyDown={(e) => e.key === 'Enter' && setOpen(true)}
                role="button"
                tabIndex={0}
            >
                <div className={s.photo}>
                    <img src={car.photoUrl} alt={car.name} loading="lazy"/>
                    <button className={`${s.like} ${liked ? s.liked : ''}`} onClick={toggleLike} aria-label={i18n.t("Вподобати")}>
                        {liked ? <FaHeart/> : <FaRegHeart/>}
                    </button>
                </div>
                <div className={s.body}>
                    <div className={s.row}>
                        <h3>{car.name}</h3>
                        <div className={s.price}>
                            <strong>{car.dailyPrice.toLocaleString(i18n.language === 'uk' ? 'uk-UA' : 'en-US')} ₴</strong>
                            <span>{i18n.t("/ день")}</span>
                        </div>
                    </div>
                    <p className={s.meta}>{bodyLabel(car.bodyType)} · {transmissionLabel(car.transmissionType)}</p>
                    <div className={s.tags}>
                        <span className={s.green}>{engineLabel(car.engineType)}</span>
                        <span>{i18n.t('{{count}} місць', { count: car.seatsCount })}</span>
                        <span>{car.year}</span>
                    </div>
                </div>
            </article>

            {open && <CarModal carId={car.id} onClose={() => setOpen(false)}/>}
        </>
    );
}