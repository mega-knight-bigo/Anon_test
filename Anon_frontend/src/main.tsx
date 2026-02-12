import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { Auth0Provider } from '@auth0/auth0-react'
import { Auth0TokenSync } from '@/components/auth/Auth0TokenSync'
import './index.css'
import App from './App.tsx'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
      staleTime: 30_000,
    },
  },
})

// Auth0 configuration
const domain = import.meta.env.VITE_AUTH0_DOMAIN;
const clientId = import.meta.env.VITE_AUTH0_CLIENT_ID;
const audience = import.meta.env.VITE_AUTH0_AUDIENCE;
// const organization = import.meta.env.VITE_AUTH0_ORGANIZATION; // Temporarily disabled

if (!domain || !clientId) {
  throw new Error("Missing Auth0 configuration. Please check your .env file.");
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <Auth0Provider
      domain={domain}
      clientId={clientId}
      authorizationParams={{
        redirect_uri: window.location.origin,
        audience: audience,
        // organization: organization, // Temporarily disabled
      }}
      // SECURITY: Use memory storage instead of localStorage to prevent XSS token theft
      // Tokens are stored in memory and cleared on page refresh
      // Use refresh tokens (configured in Auth0 dashboard) for session persistence
      cacheLocation="memory"
      // Enable refresh token rotation for secure session management
      useRefreshTokens={true}
      useRefreshTokensFallback={false}
    >
      <QueryClientProvider client={queryClient}>
        <Auth0TokenSync />
        <App />
      </QueryClientProvider>
    </Auth0Provider>
  </StrictMode>,
)
