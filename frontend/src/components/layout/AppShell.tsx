'use client';

import { useEffect, useState, type ReactNode } from 'react';
import Link from 'next/link';
import { usePathname, useRouter } from 'next/navigation';
import { useTheme } from 'next-themes';
import {
  LayoutDashboard,
  Users,
  Map,
  Building2,
  Code2,
  FileBarChart,
  LogOut,
  Search,
  Menu,
  X,
  Sun,
  Moon,
  ChevronRight,
} from 'lucide-react';
import { useAuth } from '@/hooks/useAuth';
import { cn } from '@/lib/utils';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { confirmAlert } from '@/lib/swal';

const nav = [
  { href: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { href: '/developers', label: 'Developers', icon: Users },
  { href: '/states', label: 'States', icon: Map },
  { href: '/cities', label: 'Cities', icon: Building2 },
  { href: '/languages', label: 'Languages', icon: Code2 },
  { href: '/reports', label: 'Reports', icon: FileBarChart },
] as const;

function Logo() {
  return (
    <div className="flex items-center gap-2">
      <div className="grid h-8 w-8 place-items-center rounded-lg bg-primary text-primary-foreground">
        <Code2 className="h-4 w-4" />
      </div>
      <div className="text-[15px] font-semibold tracking-tight">DevManager</div>
    </div>
  );
}

function SidebarBody({ onNav }: { onNav?: () => void }) {
  const pathname = usePathname();
  const { theme, setTheme } = useTheme();
  const { user, logout } = useAuth();
  const router = useRouter();

  const toggle = () => setTheme(theme === 'dark' ? 'light' : 'dark');

  return (
    <div className="flex h-full flex-col">
      <div className="flex h-14 items-center px-5">
        <Logo />
      </div>
      <nav className="flex-1 space-y-0.5 px-3 py-2">
        {nav.map((item) => {
          const active = pathname === item.href || pathname.startsWith(`${item.href}/`);
          return (
            <Link
              key={item.href}
              href={item.href}
              onClick={onNav}
              className={cn(
                'group flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors',
                active
                  ? 'bg-sidebar-accent text-sidebar-accent-foreground'
                  : 'text-muted-foreground hover:bg-sidebar-accent hover:text-sidebar-accent-foreground',
              )}
            >
              <item.icon className={cn('h-4 w-4', active ? 'text-primary' : '')} />
              <span>{item.label}</span>
            </Link>
          );
        })}
      </nav>
      <div className="border-t p-3">
        <div className="mb-2 flex items-center justify-between px-2">
          <div className="min-w-0">
            <div className="truncate text-xs font-medium">{user?.name ?? 'Usuário'}</div>
            <div className="truncate text-[11px] text-muted-foreground">{user?.email ?? ''}</div>
          </div>
        </div>
        <div className="flex gap-1">
          <Button variant="ghost" size="sm" className="flex-1 justify-start" onClick={toggle} aria-label="Alternar tema">
            {theme === 'dark' ? <Sun className="h-4 w-4" /> : <Moon className="h-4 w-4" />}
            <span className="ml-2 text-xs">{theme === 'dark' ? 'Light' : 'Dark'}</span>
          </Button>
          <Button
            variant="ghost"
            size="sm"
            className="flex-1 justify-start text-destructive hover:text-destructive"
            onClick={async () => {
              const result = await confirmAlert({
                title: 'Deseja mesmo sair?',
                text: 'Sua sessão atual será encerrada.',
                icon: 'question',
                confirmButtonText: 'Sim, sair',
                cancelButtonText: 'Cancelar',
                variant: 'destructive',
              });
              if (result.isConfirmed) {
                logout();
                router.push('/login');
              }
            }}
            aria-label="Sair"
          >
            <LogOut className="h-4 w-4" />
            <span className="ml-2 text-xs">Logout</span>
          </Button>
        </div>
      </div>
    </div>
  );
}

function Breadcrumbs() {
  const pathname = usePathname();
  const parts = pathname.split('/').filter(Boolean);

  return (
    <nav aria-label="Breadcrumb" className="flex items-center gap-1.5 text-sm text-muted-foreground">
      <Link href="/dashboard" className="hover:text-foreground">
        DevManager
      </Link>
      {parts.map((p, i) => (
        <span key={i} className="flex items-center gap-1.5">
          <ChevronRight className="h-3.5 w-3.5" />
          <span className={cn('capitalize', i === parts.length - 1 && 'font-medium text-foreground')}>{p}</span>
        </span>
      ))}
    </nav>
  );
}

function Header({ onMenu }: { onMenu: () => void }) {
  const { theme, setTheme } = useTheme();
  const { user } = useAuth();
  const toggle = () => setTheme(theme === 'dark' ? 'light' : 'dark');

  return (
    <header className="sticky top-0 z-30 flex h-14 items-center gap-3 border-b bg-background/80 px-4 backdrop-blur md:px-6">
      <Button variant="ghost" size="icon" className="md:hidden" onClick={onMenu} aria-label="Abrir menu">
        <Menu className="h-5 w-5" />
      </Button>
      <div className="hidden md:block">
        <Breadcrumbs />
      </div>
      <div className="ml-auto flex items-center gap-2">
        <div className="relative hidden sm:block">
          <Search className="pointer-events-none absolute left-2.5 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input type="search" placeholder="Buscar..." className="h-9 w-56 pl-8 lg:w-72" aria-label="Busca global" />
        </div>
        <Button variant="ghost" size="icon" onClick={toggle} aria-label="Alternar tema">
          {theme === 'dark' ? <Sun className="h-4 w-4" /> : <Moon className="h-4 w-4" />}
        </Button>
        <div
          className="grid h-8 w-8 place-items-center rounded-full bg-primary text-xs font-semibold text-primary-foreground"
          aria-label={user?.name}
        >
          {(user?.name ?? 'U').slice(0, 1).toUpperCase()}
        </div>
      </div>
    </header>
  );
}

export function AppShell({ children }: { children: ReactNode }) {
  const { user } = useAuth();
  const router = useRouter();
  const [mobileOpen, setMobileOpen] = useState(false);

  useEffect(() => {
    if (!user) router.replace('/login');
  }, [user, router]);

  if (!user) return null;

  return (
    <div className="flex min-h-screen w-full bg-background">
      <aside className="hidden w-60 shrink-0 border-r bg-sidebar md:block">
        <SidebarBody />
      </aside>

      {mobileOpen && (
        <div className="fixed inset-0 z-40 md:hidden">
          <div className="absolute inset-0 bg-black/40" onClick={() => setMobileOpen(false)} />
          <div className="absolute inset-y-0 left-0 w-64 border-r bg-sidebar shadow-xl">
            <div className="flex justify-end p-2">
              <Button variant="ghost" size="icon" onClick={() => setMobileOpen(false)} aria-label="Fechar menu">
                <X className="h-5 w-5" />
              </Button>
            </div>
            <SidebarBody onNav={() => setMobileOpen(false)} />
          </div>
        </div>
      )}

      <div className="flex min-w-0 flex-1 flex-col">
        <Header onMenu={() => setMobileOpen(true)} />
        <main className="flex-1 animate-in fade-in duration-300">{children}</main>
        <footer className="border-t px-6 py-3 text-xs text-muted-foreground">
          © {new Date().getFullYear()} DevManager — Built with care.
        </footer>
      </div>
    </div>
  );
}

export function PageContainer({ children }: { children: ReactNode }) {
  return <div className="mx-auto w-full max-w-7xl px-4 py-6 md:px-6 md:py-8">{children}</div>;
}

export function PageTitle({
  title,
  description,
  actions,
}: {
  title: string;
  description?: string;
  actions?: ReactNode;
}) {
  return (
    <div className="mb-6 flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
      <div className="min-w-0">
        <h1 className="text-2xl font-semibold tracking-tight">{title}</h1>
        {description && <p className="mt-1 text-sm text-muted-foreground">{description}</p>}
      </div>
      {actions && <div className="flex flex-wrap items-center gap-2">{actions}</div>}
    </div>
  );
}
