import i18n from '../i18n';
import { isAxiosError } from 'axios';

export function getErrorMessage(e: unknown): string {
    if (isAxiosError(e)) {
        const data = e.response?.data as { error?: string; errors?: { message: string }[] } | undefined;
        if (data?.errors?.length) return data.errors.map((x) => i18n.exists(x.message) ? i18n.t(x.message) : x.message).join(' ');
        if (data?.error) return i18n.exists(data.error) ? i18n.t(data.error) : data.error;
        if (!e.response) return i18n.t('Сервер недоступний.');
    }
    return i18n.t('Щось пішло не так.');
}