import { isAxiosError } from 'axios';

export function getErrorMessage(e: unknown): string {
    if (isAxiosError(e)) {
        const data = e.response?.data as { error?: string; errors?: { message: string }[] } | undefined;
        if (data?.errors?.length) return data.errors.map((x) => x.message).join(' ');
        if (data?.error) return data.error;
        if (!e.response) return 'Сервер недоступний.';
    }
    return 'Щось пішло не так.';
}