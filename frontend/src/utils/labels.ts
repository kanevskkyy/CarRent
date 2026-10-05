import i18n from '../i18n';
const DAYS: Record<string, string> = {
    Monday: 'Понеділок', Tuesday: 'Вівторок', Wednesday: 'Середа', Thursday: 'Четвер',
    Friday: "П'ятниця", Saturday: 'Субота', Sunday: 'Неділя',
};
const DAYS_SHORT: Record<string, string> = {
    Monday: 'Пн', Tuesday: 'Вт', Wednesday: 'Ср', Thursday: 'Чт', Friday: 'Пт', Saturday: 'Сб', Sunday: 'Нд',
};

const BODY: Record<string, string> = {
    Sedan: 'Седан', Crossover: 'Кросовер', SUV: 'Кросовер', Hatchback: 'Хетчбек',
    Coupe: 'Купе', Minivan: 'Мінівен', Wagon: 'Універсал',
};
const TRANSMISSION: Record<string, string> = { Automatic: 'Автомат', Manual: 'Механіка' };
const ENGINE: Record<string, string> = { Petrol: 'Бензин', Diesel: 'Дизель', Electric: 'Електро', Hybrid: 'Гібрид' };
const DRIVE: Record<string, string> = {
    Front: 'Передній', FWD: 'Передній', Rear: 'Задній', RWD: 'Задній', AWD: 'Повний', Full: 'Повний',
};
const LICENSE: Record<string, string> = {
    NotSubmitted: 'Не подано', Pending: 'На перевірці', Verified: 'Перевірено', Rejected: 'Відхилено',
};
const BOOKING_STATUS: Record<string, string> = {
    Pending: 'Очікує оплати', Confirmed: 'Оплачено', Completed: 'Завершено', Cancelled: 'Скасовано',
};

export const BODY_OPTIONS = ['Sedan', 'Crossover', 'Hatchback'];
export const TRANSMISSION_OPTIONS = ['Automatic', 'Manual'];

export const bookingStatusLabel = (v: string) => i18n.t(BOOKING_STATUS[v] ?? v);

export const dayFull = (d: string) => i18n.t(DAYS[d] ?? d);
export const dayShort = (d: string) => i18n.t(DAYS_SHORT[d] ?? d);
export const bodyLabel = (v: string) => i18n.t(BODY[v] ?? v);
export const transmissionLabel = (v: string) => i18n.t(TRANSMISSION[v] ?? v);
export const engineLabel = (v: string) => i18n.t(ENGINE[v] ?? v);
export const driveLabel = (v: string) => i18n.t(DRIVE[v] ?? v);
export const licenseLabel = (v: string) => i18n.t(LICENSE[v] ?? v);