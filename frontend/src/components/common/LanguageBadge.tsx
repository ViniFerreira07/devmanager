'use client';

import { Badge } from '@/components/ui/badge';
import type { ProgrammingLanguageItem } from '@/types';

interface LanguageBadgeProps {
  language: ProgrammingLanguageItem;
}

const typeStyles: Record<string, string> = {
  Backend: 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200',
  Frontend: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200',
  Mobile: 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200',
  Database: 'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200',
  Cloud: 'bg-cyan-100 text-cyan-800 dark:bg-cyan-900 dark:text-cyan-200',
  DevOps: 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-200',
  Game: 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200',
};

export function LanguageBadge({ language }: LanguageBadgeProps) {
  const style = typeStyles[language.type] ?? typeStyles.Backend;

  return (
    <Badge
      variant="outline"
      className={`inline-flex items-center gap-1 px-2 py-0.5 text-xs font-medium rounded-full ${style}`}
    >
      <span
        className="inline-block w-2 h-2 rounded-full"
        style={{ backgroundColor: language.color }}
      />
      {language.name}
    </Badge>
  );
}