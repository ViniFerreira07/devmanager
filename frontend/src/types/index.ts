export interface UserSession {
  email: string;
  name: string;
  token: string;
}

export interface StateItem {
  id: string;
  name: string;
  uf: string;
}

export interface CityItem {
  id: string;
  name: string;
  stateId: string;
  stateName?: string;
}

export type LanguageType = 'Backend' | 'Frontend' | 'Mobile' | 'Database' | 'Cloud' | 'DevOps' | 'Game';

export interface ProgrammingLanguageItem {
  id: string;
  name: string;
  type: LanguageType;
  color: string;
  icon: string;
}

export type Seniority = 'Junior' | 'Pleno' | 'Senior' | 'Lead';

export interface DeveloperItem {
  id: string;
  name: string;
  email: string;
  seniority: string;
  cityId: string;
  cityName?: string;
  stateName?: string;
  observations?: string;
  programmingLanguages: string[];
}

export interface PaginationMeta {
  page: number;
  pageSize: number;
  total: number;
  totalPages: number;
}

export interface PagedResponse<T> extends PaginationMeta {
  items: T[];
}

export interface DeveloperFilter {
  page?: number;
  pageSize?: number;
  name?: string;
  cityId?: string;
  languageId?: string;
  seniority?: string;
}

export interface ApiError {
  success: boolean;
  code: string;
  message: string;
  errors: string[];
}