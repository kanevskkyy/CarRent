import i18n from '../../i18n';
import { Link } from 'react-router-dom';
import { FiXCircle } from 'react-icons/fi';
import StatusCard from '../../components/StatusCard/StatusCard';

export default function PaymentCancelPage() {
    return (
        <StatusCard icon={<FiXCircle />} title={i18n.t("Оплату скасовано")}
                    text={i18n.t("Гроші не списано. Бронювання залишається активним ще кілька хвилин, потім воно скасується автоматично.")}>
            <Link to="/catalog">{i18n.t("До каталогу")}</Link>
        </StatusCard>
    );
}