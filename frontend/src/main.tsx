import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { Provider } from 'react-redux';
import { PersistGate } from 'redux-persist/integration/react';
import { BrowserRouter } from 'react-router-dom';
import { GoogleOAuthProvider } from '@react-oauth/google';
import { persistor, store } from './store';
import i18n from './i18n';
import './styles/global.css';
import App from './App';

createRoot(document.getElementById('root')!).render(
    <StrictMode>
        <GoogleOAuthProvider clientId={import.meta.env.VITE_GOOGLE_CLIENT_ID}>
            <Provider store={store}>
                <PersistGate
                    loading={null}
                    persistor={persistor}
                    onBeforeLift={async () => {
                        await i18n.changeLanguage(store.getState().settings.language);
                    }}
                >
                    <BrowserRouter>
                        <App />
                    </BrowserRouter>
                </PersistGate>
            </Provider>
        </GoogleOAuthProvider>
    </StrictMode>,
);