import i18n from '../../i18n';
import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { FiCheckCircle, FiClock, FiXCircle } from 'react-icons/fi';
import { fetchPaymentStatus } from '../../api/paymentsApi';
import { useAppDispatch } from '../../store/hooks';
import { invalidatePopular } from '../../store/popularSlice';
import StatusCard from '../../components/StatusCard/StatusCard';

type State = 'checking' | 'confirmed' | 'cancelled' | 'timeout' | 'unknown';

const sleep = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

export default function PaymentSuccessPage() {
    const dispatch = useAppDispatch();
    const [state, setState] = useState<State>('checking');

    useEffect(() => {
        const bookingId = sessionStorage.getItem('pendingBookingId');
        if (!bookingId) {
            setState('unknown');
            return;
        }

        let cancelled = false;
        async function poll(id: string) {
            // the webhook usually arrives within a few seconds
            for (let attempt = 0; attempt < 20 && !cancelled; attempt++) {
                try {
                    const status = await fetchPaymentStatus(id);
                    if (status.bookingStatus === 'Confirmed' || status.bookingStatus === 'Completed') {
                        sessionStorage.removeItem('pendingBookingId');
                        dispatch(invalidatePopular());
                        if (!cancelled) setState('confirmed');
                        return;
                    }
                    if (status.bookingStatus === 'Cancelled') {
                        if (!cancelled) setState('cancelled');
                        return;
                    }
                } catch {
                    // a temporary error: try again
                }
                await sleep(2000);
            }
            if (!cancelled) setState('timeout');
        }

        poll(bookingId);
        return () => { cancelled = true; };
    }, [dispatch]);

    if (state === 'confirmed') {
        return (
            <StatusCard icon={<FiCheckCircle />} title={i18n.t("Оплату отримано")} text={i18n.t("Бронювання підтверджено. Ключ чекає на вас у терміналі.")}>
                <Link to="/catalog">{i18n.t("До каталогу")}</Link>
            </StatusCard>
        );
    }
    if (state === 'cancelled') {
        return (
            <StatusCard icon={<FiXCircle />} title={i18n.t("Бронювання скасовано")}
                        text={i18n.t("Час на оплату минув. Якщо гроші були списані, вони повернуться автоматично.")}>
                <Link to="/catalog">{i18n.t("До каталогу")}</Link>
            </StatusCard>
        );
    }
    if (state === 'timeout' || state === 'unknown') {
        return (
            <StatusCard icon={<FiClock />} title={i18n.t("Оплату ще обробляємо")}
                        text={i18n.t("Підтвердження може прийти за кілька хвилин. Статус бронювання можна перевірити пізніше.")}>
                <Link to="/">{i18n.t("На головну")}</Link>
            </StatusCard>
        );
    }
    return <StatusCard icon={<FiClock />} title={i18n.t("Перевіряємо оплату...")} text={i18n.t("Це займе кілька секунд.")} />;
}