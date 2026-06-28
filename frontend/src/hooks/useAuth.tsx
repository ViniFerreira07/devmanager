'use client';

import { createContext, useContext, useEffect, useMemo, useState } from 'react';
import { useRouter } from 'next/navigation';
import api from '@/services/api';
import { UserSession } from '@/types';

type AuthUser = { name: string; email: string };

type AuthContextValue = {
  session: UserSession | null;
  user: AuthUser | null;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const [session, setSession] = useState<UserSession | null>(null);
  const [hydrated, setHydrated] = useState(false);

  useEffect(() => {
    const token = localStorage.getItem('devmanager-token');
    if (token) {
      const email = localStorage.getItem('devmanager-email') ?? '';
      const name = localStorage.getItem('devmanager-name') ?? '';
      setSession({ token, email, name });
    }
    setHydrated(true);
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      session,
      user: session ? { name: session.name, email: session.email } : null,
      async login(email, password) {
        const response = await api.post('/api/auth/login', { email, password });
        const nextSession = {
          token: response.data.token,
          email: response.data.email,
          name: response.data.name,
        };
        localStorage.setItem('devmanager-token', nextSession.token);
        localStorage.setItem('devmanager-email', nextSession.email);
        localStorage.setItem('devmanager-name', nextSession.name);
        setSession(nextSession);
        router.push('/dashboard');
      },
      logout() {
        localStorage.removeItem('devmanager-token');
        localStorage.removeItem('devmanager-email');
        localStorage.removeItem('devmanager-name');
        setSession(null);
        router.push('/login');
      },
    }),
    [router, session],
  );

  if (!hydrated) {
    return (
      <main className="flex min-h-screen items-center justify-center bg-background text-foreground">
        Carregando...
      </main>
    );
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used inside AuthProvider.');
  }
  return context;
}
