'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import api from '@/services/api';
import { languageSchema } from '@/schemas';
import { ProgrammingLanguageItem } from '@/types';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';

type LanguageFormValues = z.infer<typeof languageSchema>;

export default function LanguagesPage() {
  const [items, setItems] = useState<ProgrammingLanguageItem[]>([]);
  const [editingId, setEditingId] = useState<string | null>(null);

  const { register, handleSubmit, reset, watch, formState: { errors, isSubmitting } } = useForm<LanguageFormValues>({
    resolver: zodResolver(languageSchema),
    defaultValues: { name: '', type: '' },
  });

  const form = watch();

  const load = async () => {
    const response = await api.get('/api/languages');
    setItems(response.data);
  };

  useEffect(() => { load(); }, []);

  const onSubmit = async (data: LanguageFormValues) => {
    try {
      if (editingId) {
        await api.put(`/api/languages/${editingId}`, { id: editingId, ...data });
      } else {
        await api.post('/api/languages', data);
      }
      reset({ name: '', type: '' });
      setEditingId(null);
      load();
    } catch {
      // Handle error
    }
  };

  const remove = async (id: string) => {
    if (!confirm('Deseja excluir esta linguagem?')) return;
    await api.delete(`/api/languages/${id}`);
    load();
  };

  const editItem = (item: ProgrammingLanguageItem) => {
    setEditingId(item.id);
    reset({ name: item.name, type: item.type });
  };

  return (
    <main className="min-h-screen bg-background p-8">
      <div className="mx-auto max-w-6xl">
        <div className="mb-6 flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-semibold">Linguagens</h1>
            <p className="text-sm text-slate-500">Cadastre linguagens de programacao.</p>
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
              <Select onValueChange={(value) => form.type !== value && reset({ ...form, type: value })} value={form.type}>
                <SelectTrigger>
                  <SelectValue placeholder="Selecione o tipo" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Backend">Backend</SelectItem>
                  <SelectItem value="Frontend">Frontend</SelectItem>
                  <SelectItem value="FullStack">FullStack</SelectItem>
                  <SelectItem value="Mobile">Mobile</SelectItem>
                </SelectContent>
              </Select>
              {errors.type && <p className="mt-1 text-sm text-red-500">{errors.type.message}</p>}
            </div>
            <Button type="submit" disabled={isSubmitting}>{editingId ? 'Salvar' : 'Criar'}</Button>
          </div>
        </form>

        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Nome</TableHead>
              <TableHead>Tipo</TableHead>
              <TableHead>Acoes</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {items.map((item) => (
              <TableRow key={item.id}>
                <TableCell>{item.name}</TableCell>
                <TableCell>{item.type}</TableCell>
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