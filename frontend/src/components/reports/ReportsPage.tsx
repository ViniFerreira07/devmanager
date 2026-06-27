'use client';

import { useState } from 'react';
import { Download, FileText, Users, Code2, Building2 } from 'lucide-react';
import { toast } from 'sonner';
import { PageContainer, PageTitle } from '@/components/layout/AppShell';
import { Button } from '@/components/ui/button';
import { devManagerApi } from '@/services/devManagerApi';

const reports = [
  {
    id: 'devs',
    title: 'Relatório de Desenvolvedores',
    description: 'Lista completa com senioridade, cidade e tecnologias.',
    icon: Users,
    available: true,
  },
  {
    id: 'tech',
    title: 'Tecnologias mais usadas',
    description: 'Ranking de linguagens e frameworks utilizados.',
    icon: Code2,
    available: false,
  },
  {
    id: 'geo',
    title: 'Distribuição Geográfica',
    description: 'Desenvolvedores por estado e cidade.',
    icon: Building2,
    available: false,
  },
];

export function ReportsPage() {
  const [loading, setLoading] = useState<string | null>(null);

  const handleExport = async (id: string, title: string, available: boolean) => {
    if (!available) {
      toast.info('Este relatório estará disponível em breve.');
      return;
    }

    setLoading(id);
    try {
      const blob = await devManagerApi.downloadDevelopersReport();
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = 'developers-report.pdf';
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
      toast.success(`${title} gerado com sucesso`);
    } catch {
      toast.error('Não foi possível gerar o relatório');
    } finally {
      setLoading(null);
    }
  };

  return (
    <PageContainer>
      <PageTitle title="Relatórios" description="Exporte relatórios em PDF da sua operação." />

      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
        {reports.map((r) => (
          <div
            key={r.id}
            className="group rounded-xl border bg-card p-5 shadow-sm transition-all hover:-translate-y-0.5 hover:shadow-md"
          >
            <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary/10 text-primary">
              <r.icon className="h-5 w-5" />
            </div>
            <h3 className="mt-4 text-base font-semibold">{r.title}</h3>
            <p className="mt-1 text-sm text-muted-foreground">{r.description}</p>
            <div className="mt-5 flex items-center gap-2">
              <Button
                onClick={() => handleExport(r.id, r.title, r.available)}
                disabled={loading === r.id}
                size="sm"
              >
                {loading === r.id ? (
                  <span className="mr-2 inline-block h-3.5 w-3.5 animate-spin rounded-full border-2 border-current border-r-transparent" />
                ) : (
                  <Download className="h-4 w-4" />
                )}
                {loading === r.id ? 'Gerando...' : 'Exportar PDF'}
              </Button>
              <Button variant="ghost" size="sm" disabled={!r.available}>
                <FileText className="h-4 w-4" />
                Visualizar
              </Button>
            </div>
          </div>
        ))}
      </div>
    </PageContainer>
  );
}
