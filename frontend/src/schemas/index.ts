import { z } from 'zod';

export const loginSchema = z.object({
  email: z.string().email('Email inválido'),
  password: z.string().min(1, 'Informe a senha'),
});

export const stateSchema = z.object({
  name: z.string().min(2, 'Nome obrigatório'),
  uf: z.string().length(2, 'UF com 2 letras'),
});

export const citySchema = z.object({
  name: z.string().min(2, 'Nome obrigatório'),
  stateId: z.string().min(1, 'Selecione o estado'),
});

export const languageSchema = z.object({
  name: z.string().min(1, 'Nome obrigatório'),
  type: z.enum(['Backend', 'Frontend', 'Mobile', 'Database', 'Cloud', 'DevOps', 'Game']),
});

export const developerSchema = z.object({
  name: z.string().min(2, 'Informe o nome'),
  email: z.string().email('E-mail inválido'),
  seniority: z.enum(['Junior', 'Pleno', 'Senior', 'Lead']),
  cityId: z.string().min(1, 'Selecione a cidade'),
  programmingLanguageIds: z.array(z.string()).min(1, 'Selecione ao menos uma linguagem'),
  observations: z.string().optional(),
});