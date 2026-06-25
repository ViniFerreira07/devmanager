import { z } from 'zod';

export const loginSchema = z.object({
  email: z.string().email('Email inválido'),
  password: z.string().min(1, 'Informe a senha'),
});

export const registerSchema = z.object({
  name: z.string().min(2, 'Nome obrigatório'),
  email: z.string().email('Email inválido'),
  password: z.string().min(6, 'Senha mínima de 6 caracteres'),
});

export const stateSchema = z.object({
  name: z.string().min(1, 'Nome obrigatório'),
  uf: z.string().length(2, 'UF deve ter 2 letras'),
});

export const citySchema = z.object({
  name: z.string().min(1, 'Nome obrigatório'),
  stateId: z.string().min(1, 'Estado obrigatório'),
});

export const languageSchema = z.object({
  name: z.string().min(1, 'Nome obrigatório'),
  type: z.string().min(1, 'Tipo obrigatório'),
});

export const developerSchema = z.object({
  name: z.string().min(1, 'Nome obrigatório'),
  email: z.string().email('Email inválido'),
  seniority: z.string().min(1, 'Senioridade obrigatória'),
  cityId: z.string().min(1, 'Cidade obrigatória'),
  observations: z.string().optional(),
  programmingLanguageIds: z.array(z.string()).min(1, 'Informe pelo menos uma linguagem'),
});
