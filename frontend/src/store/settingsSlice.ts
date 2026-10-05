import { createSlice, type PayloadAction } from '@reduxjs/toolkit';

export type Language = 'uk' | 'en';

interface SettingsState {
    language: Language;
}

const initialState: SettingsState = { language: 'uk' };

const settingsSlice = createSlice({
    name: 'settings',
    initialState,
    reducers: {
        setLanguage: (state, action: PayloadAction<Language>) => {
            state.language = action.payload;
        },
    },
});

export const { setLanguage } = settingsSlice.actions;
export default settingsSlice.reducer;