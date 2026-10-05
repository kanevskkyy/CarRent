import { useEffect, useMemo, useState } from 'react';
import { createPortal } from 'react-dom';
import { useLocation, useNavigate } from 'react-router-dom';
import { FiAlertTriangle, FiCheckCircle, FiX } from 'react-icons/fi';
import { fetchCar } from '../../api/carsApi';
import { createBooking } from '../../api/bookingsApi';
import { createPayment } from '../../api/paymentsApi';
import { useAppSelector } from '../../store/hooks';
import type { CarDetail } from '../../types';
import { formatMoment, parseUtc, toLocalInput } from '../../utils/date';
import { getErrorMessage } from '../../utils/errors';
import { driveLabel, engineLabel, transmissionLabel } from '../../utils/labels';
import s from './CarModal.module.css';

const DAY_MS = 24 * 60 * 60 * 1000;

function defaultRange() {
    const start = new Date();
    start.setDate(start.getDate() + 1);
    start.setHours(10, 0, 0, 0);
    const end = new Date(start.getTime() + 4 * DAY_MS);
    return { start: toLocalInput(start), end: toLocalInput(end) };
}

export default function CarModal({ carId, onClose }: { carId: string; onClose: () => void }) {
    const user = useAppSelector((state) => state.auth.user);
    const navigate = useNavigate();
    const location = useLocation();

    const [car, setCar] = useState<CarDetail | null>(null);
    const [loadFailed, setLoadFailed] = useState(false);
    const [range, setRange] = useState(defaultRange);
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;
        async function load() {
            try {
                const data = await fetchCar(carId);
                if (!cancelled) setCar(data);
            } catch {
                if (!cancelled) setLoadFailed(true);
            }
        }
        load();
        return () => { cancelled = true; };
    }, [carId]);

    useEffect(() => {
        const onKey = (e: KeyboardEvent) => e.key === 'Escape' && onClose();
        document.addEventListener('keydown', onKey);
        document.body.style.overflow = 'hidden';
        return () => {
            document.removeEventListener('keydown', onKey);
            document.body.style.overflow = '';
        };
    }, [onClose]);

    const startDate = new Date(range.start);
    const endDate = new Date(range.end);
    const days = Math.ceil((endDate.getTime() - startDate.getTime()) / DAY_MS);
    const datesValid = Number.isFinite(days) && days > 0 && startDate.getTime() > Date.now();

    const overlaps = useMemo(
        () => !!car?.bookings.some((b) => parseUtc(b.startDate) < endDate && parseUtc(b.endDate) > startDate),
        // eslint-disable-next-line react-hooks/exhaustive-deps
        [car, range],
    );

    const verified = user?.licenseStatus === 'Verified';
    const total = car && datesValid ? days * car.dailyPrice : 0;

    async function handleContinue() {
        if (!car) return;
        if (!user) {
            onClose();
            navigate('/login', { state: { from: location.pathname } });
            return;
        }
        if (!verified) {
            onClose();
            navigate('/account');
            return;
        }

        setError(null);
        setBusy(true);
        try {
            const booking = await createBooking(car.id, startDate.toISOString(), endDate.toISOString());
            sessionStorage.setItem('pendingBookingId', booking.id);
            const payment = await createPayment(booking.id);
            window.location.href = payment.approvalUrl;
        } catch (e) {
            setError(getErrorMessage(e));
            setBusy(false);
        }
    }

    const buttonLabel = !user ? 'Увійти' : !verified ? 'Завантажити посвідчення' : 'Продовжити до оплати';
    const disabled = busy || (verified && (!datesValid || overlaps));

    return createPortal(
        <div className={s.overlay} onClick={onClose}>
            <div className={s.modal} onClick={(e) => e.stopPropagation()}>
                <button className={s.close} onClick={onClose} aria-label="Закрити"><FiX /></button>

                {!car ? (
                    <p className={s.loading}>{loadFailed ? 'Не вдалося завантажити авто.' : 'Завантаження...'}</p>
                ) : (
                    <>
                        <div className={s.left}>
                            <span className={s.kicker}>Spec sheet</span>
                            <h2>{car.name}</h2>
                            <img className={s.photo} src={car.photoUrl} alt={car.name} />

                            <div className={s.specs}>
                                <Spec label="Двигун" value={`${engineLabel(car.engineType)} · ${car.horsePower} к.с.`} />
                                <Spec label="Привід" value={driveLabel(car.driveType)} />
                                <Spec label="Рік" value={String(car.year)} />
                                <Spec label="Коробка" value={transmissionLabel(car.transmissionType)} />
                                <Spec label="Місць" value={String(car.seatsCount)} />
                                <Spec label="Багажник" value={`${car.trunkVolumeLiters} л`} />
                            </div>

                            {car.bookings.length > 0 && (
                                <div className={s.busy}>
                                    <span className={s.kicker}>Зайняті періоди</span>
                                    {car.bookings.map((b) => (
                                        <div key={b.id} className={s.period}>
                                            {formatMoment(parseUtc(b.startDate))} — {formatMoment(parseUtc(b.endDate))}
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>

                        <div className={s.right}>
                            <div className={s.price}>
                                <strong>{car.dailyPrice.toLocaleString('uk-UA')} ₴</strong> <span>/ день</span>
                            </div>

                            <label className={s.field}>
                                <small>Отримати</small>
                                <input type="datetime-local" value={range.start}
                                       onChange={(e) => setRange({ ...range, start: e.target.value })} />
                            </label>
                            <label className={s.field}>
                                <small>Повернути</small>
                                <input type="datetime-local" value={range.end}
                                       onChange={(e) => setRange({ ...range, end: e.target.value })} />
                            </label>

                            <div className={s.note}>
                                {verified ? <FiCheckCircle className={s.ok} /> : <FiAlertTriangle className={s.warn} />}
                                <span>
                  {!user ? 'Увійдіть, щоб орендувати'
                      : verified ? 'Посвідчення перевірено'
                          : 'Завантажте посвідчення водія'}
                </span>
                            </div>

                            {verified && !datesValid && <p className={s.error}>Оберіть коректні дати в майбутньому.</p>}
                            {verified && datesValid && overlaps && <p className={s.error}>На ці дати авто вже зайняте.</p>}
                            {error && <p className={s.error}>{error}</p>}

                            <div className={s.bottom}>
                                <div className={s.total}>
                                    <span>Разом · {datesValid ? days : 0} дн.</span>
                                    <strong>{total.toLocaleString('uk-UA')} ₴</strong>
                                </div>
                                <button className={s.pay} onClick={handleContinue} disabled={disabled}>
                                    {busy ? 'Зачекайте...' : buttonLabel}
                                </button>
                                <small className={s.hint}>Оплата через PayPal</small>
                            </div>
                        </div>
                    </>
                )}
            </div>
        </div>,
        document.body,
    );
}

function Spec({ label, value }: { label: string; value: string }) {
    return (
        <div className={s.spec}>
            <span>{label}</span>
            <strong>{value}</strong>
        </div>
    );
}