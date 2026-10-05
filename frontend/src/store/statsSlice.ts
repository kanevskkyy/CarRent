import { createAsyncThunk, createSlice } from '@reduxjs/toolkit';
import { fetchDailyStats } from '../api/bookingsApi';
import type { DailyBookingCount } from '../types';
import { today } from '../utils/date';

interface StatsState {
    items: DailyBookingCount[];
    updatedOn: string | null;
}

const initialState: StatsState = { items: [], updatedOn: null };

export const loadStats = createAsyncThunk('stats/load', async () => {
    return await fetchDailyStats();
}, {
    condition: (_, { getState }) => {
        const { stats } = getState() as { stats: StatsState };
        return stats.updatedOn !== today() || stats.items.length === 0;
    },
});

const statsSlice = createSlice({
    name: 'stats',
    initialState,
    reducers: {},
    extraReducers: (builder) => {
        builder.addCase(loadStats.fulfilled, (state, action) => {
            state.items = action.payload;
            state.updatedOn = today();
        });
    },
});

export default statsSlice.reducer;