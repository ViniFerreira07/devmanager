'use client';

import { useQuery } from '@tanstack/react-query';
import { Users, Code2, Building2, Map as MapIcon, Activity } from 'lucide-react';
import {
  Bar,
  BarChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
  Pie,
  PieChart,
  Cell,
  Legend,
} from 'recharts';
import { PageContainer, PageTitle } from '@/components/layout/AppShell';
import { StatCard } from '@/components/common';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { devManagerApi, developerUsesLanguage } from '@/services/devManagerApi';
import type { Seniority } from '@/types';

const SENIORITIES: Seniority[] = ['Junior', 'Pleno', 'Senior', 'Lead'];
const COLORS = ['hsl(var(--chart-1))', 'hsl(var(--chart-2))', 'hsl(var(--chart-3))', 'hsl(var(--chart-4))'];

export function DashboardPage() {
  const devs = useQuery({ queryKey: ['developers', 'all'], queryFn: devManagerApi.listAllDevelopers });
  const states = useQuery({ queryKey: ['states'], queryFn: devManagerApi.listStates });
  const cities = useQuery({ queryKey: ['cities'], queryFn: devManagerApi.listCities });
  const langs = useQuery({ queryKey: ['languages'], queryFn: devManagerApi.listLanguages });

  const seniorityData = SENIORITIES.map((s) => ({
    name: s,
    value: (devs.data ?? []).filter((d) => d.seniority === s).length,
  }));

  const techData = (langs.data ?? [])
    .map((l) => ({
      name: l.name,
      value: (devs.data ?? []).filter((d) => developerUsesLanguage(d, l)).length,
    }))
    .sort((a, b) => b.value - a.value)
    .slice(0, 6);

  const recent = [...(devs.data ?? [])].slice(0, 5);
  const cityName = (id: string) => cities.data?.find((c) => c.id === id)?.name ?? '—';

  return (
    <PageContainer>
      <PageTitle title="Dashboard" description="Visão geral da sua equipe e tecnologias." />

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard
          label="Total de Desenvolvedores"
          value={devs.isLoading ? <Skeleton className="h-8 w-16" /> : (devs.data?.length ?? 0)}
          icon={Users}
          trend="+12%"
          hint="vs. mês passado"
        />
        <StatCard
          label="Total de Linguagens"
          value={langs.isLoading ? <Skeleton className="h-8 w-16" /> : (langs.data?.length ?? 0)}
          icon={Code2}
        />
        <StatCard
          label="Total de Cidades"
          value={cities.isLoading ? <Skeleton className="h-8 w-16" /> : (cities.data?.length ?? 0)}
          icon={Building2}
        />
        <StatCard
          label="Total de Estados"
          value={states.isLoading ? <Skeleton className="h-8 w-16" /> : (states.data?.length ?? 0)}
          icon={MapIcon}
        />
      </div>

      <div className="mt-6 grid grid-cols-1 gap-4 lg:grid-cols-3">
        <div className="rounded-xl border bg-card p-5 shadow-sm lg:col-span-2">
          <div className="mb-4">
            <h2 className="text-sm font-semibold">Tecnologias mais usadas</h2>
            <p className="text-xs text-muted-foreground">Top linguagens entre os desenvolvedores.</p>
          </div>
          <div className="h-64">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={techData}>
                <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" vertical={false} />
                <XAxis dataKey="name" stroke="hsl(var(--muted-foreground))" fontSize={12} tickLine={false} axisLine={false} />
                <YAxis stroke="hsl(var(--muted-foreground))" fontSize={12} tickLine={false} axisLine={false} allowDecimals={false} />
                <Tooltip
                  cursor={{ fill: 'hsl(var(--muted))' }}
                  contentStyle={{
                    background: 'hsl(var(--popover))',
                    border: '1px solid hsl(var(--border))',
                    borderRadius: 8,
                    fontSize: 12,
                  }}
                />
                <Bar dataKey="value" fill="hsl(var(--primary))" radius={[6, 6, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>

        <div className="rounded-xl border bg-card p-5 shadow-sm">
          <div className="mb-4">
            <h2 className="text-sm font-semibold">Senioridade</h2>
            <p className="text-xs text-muted-foreground">Distribuição da equipe.</p>
          </div>
          <div className="h-64">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie data={seniorityData} dataKey="value" nameKey="name" innerRadius={50} outerRadius={80} paddingAngle={3}>
                  {seniorityData.map((_, i) => (
                    <Cell key={i} fill={COLORS[i % COLORS.length]} />
                  ))}
                </Pie>
                <Legend iconType="circle" wrapperStyle={{ fontSize: 12 }} />
                <Tooltip
                  contentStyle={{
                    background: 'hsl(var(--popover))',
                    border: '1px solid hsl(var(--border))',
                    borderRadius: 8,
                    fontSize: 12,
                  }}
                />
              </PieChart>
            </ResponsiveContainer>
          </div>
        </div>
      </div>

      <div className="mt-6 grid grid-cols-1 gap-4 lg:grid-cols-3">
        <div className="rounded-xl border bg-card shadow-sm lg:col-span-2">
          <div className="border-b p-5">
            <h2 className="text-sm font-semibold">Últimos desenvolvedores</h2>
            <p className="text-xs text-muted-foreground">Adicionados recentemente à plataforma.</p>
          </div>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Nome</TableHead>
                <TableHead>Cidade</TableHead>
                <TableHead>Senioridade</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {recent.map((d) => (
                <TableRow key={d.id}>
                  <TableCell>
                    <div className="font-medium">{d.name}</div>
                    <div className="text-xs text-muted-foreground">{d.email}</div>
                  </TableCell>
                  <TableCell>{d.cityName ?? cityName(d.cityId)}</TableCell>
                  <TableCell>
                    <Badge variant="secondary">{d.seniority}</Badge>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>

        <div className="rounded-xl border bg-card shadow-sm">
          <div className="border-b p-5">
            <h2 className="text-sm font-semibold">Atividades recentes</h2>
            <p className="text-xs text-muted-foreground">Últimas ações no sistema.</p>
          </div>
          <ul className="divide-y">
            {recent.slice(0, 5).map((d) => (
              <li key={d.id} className="flex items-start gap-3 p-4">
                <div className="mt-0.5 grid h-7 w-7 place-items-center rounded-full bg-muted">
                  <Activity className="h-3.5 w-3.5 text-muted-foreground" />
                </div>
                <div className="min-w-0 flex-1 text-sm">
                  <p className="truncate">
                    <span className="font-medium">{d.name}</span> foi adicionado
                  </p>
                  <p className="text-xs text-muted-foreground">{d.cityName ?? cityName(d.cityId)}</p>
                </div>
              </li>
            ))}
          </ul>
        </div>
      </div>
    </PageContainer>
  );
}
