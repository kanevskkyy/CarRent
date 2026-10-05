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
            setError('Додайте фото посвідчення.');
            return;
        }

        setError(null);
        setBusy(true);
        try {
            const result = await uploadLicense(number, file);
            dispatch(setLicenseStatus(result.status));
            if (result.status !== 'Verified') setError(result.message);
        } catch (err) {
            setError(getErrorMessage(err));
        } finally {
            setBusy(false);
        }
    }

    return (
        <div className={`panel ${s.card}`}>
            <h2>Посвідчення водія</h2>
            <p className={s.email}>{user.email}</p>

            {verified ? (
                <>
                    <div className={s.status}>
                        <FiCheckCircle className={s.ok} />
                        <div>
                            <strong>Перевірено</strong>
                            <p>Оренда доступна.</p>
                            {user.driverLicenseNumber && <p>Номер: {user.driverLicenseNumber}</p>}
                        </div>
                    </div>
                    <Link to="/catalog" className={s.primary}>До каталогу</Link>
                </>
            ) : (
                <form className={s.form} onSubmit={handleSubmit}>
                    <div className={s.status}>
                        <FiAlertTriangle className={s.warn} />
                        <div>
                            <strong>{user.licenseStatus === 'Rejected' ? 'Відхилено' : 'Не підтверджено'}</strong>
                            <p>Без перевіреного посвідчення оренда недоступна.</p>
                        </div>
                    </div>

                    <input placeholder="Номер (наприклад ABC123456)" value={number} required
                           onChange={(e) => setNumber(e.target.value)} />

                    <label className={s.file}>
                        <FiUpload /> {file ? file.name : 'Обрати фото (JPG, PNG до 5 МБ)'}
                        <input type="file" accept=".jpg,.jpeg,.png" hidden
                               onChange={(e) => setFile(e.target.files?.[0] ?? null)} />
                    </label>

                    {error && <p className={s.error}>{error}</p>}

                    <button className={s.primary} type="submit" disabled={busy}>
                        {busy ? 'Перевіряємо...' : 'Завантажити посвідчення'}
                    </button>
                </form>
            )}
        </div>
    );
}