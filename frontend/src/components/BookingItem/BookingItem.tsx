import type { Booking } from '../../types';
import { formatMoment, parseUtc } from '../../utils/date';
import { bookingStatusLabel } from '../../utils/labels';
import s from './BookingItem.module.css';

interface Props { booking: Booking; onCancel: (booking: Booking) => void }

export default function BookingItem({ booking, onCancel }: Props) {
    const canCancel = booking.status === 'Pending' || booking.status === 'Confirmed';

    return (
        <article className={`panel ${s.item}`}>
            <img src={booking.car.photoUrl} alt={booking.car.name} />
            <div className={s.info}>
                <h3>{booking.car.name}</h3>
                <p>{formatMoment(parseUtc(booking.startDate))} — {formatMoment(parseUtc(booking.endDate))}</p>
            </div>
            <div className={s.side}>
                <strong>{booking.totalPrice.toLocaleString('uk-UA')} ₴</strong>
                <span className={`${s.badge} ${s[booking.status]}`}>{bookingStatusLabel(booking.status)}</span>
                {canCancel && <button className={s.cancel} onClick={() => onCancel(booking)}>Скасувати</button>}
            </div>
        </article>
    );
}