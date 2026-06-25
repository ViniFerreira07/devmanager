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

export interface ProgrammingLanguageItem {
  id: string;
  name: string;
  type: string;
}

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
