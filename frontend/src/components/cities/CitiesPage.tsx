'use client';

import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Plus, Pencil, Trash2, Building2 } from 'lucide-react';
import { toast } from 'sonner';
import { PageContainer, PageTitle } from '@/components/layout/AppShell';
import { EmptyState, DeleteDialog, LoadingTable } from '@/components/common';
import { Combobox } from '@/components/common/selects';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { devManagerApi } from '@/services/devManagerApi';
import { citySchema } from '@/schemas';
import type { CityItem } from '@/types';

type FormData = z.infer<typeof citySchema>;

export function CitiesPage() {
  const qc = useQueryClient();
  const list = useQuery({ queryKey: ['cities'], queryFn: devManagerApi.listCities });
  const states = useQuery({ queryKey: ['states'], queryFn: devManagerApi.listStates });
  const [dialog, setDialog] = useState(false);
  const [editing, setEditing] = useState<CityItem | null>(null);
  const [toDelete, setToDelete] = useState<CityItem | null>(null);
  const [stateFilter, setStateFilter] = useState('all');

  const { register, handleSubmit, reset, watch, setValue, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(citySchema),
    values: editing ? { name: editing.name, stateId: editing.stateId } : { name: '', stateId: '' },
  });

  const create = useMutation({
    mutationFn: (data: FormData) => devManagerApi.createCity(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['cities'] });
      toast.success('Cidade criada');
      setDialog(false);
      reset();
    },
    onError: () => toast.error('Não foi possível criar a cidade'),
  });

  const update = useMutation({
    mutationFn: (data: FormData) => devManagerApi.updateCity(editing!.id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['cities'] });
      toast.success('Cidade atualizada');
      setDialog(false);
    },
    onError: () => toast.error('Não foi possível atualizar a cidade'),
  });

  const del = useMutation({
    mutationFn: (id: string) => devManagerApi.deleteCity(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['cities'] });
      toast.success('Cidade removida');
      setToDelete(null);
    },
    onError: () => toast.error('Não foi possível excluir a cidade'),
  });

  const stateName = (id: string) => states.data?.find((s) => s.id === id)?.name ?? '—';
  const stateOpts = (states.data ?? []).map((s) => ({ value: s.id, label: `${s.name} (${s.uf})` }));

  const filtered = useMemo(() => {
    let items = list.data ?? [];
    if (stateFilter !== 'all') items = items.filter((c) => c.stateId === stateFilter);
    return items;
  }, [list.data, stateFilter]);

  return (
    <PageContainer>
      <PageTitle
        title="Cidades"
        description="Cadastre as cidades vinculadas aos estados."
        actions={
          <Button
            onClick={() => {
              setEditing(null);
              setDialog(true);
            }}
          >
            <Plus className="h-4 w-4" />
            Nova Cidade
          </Button>
        }
      />

      <div className="rounded-xl border bg-card shadow-sm">
        <div className="flex items-center gap-2 border-b p-4">
          <Select value={stateFilter} onValueChange={setStateFilter}>
            <SelectTrigger className="h-9 w-56">
              <SelectValue placeholder="Filtrar por estado" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Todos os estados</SelectItem>
              {states.data?.map((s) => (
                <SelectItem key={s.id} value={s.id}>
                  {s.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
        {list.isLoading ? (
          <div className="p-4">
            <LoadingTable rows={5} cols={3} />
          </div>
        ) : filtered.length === 0 ? (
          <EmptyState
            icon={Building2}
            title="Nenhuma cidade encontrada"
            action={
              <Button
                onClick={() => {
                  setEditing(null);
                  setDialog(true);
                }}
              >
                <Plus className="h-4 w-4" />
                Nova Cidade
              </Button>
            }
          />
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Nome</TableHead>
                <TableHead>Estado</TableHead>
                <TableHead className="w-20 text-right">Ações</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filtered.map((c) => (
                <TableRow key={c.id}>
                  <TableCell className="font-medium">{c.name}</TableCell>
                  <TableCell>{c.stateName ?? stateName(c.stateId)}</TableCell>
                  <TableCell className="text-right">
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => {
                        setEditing(c);
                        setDialog(true);
                      }}
                      aria-label="Editar"
                    >
                      <Pencil className="h-4 w-4" />
                    </Button>
                    <Button variant="ghost" size="icon" onClick={() => setToDelete(c)} aria-label="Excluir">
                      <Trash2 className="h-4 w-4 text-destructive" />
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </div>

      <Dialog open={dialog} onOpenChange={setDialog}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>{editing ? 'Editar cidade' : 'Nova cidade'}</DialogTitle>
          </DialogHeader>
          <form
            onSubmit={handleSubmit((data) => (editing ? update.mutate(data) : create.mutate(data)))}
            className="space-y-4"
            noValidate
          >
            <div className="space-y-1.5">
              <Label htmlFor="name">Nome</Label>
              <Input id="name" {...register('name')} />
              {errors.name && <p className="text-xs text-destructive">{errors.name.message}</p>}
            </div>
            <div className="space-y-1.5">
              <Label>Estado</Label>
              <Combobox
                options={stateOpts}
                value={watch('stateId')}
                onChange={(v) => setValue('stateId', v, { shouldValidate: true })}
                placeholder="Selecione"
              />
              {errors.stateId && <p className="text-xs text-destructive">{errors.stateId.message}</p>}
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setDialog(false)}>
                Cancelar
              </Button>
              <Button type="submit" disabled={create.isPending || update.isPending}>
                {editing ? 'Salvar' : 'Criar'}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      <DeleteDialog
        open={!!toDelete}
        onOpenChange={(v) => !v && setToDelete(null)}
        title={`Excluir ${toDelete?.name}?`}
        loading={del.isPending}
        onConfirm={() => {
          if (toDelete) del.mutate(toDelete.id);
        }}
      />
    </PageContainer>
  );
}
