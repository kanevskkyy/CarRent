import i18n from '../../i18n';
import { useEffect, useState } from 'react';
import { cancelBooking, fetchMyBookings } from '../../api/bookingsApi';
import { useAppDispatch } from '../../store/hooks';
import { invalidatePopular } from '../../store/popularSlice';
import type { Booking, PagedList } from '../../types';
import { getErrorMessage } from '../../utils/errors';
import BookingItem from '../../components/BookingItem/BookingItem';
import ConfirmDialog from '../../components/ConfirmDialog/ConfirmDialog';
import LicenseCard from '../../components/LicenseCard/LicenseCard';
import Pagination from '../../components/Pagination/Pagination';
import s from './AccountPage.module.css';

export default function AccountPage() {
    const dispatch = useAppDispatch();
    const [page, setPage] = useState(1);
    const [reload, setReload] = useState(0);
    const [data, setData] = useState<PagedList<Booking> | null>(null);
    const [error, setError] = useState(false);

    const [target, setTarget] = useState<Booking | null>(null);
    const [busy, setBusy] = useState(false);
    const [cancelError, setCancelError] = useState<string | null>(null);
    const [notice, setNotice] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;
        async function load() {
            try {
                setError(false);
                const result = await fetchMyBookings(page);
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

    async function confirmCancel() {
        if (!target) return;
        setBusy(true);
        setCancelError(null);
        try {
            await cancelBooking(target.id);
            setNotice(target.status === 'Confirmed'
                ? i18n.t("Бронювання скасовано. Повернення коштів через PayPal розпочато.")
                : i18n.t("Бронювання скасовано."));
            setTarget(null);
            dispatch(invalidatePopular());
            setReload((n) => n + 1);
        } catch (e) {
            setCancelError(getErrorMessage(e));
        } finally {
            setBusy(false);
        }
    }

    const refund = target?.status === 'Confirmed';

    return (
        <main className={`container ${s.layout}`}>
            <LicenseCard />

            <section>
                <h2>{i18n.t("Мої бронювання")}</h2>

                {notice && <p className={s.notice}>{notice}</p>}
                {error && <p className={s.empty}>{i18n.t("Не вдалося завантажити бронювання.")}</p>}
                {data && data.items.length === 0 && <p className={s.empty}>{i18n.t("У вас ще немає бронювань.")}</p>}

                <div className={s.list}>
                    {data?.items.map((b) => (
                        <BookingItem key={b.id} booking={b} onCancel={(x) => { setCancelError(null); setTarget(x); }} />
                    ))}
                </div>

                {data && (
                    <div className={s.pager}>
                        <Pagination page={data.currentPage} totalPages={data.totalPages} onChange={setPage} />
                    </div>
                )}
            </section>

            {target && (
                <ConfirmDialog
                    title={i18n.t("Скасувати бронювання?")}
                    text={refund
                        ? i18n.t('A full refund of {{amount}} ₴ will be sent to your PayPal. It usually takes a few minutes to a few days.', { amount: target.totalPrice.toLocaleString(i18n.language === 'uk' ? 'uk-UA' : 'en-US') })
                        : i18n.t("Бронювання ще не оплачене, тому гроші не списувались.")}
                    confirmLabel={refund ? i18n.t("Скасувати і повернути гроші") : i18n.t("Скасувати")}
                    busy={busy}
                    error={cancelError}
                    onConfirm={confirmCancel}
                    onClose={() => setTarget(null)}
                />
            )}
        </main>
    );
}