'use client';

import Link from 'next/link';
import { ThemeToggle } from '@/components/ThemeToggle';
import { useAuth } from '@/hooks/useAuth';

const cards = [
  { title: 'Estados', href: '/dashboard/states' },
  { title: 'Cidades', href: '/dashboard/cities' },
  { title: 'Linguagens', href: '/dashboard/languages' },
  { title: 'Desenvolvedores', href: '/dashboard/developers' },
];

export default function DashboardPage() {
  const { logout } = useAuth();

  return (
    <main className="min-h-screen bg-background p-8">
      <div className="mx-auto max-w-6xl">
        <div className="mb-8 flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-semibold">Dashboard</h1>
            <p className="text-sm text-slate-500">Gerencie estados, cidades, linguagens e desenvolvedores.</p>
          </div>
          <div className="flex items-center gap-2">
            <ThemeToggle />
            <button onClick={logout} className="rounded-md border border-border px-4 py-2 text-sm hover:bg-muted">Sair</button>
          </div>
        </div>
        <div className="grid gap-6 md:grid-cols-2 xl:grid-cols-4">
          {cards.map((card) => (
            <Link key={card.title} href={card.href} className="rounded-lg border border-border bg-card p-6 shadow-sm transition hover:border-cyan-500">
              <h2 className="text-xl font-semibold">{card.title}</h2>
              <p className="mt-2 text-sm text-slate-500">Acessar CRUD</p>
            </Link>
          ))}
        </div>
      </div>
    </main>
  );
}
