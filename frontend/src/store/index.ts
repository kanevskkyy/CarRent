import { combineReducers, configureStore } from '@reduxjs/toolkit';
import {
    persistReducer, persistStore,
    FLUSH, REHYDRATE, PAUSE, PERSIST, PURGE, REGISTER,
} from 'redux-persist';
import storage from './storage';
import settings from './settingsSlice';
import stats from './statsSlice';
import popular from './popularSlice';
import auth from './authSlice';
import { setupHttp } from '../api/http';

const rootReducer = combineReducers({ settings, stats, popular, auth });

const persistedReducer = persistReducer(
    { key: 'carrent', storage, whitelist: ['settings', 'stats', 'popular', 'auth'] },
    rootReducer,
);

export const store = configureStore({
    reducer: persistedReducer,
    middleware: (getDefault) =>
        getDefault({
            serializableCheck: {
                ignoredActions: [FLUSH, REHYDRATE, PAUSE, PERSIST, PURGE, REGISTER],
            },
        }),
});

setupHttp(store);

export const persistor = persistStore(store);

export type RootState = ReturnType<typeof rootReducer>;
export type AppDispatch = typeof store.dispatch;