'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import api from '@/services/api';
import { developerSchema } from '@/schemas';
import { CityItem, DeveloperItem, PagedResponse, ProgrammingLanguageItem, StateItem } from '@/types';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';

type DeveloperFormValues = z.infer<typeof developerSchema>;

export default function DevelopersPage() {
  const [items, setItems] = useState<DeveloperItem[]>([]);
  const [cities, setCities] = useState<CityItem[]>([]);
  const [languages, setLanguages] = useState<ProgrammingLanguageItem[]>([]);
  const [states, setStates] = useState<StateItem[]>([]);
  const [selectedStateId, setSelectedStateId] = useState<string>('');
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [filter, setFilter] = useState('');
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');

  const { register, handleSubmit, reset, setValue, watch, formState: { errors, isSubmitting } } = useForm<DeveloperFormValues>({
    resolver: zodResolver(developerSchema),
    defaultValues: {
      name: '',
      email: '',
      seniority: '',
      cityId: '',
      observations: '',
      programmingLanguageIds: [],
    },
  });

  const form = watch();

  const load = async () => {
    setLoading(true);
    try {
      const [developersRes, citiesRes, languagesRes, statesRes] = await Promise.all([
        api.get<PagedResponse<DeveloperItem>>('/api/developers', { params: { page, pageSize: 10, name: filter || undefined } }),
        api.get<CityItem[]>('/api/cities'),
        api.get<ProgrammingLanguageItem[]>('/api/languages'),
        api.get<StateItem[]>('/api/states'),
      ]);
      setItems(developersRes.data.items);
      setTotal(developersRes.data.total);
      setCities(citiesRes.data);
      setLanguages(languagesRes.data);
      setStates(statesRes.data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, [page]);

  const onSubmit = async (data: DeveloperFormValues) => {
    const payload = {
      name: data.name,
      email: data.email,
      seniority: data.seniority,
      cityId: data.cityId,
      observations: data.observations,
      programmingLanguageIds: data.programmingLanguageIds,
    };

    try {
      if (form.id) {
        await api.put(`/api/developers/${form.id}`, { ...payload, id: form.id });
      } else {
        await api.post('/api/developers', payload);
      }
      reset();
      setSelectedStateId('');
      setMessage('Desenvolvedor salvo com sucesso.');
      await load();
    } catch (error: any) {
      setMessage(error.response?.data?.message ?? 'Nao foi possivel salvar.');
    }
  };

  const remove = async (id: string) => {
    if (!confirm('Deseja excluir este desenvolvedor?')) return;
    await api.delete(`/api/developers/${id}`);
    setMessage('Desenvolvedor excluido.');
    await load();
  };

  const toggleLanguage = (languageId: string) => {
    const current = form.programmingLanguageIds || [];
    const updated = current.includes(languageId)
      ? current.filter((item) => item !== languageId)
      : [...current, languageId];
    setValue('programmingLanguageIds', updated);
  };

  const editDeveloper = (item: DeveloperItem) => {
    const stateId = cities.find((city) => city.id === item.cityId)?.stateId ?? '';
    setSelectedStateId(stateId);
    reset({
      name: item.name,
      email: item.email,
      seniority: item.seniority,
      cityId: item.cityId,
      observations: item.observations ?? '',
      programmingLanguageIds: item.programmingLanguages.map((language) => languages.find((entry) => entry.name.toLowerCase() === language.toLowerCase())?.id ?? '').filter(Boolean),
    });
  };

  return (
    <main className="min-h-screen bg-background p-8">
      <div className="mx-auto max-w-6xl">
        <div className="mb-6 flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-semibold">Desenvolvedores</h1>
            <p className="text-sm text-slate-500">Cadastre desenvolvedores e suas linguagens.</p>
          </div>
          <Link href="/dashboard" className="text-sm text-cyan-600">Voltar</Link>
        </div>

        <section className="mb-6 rounded-lg border border-border bg-card p-4">
          <form onSubmit={handleSubmit(onSubmit)}>
            <div className="grid gap-4 md:grid-cols-2">
              <div>
                <Input {...register('name')} placeholder="Nome completo" />
                {errors.name && <p className="mt-1 text-sm text-red-500">{errors.name.message}</p>}
              </div>
              <div>
                <Input {...register('email')} placeholder="Email" />
                {errors.email && <p className="mt-1 text-sm text-red-500">{errors.email.message}</p>}
              </div>
              <div>
                <Input {...register('seniority')} placeholder="Senioridade" />
              </div>
              <div>
                <Input {...register('observations')} placeholder="Observacoes" />
              </div>
              <div>
                <Select onValueChange={(value) => { setSelectedStateId(value); setValue('cityId', ''); }} value={selectedStateId}>
                  <SelectTrigger>
                    <SelectValue placeholder="Selecione um estado" />
                  </SelectTrigger>
                  <SelectContent>
                    {states.map((state) => <SelectItem key={state.id} value={state.id}>{state.name}</SelectItem>)}
                  </SelectContent>
                </Select>
              </div>
              <div>
                <Select onValueChange={(value) => setValue('cityId', value)} value={form.cityId}>
                  <SelectTrigger>
                    <SelectValue placeholder="Selecione uma cidade" />
                  </SelectTrigger>
                  <SelectContent>
                    {cities.filter((city) => !selectedStateId || city.stateId === selectedStateId).map((city) => <SelectItem key={city.id} value={city.id}>{city.name}</SelectItem>)}
                  </SelectContent>
                </Select>
                {errors.cityId && <p className="mt-1 text-sm text-red-500">{errors.cityId.message}</p>}
              </div>
            </div>

            <div className="mt-4">
              <p className="mb-2 text-sm text-slate-500">Linguagens</p>
              <div className="flex flex-wrap gap-2">
                {languages.map((language) => (
                  <Button key={language.id} type="button" variant={form.programmingLanguageIds?.includes(language.id) ? "default" : "outline"} size="sm" onClick={() => toggleLanguage(language.id)}>
                    {language.name}
                  </Button>
                ))}
              </div>
              {errors.programmingLanguageIds && <p className="mt-1 text-sm text-red-500">{errors.programmingLanguageIds.message}</p>}
            </div>

            {message && <p className="mt-4 rounded-md bg-cyan-500/10 px-3 py-2 text-sm text-cyan-700 dark:text-cyan-300">{message}</p>}
            <Button type="submit" disabled={isSubmitting} className="mt-4">
              Criar
            </Button>
          </form>
        </section>

        <div className="mb-3 flex gap-2">
          <Input value={filter} onChange={(e) => setFilter(e.target.value)} placeholder="Filtrar por nome" />
          <Button type="button" variant="outline" onClick={() => { setPage(1); load(); }}>Filtrar</Button>
        </div>

        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Nome</TableHead>
              <TableHead>Email</TableHead>
              <TableHead>Cidade</TableHead>
              <TableHead>Acoes</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {items.map((item) => (
              <TableRow key={item.id}>
                <TableCell>{item.name}</TableCell>
                <TableCell>{item.email}</TableCell>
                <TableCell>{item.cityName}</TableCell>
                <TableCell className="space-x-2">
                  <Button type="button" variant="link" size="sm" onClick={() => editDeveloper(item)}>Editar</Button>
                  <Button type="button" variant="destructive" size="sm" onClick={() => remove(item.id)}>Excluir</Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
        {loading && <p className="p-4 text-sm text-slate-500">Carregando...</p>}

        <div className="mt-4 flex items-center justify-between text-sm">
          <span>Total: {total}</span>
          <div className="flex gap-2">
            <Button disabled={page === 1} onClick={() => setPage((value) => Math.max(1, value - 1))}>Anterior</Button>
            <Button disabled={page * 10 >= total} onClick={() => setPage((value) => value + 1)}>Proxima</Button>
          </div>
        </div>
      </div>
    </main>
  );
}