'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import api from '@/services/api';
import { stateSchema } from '@/schemas';
import { StateItem } from '@/types';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';

type StateFormValues = z.infer<typeof stateSchema>;

export default function StatesPage() {
  const [items, setItems] = useState<StateItem[]>([]);
  const [editingId, setEditingId] = useState<string | null>(null);

  const { register, handleSubmit, reset, watch, formState: { errors, isSubmitting } } = useForm<StateFormValues>({
    resolver: zodResolver(stateSchema),
    defaultValues: { name: '', uf: '' },
  });

  const form = watch();

  const load = async () => {
    const response = await api.get('/api/states');
    setItems(response.data);
  };

  useEffect(() => { load(); }, []);

  const onSubmit = async (data: StateFormValues) => {
    try {
      if (editingId) {
        await api.put(`/api/states/${editingId}`, { id: editingId, ...data });
      } else {
        await api.post('/api/states', data);
      }
      reset({ name: '', uf: '' });
      setEditingId(null);
      load();
    } catch {
      // Handle error
    }
  };

  const remove = async (id: string) => {
    if (!confirm('Deseja excluir este estado?')) return;
    await api.delete(`/api/states/${id}`);
    load();
  };

  const editItem = (item: StateItem) => {
    setEditingId(item.id);
    reset({ name: item.name, uf: item.uf });
  };

  return (
    <main className="min-h-screen bg-background p-8">
      <div className="mx-auto max-w-6xl">
        <div className="mb-6 flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-semibold">Estados</h1>
            <p className="text-sm text-slate-500">Gerencie estados e UFs.</p>
          </div>
          <Link href="/dashboard" className="text-sm text-cyan-600">Voltar</Link>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="mb-6 rounded-lg border border-border bg-card p-4">
          <div className="grid gap-4 md:grid-cols-3">
            <div>
              <Input {...register('name')} placeholder="Nome" />
              {errors.name && <p className="mt-1 text-sm text-red-500">{errors.name.message}</p>}
            </div>
            <div>
              <Input {...register('uf')} placeholder="UF" maxLength={2} />
              {errors.uf && <p className="mt-1 text-sm text-red-500">{errors.uf.message}</p>}
            </div>
            <Button type="submit" disabled={isSubmitting}>{editingId ? 'Salvar' : 'Criar'}</Button>
          </div>
        </form>

        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Nome</TableHead>
              <TableHead>UF</TableHead>
              <TableHead>Acoes</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {items.map((item) => (
              <TableRow key={item.id}>
                <TableCell>{item.name}</TableCell>
                <TableCell>{item.uf}</TableCell>
                <TableCell className="space-x-2">
                  <Button type="button" variant="link" size="sm" onClick={() => editItem(item)}>Editar</Button>
                  <Button type="button" variant="destructive" size="sm" onClick={() => remove(item.id)}>Excluir</Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    </main>
  );
}