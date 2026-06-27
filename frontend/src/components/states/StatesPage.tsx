'use client';

import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Plus, Pencil, Trash2, Map as MapIcon } from 'lucide-react';
import { toast } from 'sonner';
import { PageContainer, PageTitle } from '@/components/layout/AppShell';
import { EmptyState, DeleteDialog, LoadingTable } from '@/components/common';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { devManagerApi } from '@/services/devManagerApi';
import { stateSchema } from '@/schemas';
import type { StateItem } from '@/types';

type FormData = z.infer<typeof stateSchema>;

export function StatesPage() {
  const qc = useQueryClient();
  const list = useQuery({ queryKey: ['states'], queryFn: devManagerApi.listStates });
  const [dialog, setDialog] = useState(false);
  const [editing, setEditing] = useState<StateItem | null>(null);
  const [toDelete, setToDelete] = useState<StateItem | null>(null);

  const { register, handleSubmit, reset, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(stateSchema),
    values: editing ? { name: editing.name, uf: editing.uf } : { name: '', uf: '' },
  });

  const create = useMutation({
    mutationFn: (data: FormData) => devManagerApi.createState(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['states'] });
      toast.success('Estado criado');
      setDialog(false);
      reset();
    },
    onError: () => toast.error('Não foi possível criar o estado'),
  });

  const update = useMutation({
    mutationFn: (data: FormData) => devManagerApi.updateState(editing!.id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['states'] });
      toast.success('Estado atualizado');
      setDialog(false);
    },
    onError: () => toast.error('Não foi possível atualizar o estado'),
  });

  const del = useMutation({
    mutationFn: (id: string) => devManagerApi.deleteState(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['states'] });
      qc.invalidateQueries({ queryKey: ['cities'] });
      toast.success('Estado removido');
      setToDelete(null);
    },
    onError: () => toast.error('Não foi possível excluir o estado'),
  });

  const onSubmit = (data: FormData) => (editing ? update.mutate(data) : create.mutate(data));

  return (
    <PageContainer>
      <PageTitle
        title="Estados"
        description="Cadastre os estados disponíveis."
        actions={
          <Button
            onClick={() => {
              setEditing(null);
              setDialog(true);
            }}
          >
            <Plus className="h-4 w-4" />
            Novo Estado
          </Button>
        }
      />

      <div className="rounded-xl border bg-card shadow-sm">
        {list.isLoading ? (
          <div className="p-4">
            <LoadingTable rows={5} cols={3} />
          </div>
        ) : (list.data?.length ?? 0) === 0 ? (
          <EmptyState
            icon={MapIcon}
            title="Nenhum estado cadastrado"
            action={
              <Button
                onClick={() => {
                  setEditing(null);
                  setDialog(true);
                }}
              >
                <Plus className="h-4 w-4" />
                Novo Estado
              </Button>
            }
          />
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Nome</TableHead>
                <TableHead className="w-24">UF</TableHead>
                <TableHead className="w-20 text-right">Ações</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {list.data?.map((s) => (
                <TableRow key={s.id}>
                  <TableCell className="font-medium">{s.name}</TableCell>
                  <TableCell>
                    <span className="rounded-md bg-muted px-2 py-0.5 text-xs font-medium">{s.uf}</span>
                  </TableCell>
                  <TableCell className="text-right">
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => {
                        setEditing(s);
                        setDialog(true);
                      }}
                      aria-label="Editar"
                    >
                      <Pencil className="h-4 w-4" />
                    </Button>
                    <Button variant="ghost" size="icon" onClick={() => setToDelete(s)} aria-label="Excluir">
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
            <DialogTitle>{editing ? 'Editar estado' : 'Novo estado'}</DialogTitle>
          </DialogHeader>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
            <div className="space-y-1.5">
              <Label htmlFor="name">Nome</Label>
              <Input id="name" {...register('name')} />
              {errors.name && <p className="text-xs text-destructive">{errors.name.message}</p>}
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="uf">UF</Label>
              <Input id="uf" maxLength={2} className="uppercase" {...register('uf')} />
              {errors.uf && <p className="text-xs text-destructive">{errors.uf.message}</p>}
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
        description="As cidades vinculadas também serão removidas."
        loading={del.isPending}
        onConfirm={() => {
          if (toDelete) del.mutate(toDelete.id);
        }}
      />
    </PageContainer>
  );
}
