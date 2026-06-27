'use client';

import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Plus, Pencil, Trash2, Users, ChevronLeft, ChevronRight } from 'lucide-react';
import { toast } from 'sonner';
import { PageContainer, PageTitle } from '@/components/layout/AppShell';
import { SearchInput, EmptyState, DeleteDialog, LoadingTable } from '@/components/common';
import { Combobox, MultiSelect } from '@/components/common/selects';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { devManagerApi, mapLanguageNamesToIds } from '@/services/devManagerApi';
import { developerSchema } from '@/schemas';
import type { DeveloperItem, Seniority } from '@/types';

const seniorityOptions: Seniority[] = ['Junior', 'Pleno', 'Senior', 'Lead'];
const PAGE_SIZE = 8;

type DevForm = z.infer<typeof developerSchema>;

function DeveloperDialog({
  open,
  onOpenChange,
  editing,
}: {
  open: boolean;
  onOpenChange: (v: boolean) => void;
  editing: DeveloperItem | null;
}) {
  const qc = useQueryClient();
  const cities = useQuery({ queryKey: ['cities'], queryFn: devManagerApi.listCities });
  const langs = useQuery({ queryKey: ['languages'], queryFn: devManagerApi.listLanguages });

  const { register, handleSubmit, reset, watch, setValue, formState: { errors, isSubmitting } } = useForm<DevForm>({
    resolver: zodResolver(developerSchema),
    values: editing
      ? {
          name: editing.name,
          email: editing.email,
          seniority: (editing.seniority as Seniority) || 'Pleno',
          cityId: editing.cityId,
          programmingLanguageIds: mapLanguageNamesToIds(editing.programmingLanguages, langs.data ?? []),
          observations: editing.observations ?? '',
        }
      : {
          name: '',
          email: '',
          seniority: 'Pleno',
          cityId: '',
          programmingLanguageIds: [],
          observations: '',
        },
  });

  const create = useMutation({
    mutationFn: (data: DevForm) => devManagerApi.createDeveloper(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['developers'] });
      toast.success('Desenvolvedor criado');
      onOpenChange(false);
      reset();
    },
    onError: () => toast.error('Não foi possível criar o desenvolvedor'),
  });

  const update = useMutation({
    mutationFn: (data: DevForm) => devManagerApi.updateDeveloper(editing!.id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['developers'] });
      toast.success('Desenvolvedor atualizado');
      onOpenChange(false);
    },
    onError: () => toast.error('Não foi possível atualizar o desenvolvedor'),
  });

  const onSubmit = (data: DevForm) => (editing ? update.mutate(data) : create.mutate(data));
  const cityOpts = (cities.data ?? []).map((c) => ({ value: c.id, label: c.name }));
  const langOpts = (langs.data ?? []).map((l) => ({ value: l.id, label: l.name }));

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>{editing ? 'Editar desenvolvedor' : 'Novo desenvolvedor'}</DialogTitle>
          <DialogDescription>Preencha as informações abaixo.</DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div className="space-y-1.5">
              <Label htmlFor="name">Nome</Label>
              <Input id="name" {...register('name')} />
              {errors.name && <p className="text-xs text-destructive">{errors.name.message}</p>}
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="email">E-mail</Label>
              <Input id="email" type="email" {...register('email')} />
              {errors.email && <p className="text-xs text-destructive">{errors.email.message}</p>}
            </div>
            <div className="space-y-1.5">
              <Label>Senioridade</Label>
              <Select
                value={watch('seniority')}
                onValueChange={(v) => setValue('seniority', v as Seniority, { shouldValidate: true })}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {seniorityOptions.map((s) => (
                    <SelectItem key={s} value={s}>
                      {s}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-1.5">
              <Label>Cidade</Label>
              <Combobox
                options={cityOpts}
                value={watch('cityId')}
                onChange={(v) => setValue('cityId', v, { shouldValidate: true })}
                placeholder="Selecione a cidade"
              />
              {errors.cityId && <p className="text-xs text-destructive">{errors.cityId.message}</p>}
            </div>
          </div>
          <div className="space-y-1.5">
            <Label>Linguagens</Label>
            <MultiSelect
              options={langOpts}
              value={watch('programmingLanguageIds')}
              onChange={(v) => setValue('programmingLanguageIds', v, { shouldValidate: true })}
              placeholder="Selecione as linguagens"
            />
            {errors.programmingLanguageIds && (
              <p className="text-xs text-destructive">{errors.programmingLanguageIds.message}</p>
            )}
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="observations">Observações</Label>
            <Textarea id="observations" rows={3} {...register('observations')} />
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Cancelar
            </Button>
            <Button type="submit" disabled={isSubmitting || create.isPending || update.isPending}>
              {(create.isPending || update.isPending) && (
                <span className="mr-2 inline-block h-4 w-4 animate-spin rounded-full border-2 border-current border-r-transparent" />
              )}
              {editing ? 'Salvar' : 'Criar'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

export function DevelopersPage() {
  const qc = useQueryClient();
  const cities = useQuery({ queryKey: ['cities'], queryFn: devManagerApi.listCities });
  const langs = useQuery({ queryKey: ['languages'], queryFn: devManagerApi.listLanguages });

  const [search, setSearch] = useState('');
  const [cityFilter, setCityFilter] = useState('all');
  const [seniorityFilter, setSeniorityFilter] = useState('all');
  const [langFilter, setLangFilter] = useState('all');
  const [page, setPage] = useState(1);
  const [dialog, setDialog] = useState(false);
  const [editing, setEditing] = useState<DeveloperItem | null>(null);
  const [toDelete, setToDelete] = useState<DeveloperItem | null>(null);

  const devs = useQuery({
    queryKey: ['developers', page, search, cityFilter, seniorityFilter, langFilter],
    queryFn: () =>
      devManagerApi.listDevelopers({
        page,
        pageSize: PAGE_SIZE,
        name: search || undefined,
        cityId: cityFilter !== 'all' ? cityFilter : undefined,
        seniority: seniorityFilter !== 'all' ? seniorityFilter : undefined,
        languageId: langFilter !== 'all' ? langFilter : undefined,
      }),
  });

  const del = useMutation({
    mutationFn: (id: string) => devManagerApi.deleteDeveloper(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['developers'] });
      toast.success('Desenvolvedor removido');
      setToDelete(null);
    },
    onError: () => toast.error('Não foi possível excluir o desenvolvedor'),
  });

  const pageItems = devs.data?.items ?? [];
  const total = devs.data?.total ?? 0;
  const pageCount = Math.max(1, devs.data?.totalPages ?? 1);

  const cityName = (id: string) => cities.data?.find((c) => c.id === id)?.name ?? '—';
  const stateUf = (cityId: string) => {
    const city = cities.data?.find((x) => x.id === cityId);
    return city?.stateName?.split(' ').pop() ?? '—';
  };

  return (
    <PageContainer>
      <PageTitle
        title="Developers"
        description="Gerencie sua equipe de desenvolvedores."
        actions={
          <Button
            onClick={() => {
              setEditing(null);
              setDialog(true);
            }}
          >
            <Plus className="h-4 w-4" />
            Novo Desenvolvedor
          </Button>
        }
      />

      <div className="rounded-xl border bg-card shadow-sm">
        <div className="flex flex-col gap-3 border-b p-4 md:flex-row md:items-center">
          <SearchInput
            value={search}
            onChange={(v) => {
              setSearch(v);
              setPage(1);
            }}
            placeholder="Pesquisar por nome ou e-mail..."
            className="md:max-w-xs md:flex-1"
          />
          <div className="grid grid-cols-1 gap-2 sm:grid-cols-3 md:flex md:items-center">
            <Select
              value={cityFilter}
              onValueChange={(v) => {
                setCityFilter(v);
                setPage(1);
              }}
            >
              <SelectTrigger className="h-9 md:w-40">
                <SelectValue placeholder="Cidade" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Todas as cidades</SelectItem>
                {cities.data?.map((c) => (
                  <SelectItem key={c.id} value={c.id}>
                    {c.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select
              value={seniorityFilter}
              onValueChange={(v) => {
                setSeniorityFilter(v);
                setPage(1);
              }}
            >
              <SelectTrigger className="h-9 md:w-40">
                <SelectValue placeholder="Senioridade" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Todas senioridades</SelectItem>
                {seniorityOptions.map((s) => (
                  <SelectItem key={s} value={s}>
                    {s}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select
              value={langFilter}
              onValueChange={(v) => {
                setLangFilter(v);
                setPage(1);
              }}
            >
              <SelectTrigger className="h-9 md:w-40">
                <SelectValue placeholder="Linguagem" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Todas linguagens</SelectItem>
                {langs.data?.map((l) => (
                  <SelectItem key={l.id} value={l.id}>
                    {l.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>

        {devs.isLoading ? (
          <div className="p-4">
            <LoadingTable rows={6} cols={6} />
          </div>
        ) : pageItems.length === 0 ? (
          <EmptyState
            icon={Users}
            title="Nenhum desenvolvedor encontrado"
            description="Ajuste os filtros ou adicione um novo desenvolvedor para começar."
            action={
              <Button
                onClick={() => {
                  setEditing(null);
                  setDialog(true);
                }}
              >
                <Plus className="h-4 w-4" />
                Novo Desenvolvedor
              </Button>
            }
          />
        ) : (
          <>
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Nome</TableHead>
                    <TableHead className="hidden md:table-cell">Email</TableHead>
                    <TableHead>Cidade</TableHead>
                    <TableHead className="hidden sm:table-cell">UF</TableHead>
                    <TableHead>Senioridade</TableHead>
                    <TableHead className="hidden lg:table-cell">Linguagens</TableHead>
                    <TableHead className="w-20 text-right">Ações</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {pageItems.map((d) => (
                    <TableRow key={d.id}>
                      <TableCell>
                        <div className="font-medium">{d.name}</div>
                        <div className="text-xs text-muted-foreground md:hidden">{d.email}</div>
                      </TableCell>
                      <TableCell className="hidden text-sm text-muted-foreground md:table-cell">{d.email}</TableCell>
                      <TableCell>{d.cityName ?? cityName(d.cityId)}</TableCell>
                      <TableCell className="hidden sm:table-cell">{d.stateName ?? stateUf(d.cityId)}</TableCell>
                      <TableCell>
                        <Badge variant="secondary">{d.seniority}</Badge>
                      </TableCell>
                      <TableCell className="hidden lg:table-cell">
                        <div className="flex flex-wrap gap-1">
                          {d.programmingLanguages.slice(0, 3).map((name) => (
                            <Badge key={name} variant="outline">
                              {name}
                            </Badge>
                          ))}
                          {d.programmingLanguages.length > 3 && (
                            <Badge variant="outline">+{d.programmingLanguages.length - 3}</Badge>
                          )}
                        </div>
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="inline-flex">
                          <Button
                            variant="ghost"
                            size="icon"
                            onClick={() => {
                              setEditing(d);
                              setDialog(true);
                            }}
                            aria-label="Editar"
                          >
                            <Pencil className="h-4 w-4" />
                          </Button>
                          <Button variant="ghost" size="icon" onClick={() => setToDelete(d)} aria-label="Excluir">
                            <Trash2 className="h-4 w-4 text-destructive" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
            <div className="flex flex-col items-center justify-between gap-2 border-t p-3 text-xs text-muted-foreground sm:flex-row">
              <div>
                Mostrando <span className="font-medium text-foreground">{(page - 1) * PAGE_SIZE + 1}</span>–
                <span className="font-medium text-foreground">{Math.min(page * PAGE_SIZE, total)}</span> de{' '}
                <span className="font-medium text-foreground">{total}</span>
              </div>
              <div className="flex items-center gap-1">
                <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
                  <ChevronLeft className="h-4 w-4" />
                  Anterior
                </Button>
                <span className="px-2">
                  {page} / {pageCount}
                </span>
                <Button variant="outline" size="sm" disabled={page >= pageCount} onClick={() => setPage((p) => p + 1)}>
                  Próxima
                  <ChevronRight className="h-4 w-4" />
                </Button>
              </div>
            </div>
          </>
        )}
      </div>

      <DeveloperDialog open={dialog} onOpenChange={setDialog} editing={editing} />
      <DeleteDialog
        open={!!toDelete}
        onOpenChange={(v) => !v && setToDelete(null)}
        title={`Excluir ${toDelete?.name}?`}
        description="O desenvolvedor será removido permanentemente."
        loading={del.isPending}
        onConfirm={() => {
          if (toDelete) del.mutate(toDelete.id);
        }}
      />
    </PageContainer>
  );
}
