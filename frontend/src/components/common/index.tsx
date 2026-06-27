import { type ReactNode, useEffect, useRef } from 'react';
import { Search } from 'lucide-react';
import { cn } from '@/lib/utils';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { confirmAlert } from '@/lib/swal';

export function SearchInput({
  value,
  onChange,
  placeholder = 'Pesquisar...',
  className,
}: {
  value: string;
  onChange: (v: string) => void;
  placeholder?: string;
  className?: string;
}) {
  return (
    <div className={cn('relative', className)}>
      <Search className="pointer-events-none absolute left-2.5 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
      <Input
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        className="h-9 pl-8"
        aria-label={placeholder}
      />
    </div>
  );
}

export function StatCard({
  label,
  value,
  hint,
  icon: Icon,
  trend,
}: {
  label: string;
  value: ReactNode;
  hint?: string;
  icon?: React.ComponentType<{ className?: string }>;
  trend?: string;
}) {
  return (
    <div className="rounded-xl border bg-card p-5 shadow-sm transition-shadow hover:shadow-md">
      <div className="flex items-start justify-between">
        <div className="text-sm text-muted-foreground">{label}</div>
        {Icon && (
          <div className="grid h-8 w-8 place-items-center rounded-lg bg-muted">
            <Icon className="h-4 w-4 text-muted-foreground" />
          </div>
        )}
      </div>
      <div className="mt-3 text-3xl font-semibold tracking-tight">{value}</div>
      {(hint || trend) && (
        <div className="mt-1 flex items-center gap-2 text-xs text-muted-foreground">
          {trend && <span className="font-medium text-success">{trend}</span>}
          {hint && <span>{hint}</span>}
        </div>
      )}
    </div>
  );
}

export function EmptyState({
  title,
  description,
  action,
  icon: Icon,
}: {
  title: string;
  description?: string;
  action?: ReactNode;
  icon?: React.ComponentType<{ className?: string }>;
}) {
  return (
    <div className="flex flex-col items-center justify-center rounded-xl border border-dashed bg-card/50 px-6 py-16 text-center">
      {Icon && (
        <div className="mb-4 grid h-12 w-12 place-items-center rounded-full bg-muted">
          <Icon className="h-5 w-5 text-muted-foreground" />
        </div>
      )}
      <h3 className="text-base font-semibold">{title}</h3>
      {description && <p className="mt-1 max-w-sm text-sm text-muted-foreground">{description}</p>}
      {action && <div className="mt-5">{action}</div>}
    </div>
  );
}

export function DeleteDialog({
  open,
  onOpenChange,
  title = 'Excluir item?',
  description = 'Esta ação não pode ser desfeita.',
  onConfirm,
  loading,
}: {
  open: boolean;
  onOpenChange: (v: boolean) => void;
  title?: string;
  description?: string;
  onConfirm: () => void | Promise<void>;
  loading?: boolean;
}) {
  const isOpening = useRef(false);

  useEffect(() => {
    if (open && !isOpening.current) {
      isOpening.current = true;
      confirmAlert({
        title,
        text: description,
        icon: 'warning',
        confirmButtonText: 'Sim, excluir',
        cancelButtonText: 'Cancelar',
        variant: 'destructive',
      }).then((result) => {
        isOpening.current = false;
        if (result.isConfirmed) {
          onConfirm();
        } else {
          onOpenChange(false);
        }
      });
    }
  }, [open, title, description, onConfirm, onOpenChange]);

  return null;
}

export function LoadingTable({ rows = 6, cols = 5 }: { rows?: number; cols?: number }) {
  return (
    <div className="space-y-2">
      {Array.from({ length: rows }).map((_, i) => (
        <div key={i} className="grid gap-3 rounded-md border p-3" style={{ gridTemplateColumns: `repeat(${cols}, minmax(0, 1fr))` }}>
          {Array.from({ length: cols }).map((__, j) => (
            <Skeleton key={j} className="h-4" />
          ))}
        </div>
      ))}
    </div>
  );
}

export function LoadingButton({
  loading,
  children,
  ...props
}: React.ButtonHTMLAttributes<HTMLButtonElement> & { loading?: boolean }) {
  return (
    <Button {...props} disabled={loading || props.disabled}>
      {loading ? (
        <span className="inline-block h-4 w-4 animate-spin rounded-full border-2 border-current border-r-transparent" />
      ) : null}
      {children}
    </Button>
  );
}
