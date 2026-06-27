'use client';

import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Plus, Pencil, Trash2, Code2 } from 'lucide-react';
import { toast } from 'sonner';
import { PageContainer, PageTitle } from '@/components/layout/AppShell';
import { EmptyState, DeleteDialog, LoadingTable } from '@/components/common';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { cn } from '@/lib/utils';
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { devManagerApi } from '@/services/devManagerApi';
import { languageSchema } from '@/schemas';
import type { LanguageType, ProgrammingLanguageItem } from '@/types';

const types: LanguageType[] = ['Backend', 'Frontend', 'Mobile', 'Database', 'Cloud', 'DevOps', 'Game'];
const typeStyles: Record<LanguageType, string> = {
  Backend: 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200',
  Frontend: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200',
  Mobile: 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200',
  Database: 'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200',
  Cloud: 'bg-cyan-100 text-cyan-800 dark:bg-cyan-900 dark:text-cyan-200',
  DevOps: 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-200',
  Game: 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200',
};

type FormData = z.infer<typeof languageSchema>;

export function LanguagesPage() {
  const qc = useQueryClient();
  const list = useQuery({ queryKey: ['languages'], queryFn: devManagerApi.listLanguages });
  const [dialog, setDialog] = useState(false);
  const [editing, setEditing] = useState<ProgrammingLanguageItem | null>(null);
  const [toDelete, setToDelete] = useState<ProgrammingLanguageItem | null>(null);

  const { register, handleSubmit, reset, watch, setValue, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(languageSchema),
    values: editing ? { name: editing.name, type: editing.type } : { name: '', type: 'Backend' },
  });

  const create = useMutation({
    mutationFn: (data: FormData) => devManagerApi.createLanguage(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['languages'] });
      toast.success('Linguagem criada');
      setDialog(false);
      reset();
    },
    onError: () => toast.error('Não foi possível criar a linguagem'),
  });

  const update = useMutation({
    mutationFn: (data: FormData) => devManagerApi.updateLanguage(editing!.id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['languages'] });
      toast.success('Linguagem atualizada');
      setDialog(false);
    },
    onError: () => toast.error('Não foi possível atualizar a linguagem'),
  });

  const del = useMutation({
    mutationFn: (id: string) => devManagerApi.deleteLanguage(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['languages'] });
      toast.success('Linguagem removida');
      setToDelete(null);
    },
    onError: () => toast.error('Não foi possível excluir a linguagem'),
  });

  return (
    <PageContainer>
      <PageTitle
        title="Linguagens"
        description="Gerencie as tecnologias usadas pela equipe."
        actions={
          <Button
            onClick={() => {
              setEditing(null);
              setDialog(true);
            }}
          >
            <Plus className="h-4 w-4" />
            Nova Linguagem
          </Button>
        }
      />

      <div className="rounded-xl border bg-card shadow-sm">
        {list.isLoading ? (
          <div className="p-4">
            <LoadingTable rows={6} cols={3} />
          </div>
        ) : (list.data?.length ?? 0) === 0 ? (
          <EmptyState
            icon={Code2}
            title="Nenhuma linguagem cadastrada"
            action={
              <Button
                onClick={() => {
                  setEditing(null);
                  setDialog(true);
                }}
              >
                <Plus className="h-4 w-4" />
                Nova Linguagem
              </Button>
            }
          />
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Nome</TableHead>
                <TableHead>Tipo</TableHead>
                <TableHead className="w-20 text-right">Ações</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {list.data?.map((l) => (
                <TableRow key={l.id}>
                  <TableCell className="font-medium">{l.name}</TableCell>
                  <TableCell>
                    <span className={cn('inline-flex items-center rounded-full border px-2 py-0.5 text-xs font-medium', typeStyles[l.type])}>
                      {l.type}
                    </span>
                  </TableCell>
                  <TableCell className="text-right">
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => {
                        setEditing(l);
                        setDialog(true);
                      }}
                      aria-label="Editar"
                    >
                      <Pencil className="h-4 w-4" />
                    </Button>
                    <Button variant="ghost" size="icon" onClick={() => setToDelete(l)} aria-label="Excluir">
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
            <DialogTitle>{editing ? 'Editar linguagem' : 'Nova linguagem'}</DialogTitle>
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
              <Label>Tipo</Label>
              <Select value={watch('type')} onValueChange={(v) => setValue('type', v as LanguageType, { shouldValidate: true })}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {types.map((t) => (
                    <SelectItem key={t} value={t}>
                      {t}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
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
