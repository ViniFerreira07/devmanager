'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';

export default function HomePage() {
  const router = useRouter();

  useEffect(() => {
    const token = localStorage.getItem('devmanager-token');
    router.replace(token ? '/dashboard' : '/login');
  }, [router]);

  return (
    <main className="flex min-h-screen items-center justify-center bg-background text-foreground">
      Carregando...
    </main>
  );
}
