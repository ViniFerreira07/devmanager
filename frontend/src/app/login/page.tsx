'use client';

import { zodResolver } from '@hookform/resolvers/zod';
import { useTheme } from 'next-themes';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { Code2, Moon, Sun } from 'lucide-react';
import { useAuth } from '@/hooks/useAuth';
import { loginSchema } from '@/schemas';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { toast } from 'sonner';

type LoginForm = z.infer<typeof loginSchema>;

export default function LoginPage() {
  const { login } = useAuth();
  const { theme, setTheme } = useTheme();
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<LoginForm>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data: LoginForm) => {
    try {
      await login(data.email, data.password);
    } catch (error: any) {
      const message = error?.response?.data?.message || 'Erro ao conectar ao servidor.';
      toast.error(message);
    }
  };

  return (
    <main className="flex min-h-screen items-center justify-center bg-background p-6">
      <Button
        variant="ghost"
        size="icon"
        className="absolute right-6 top-6"
        onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')}
        aria-label="Alternar tema"
      >
        {theme === 'dark' ? <Sun className="h-4 w-4" /> : <Moon className="h-4 w-4" />}
      </Button>
      <section className="w-full max-w-md rounded-xl border bg-card p-8 shadow-xl">
        <div className="mb-6 flex items-center gap-3">
          <div className="grid h-10 w-10 place-items-center rounded-lg bg-primary text-primary-foreground">
            <Code2 className="h-5 w-5" />
          </div>
          <div>
            <h1 className="text-2xl font-semibold tracking-tight">DevManager</h1>
            <p className="text-sm text-muted-foreground">Acesse sua conta para continuar.</p>
          </div>
        </div>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
          <div className="space-y-1.5">
            <Label htmlFor="email">Email</Label>
            <Input id="email" type="email" autoComplete="email" {...register('email')} />
            {errors.email && <p className="text-xs text-destructive">{errors.email.message}</p>}
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="password">Senha</Label>
            <Input id="password" type="password" autoComplete="current-password" {...register('password')} />
            {errors.password && <p className="text-xs text-destructive">{errors.password.message}</p>}
          </div>
          <Button type="submit" disabled={isSubmitting} className="w-full">
            {isSubmitting ? 'Entrando...' : 'Entrar'}
          </Button>
        </form>
      </section>
    </main>
  );
}
