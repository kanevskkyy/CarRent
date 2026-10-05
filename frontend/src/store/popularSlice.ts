import { createAsyncThunk, createSlice } from '@reduxjs/toolkit';
import { fetchPopularCars } from '../api/bookingsApi';
import type { Car } from '../types';

interface PopularState {
    items: Car[];
    stale: boolean;
}

const initialState: PopularState = { items: [], stale: true };

export const loadPopular = createAsyncThunk('popular/load', async () => {
    return await fetchPopularCars(3);
}, {
    condition: (_, { getState }) => {
        const { popular } = getState() as { popular: PopularState };
        return popular.stale || popular.items.length === 0;
    },
});

const popularSlice = createSlice({
    name: 'popular',
    initialState,
    reducers: {
        invalidatePopular: (state) => {
            state.stale = true;
        },
    },
    extraReducers: (builder) => {
        builder.addCase(loadPopular.fulfilled, (state, action) => {
            state.items = action.payload;
            state.stale = false;
        });
    },
});

export const { invalidatePopular } = popularSlice.actions;
export default popularSlice.reducer;