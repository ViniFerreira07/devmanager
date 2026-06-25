'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import api from '@/services/api';
import { citySchema } from '@/schemas';
import { CityItem, StateItem } from '@/types';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';

type CityFormValues = z.infer<typeof citySchema>;

export default function CitiesPage() {
  const [items, setItems] = useState<CityItem[]>([]);
  const [states, setStates] = useState<StateItem[]>([]);
  const [editingId, setEditingId] = useState<string | null>(null);

  const { register, handleSubmit, reset, watch, formState: { errors, isSubmitting } } = useForm<CityFormValues>({
    resolver: zodResolver(citySchema),
    defaultValues: { name: '', stateId: '' },
  });

  const form = watch();

  const load = async () => {
    const [citiesRes, statesRes] = await Promise.all([api.get('/api/cities'), api.get('/api/states')]);
    setItems(citiesRes.data);
    setStates(statesRes.data);
  };

  useEffect(() => { load(); }, []);

  const onSubmit = async (data: CityFormValues) => {
    try {
      if (editingId) {
        await api.put(`/api/cities/${editingId}`, { id: editingId, ...data });
      } else {
        await api.post('/api/cities', data);
      }
      reset({ name: '', stateId: '' });
      setEditingId(null);
      load();
    } catch {
      // Handle error
    }
  };

  const remove = async (id: string) => {
    if (!confirm('Deseja excluir esta cidade?')) return;
    await api.delete(`/api/cities/${id}`);
    load();
  };

  const editItem = (item: CityItem) => {
    setEditingId(item.id);
    reset({ name: item.name, stateId: item.stateId });
  };

  return (
    <main className="min-h-screen bg-background p-8">
      <div className="mx-auto max-w-6xl">
        <div className="mb-6 flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-semibold">Cidades</h1>
            <p className="text-sm text-slate-500">Assocee cidades a estados.</p>
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
              <Select onValueChange={(value) => reset({ ...form, stateId: value })} value={form.stateId}>
                <SelectTrigger>
                  <SelectValue placeholder="Selecione um estado" />
                </SelectTrigger>
                <SelectContent>
                  {states.map((state) => <SelectItem key={state.id} value={state.id}>{state.name}</SelectItem>)}
                </SelectContent>
              </Select>
              {errors.stateId && <p className="mt-1 text-sm text-red-500">{errors.stateId.message}</p>}
            </div>
            <Button type="submit" disabled={isSubmitting}>{editingId ? 'Salvar' : 'Criar'}</Button>
          </div>
        </form>

        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Nome</TableHead>
              <TableHead>Estado</TableHead>
              <TableHead>Acoes</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {items.map((item) => (
              <TableRow key={item.id}>
                <TableCell>{item.name}</TableCell>
                <TableCell>{item.stateName}</TableCell>
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