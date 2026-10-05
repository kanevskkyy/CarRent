import { Link } from 'react-router-dom';
import { FiXCircle } from 'react-icons/fi';
import StatusCard from '../../components/StatusCard/StatusCard';

export default function PaymentCancelPage() {
    return (
        <StatusCard icon={<FiXCircle />} title="Оплату скасовано"
                    text="Гроші не списано. Бронювання залишається активним ще кілька хвилин, потім воно скасується автоматично.">
            <Link to="/catalog">До каталогу</Link>
        </StatusCard>
    );
}