import { Bar, BarChart, Cell, ResponsiveContainer, Tooltip, XAxis } from 'recharts';
import type { DailyBookingCount } from '../../types';
import { dayFull, dayShort } from '../../utils/labels';
import s from './StatsChart.module.css';

interface TooltipProps {
    active?: boolean;
    payload?: { payload: DailyBookingCount }[];
}

function ChartTooltip({ active, payload }: TooltipProps) {
    if (!active || !payload?.length) return null;
    const { day, count } = payload[0].payload;

    return (
        <div className={s.tooltip}>
            <strong>{dayFull(day)}</strong>
            <span>{count} бронювань</span>
        </div>
    );
}

export default function StatsChart({ data }: { data: DailyBookingCount[] }) {
    const max = Math.max(0, ...data.map((d) => d.count));

    return (
        <div className={s.chart}>
            <ResponsiveContainer width="100%" height="100%">
                <BarChart data={data} margin={{ top: 8, right: 0, left: 0, bottom: 0 }}>
                    <XAxis
                        dataKey="day"
                        tickLine={false}
                        axisLine={false}
                        tickFormatter={dayShort}
                        tick={{ fill: 'var(--muted-foreground)', fontSize: 13 }}
                    />
                    <Tooltip cursor={{ fill: 'transparent' }} content={<ChartTooltip />} />
                    <Bar dataKey="count" radius={[10, 10, 10, 10]} maxBarSize={56}>
                        {data.map((d) => (
                            <Cell
                                key={d.day}
                                fill={d.count === max ? 'var(--brand)' : 'var(--brand-glow)'}
                                fillOpacity={d.count === max ? 1 : 0.7}
                            />
                        ))}
                    </Bar>
                </BarChart>
            </ResponsiveContainer>
        </div>
    );
}