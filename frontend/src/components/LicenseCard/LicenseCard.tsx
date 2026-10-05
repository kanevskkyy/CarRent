import i18n from '../../i18n';
import { useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { FiAlertTriangle, FiCheckCircle, FiUpload } from 'react-icons/fi';
import { uploadLicense } from '../../api/driverApi';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { setLicenseStatus } from '../../store/authSlice';
import { getErrorMessage } from '../../utils/errors';
import s from './LicenseCard.module.css';

export default function LicenseCard() {
    const user = useAppSelector((state) => state.auth.user)!;
    const dispatch = useAppDispatch();

    const [number, setNumber] = useState('');
    const [file, setFile] = useState<File | null>(null);
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const verified = user.licenseStatus === 'Verified';

    async function handleSubmit(e: FormEvent) {
        e.preventDefault();
        if (!file) {
            setError(i18n.t("Додайте фото посвідчення."));
            return;
        }

        setError(null);
        setBusy(true);
        try {
            const result = await uploadLicense(number, file);
            dispatch(setLicenseStatus(result.status));
            if (result.status !== 'Verified') setError(i18n.exists(result.message) ? i18n.t(result.message) : result.message);
        } catch (err) {
            setError(getErrorMessage(err));
        } finally {
            setBusy(false);
        }
    }

    return (
        <div className={`panel ${s.card}`}>
            <h2>{i18n.t("Посвідчення водія")}</h2>
            <p className={s.email}>{user.email}</p>

            {verified ? (
                <>
                    <div className={s.status}>
                        <FiCheckCircle className={s.ok} />
                        <div>
                            <strong>{i18n.t("Перевірено")}</strong>
                            <p>{i18n.t("Оренда доступна.")}</p>
                            {user.driverLicenseNumber && <p>{i18n.t('Номер: {{number}}', { number: user.driverLicenseNumber })}</p>}
                        </div>
                    </div>
                    <Link to="/catalog" className={s.primary}>{i18n.t("До каталогу")}</Link>
                </>
            ) : (
                <form className={s.form} onSubmit={handleSubmit}>
                    <div className={s.status}>
                        <FiAlertTriangle className={s.warn} />
                        <div>
                            <strong>{user.licenseStatus === 'Rejected' ? i18n.t("Відхилено") : i18n.t("Не підтверджено")}</strong>
                            <p>{i18n.t("Без перевіреного посвідчення оренда недоступна.")}</p>
                        </div>
                    </div>

                    <input placeholder={i18n.t("Номер (наприклад ABC123456)")} value={number} required
                           onChange={(e) => setNumber(e.target.value)} />

                    <label className={s.file}>
                        <FiUpload /> {file ? file.name : i18n.t("Обрати фото (JPG, PNG до 5 МБ)")}
                        <input type="file" accept=".jpg,.jpeg,.png" hidden
                               onChange={(e) => setFile(e.target.files?.[0] ?? null)} />
                    </label>

                    {error && <p className={s.error}>{error}</p>}

                    <button className={s.primary} type="submit" disabled={busy}>
                        {busy ? i18n.t("Перевіряємо...") : i18n.t("Завантажити посвідчення")}
                    </button>
                </form>
            )}
        </div>
    );
}