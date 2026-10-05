import i18n from '../i18n';
export const today = (): string => new Date().toISOString().slice(0, 10);

export function parseUtc(value: string): Date {
    return new Date(/Z|[+-]\d\d:?\d\d$/.test(value) ? value : `${value}Z`);
}

export function toLocalInput(date: Date): string {
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

export function formatMoment(date: Date): string {
    return date.toLocaleString(i18n.language === 'uk' ? 'uk-UA' : 'en-US', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' });
}