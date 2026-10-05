import { useEffect, useRef, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { FaGithub } from 'react-icons/fa';
import { loginWithGitHub } from '../../api/authApi';
import { useCompleteLogin } from '../../hooks/useCompleteLogin';
import { getErrorMessage } from '../../utils/errors';
import StatusCard from '../../components/StatusCard/StatusCard';

export default function GitHubCallbackPage() {
    const [params] = useSearchParams();
    const completeLogin = useCompleteLogin();
    const [error, setError] = useState<string | null>(null);
    const started = useRef(false); // the OAuth code works only once (StrictMode runs effects twice)

    useEffect(() => {
        if (started.current) return;
        started.current = true;

        const code = params.get('code');
        if (!code) {
            setError('GitHub не повернув код авторизації.');
            return;
        }

        async function run(authCode: string) {
            try {
                completeLogin(await loginWithGitHub(authCode));
            } catch (e) {
                setError(getErrorMessage(e));
            }
        }
        run(code);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    return (
        <StatusCard icon={<FaGithub />} title={error ? 'Не вдалося увійти' : 'Входимо через GitHub...'} text={error ?? undefined}>
            {error && <Link to="/login">Спробувати ще раз</Link>}
        </StatusCard>
    );
}