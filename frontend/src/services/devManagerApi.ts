import api from './api';
import type {
  CityItem,
  DeveloperFilter,
  DeveloperItem,
  LanguageType,
  PagedResponse,
  ProgrammingLanguageItem,
  StateItem,
} from '@/types';

export const devManagerApi = {
  listStates: async () => {
    const { data } = await api.get<StateItem[]>('/api/states');
    return data;
  },

  createState: async (input: { name: string; uf: string }) => {
    const { data } = await api.post<StateItem>('/api/states', { name: input.name, uf: input.uf.toUpperCase() });
    return data;
  },

  updateState: async (id: string, input: { name: string; uf: string }) => {
    const { data } = await api.put<StateItem>(`/api/states/${id}`, { id, name: input.name, uf: input.uf.toUpperCase() });
    return data;
  },

  deleteState: async (id: string) => {
    await api.delete(`/api/states/${id}`);
  },

  listCities: async () => {
    const { data } = await api.get<CityItem[]>('/api/cities');
    return data;
  },

  createCity: async (input: { name: string; stateId: string }) => {
    const { data } = await api.post<CityItem>('/api/cities', input);
    return data;
  },

  updateCity: async (id: string, input: { name: string; stateId: string }) => {
    const { data } = await api.put<CityItem>(`/api/cities/${id}`, { id, ...input });
    return data;
  },

  deleteCity: async (id: string) => {
    await api.delete(`/api/cities/${id}`);
  },

  listLanguages: async () => {
    const { data } = await api.get<ProgrammingLanguageItem[]>('/api/languages');
    return data;
  },

  createLanguage: async (input: { name: string; type: LanguageType }) => {
    const { data } = await api.post<ProgrammingLanguageItem>('/api/languages', input);
    return data;
  },

  updateLanguage: async (id: string, input: { name: string; type: LanguageType }) => {
    const { data } = await api.put<ProgrammingLanguageItem>(`/api/languages/${id}`, { id, ...input });
    return data;
  },

  deleteLanguage: async (id: string) => {
    await api.delete(`/api/languages/${id}`);
  },

  listDevelopers: async (filter: DeveloperFilter = {}) => {
    const { data } = await api.get<PagedResponse<DeveloperItem>>('/api/developers', {
      params: {
        page: filter.page ?? 1,
        pageSize: filter.pageSize ?? 8,
        name: filter.name || undefined,
        cityId: filter.cityId || undefined,
        languageId: filter.languageId || undefined,
        seniority: filter.seniority || undefined,
      },
    });
    return data;
  },

  listAllDevelopers: async () => {
    const { data } = await api.get<PagedResponse<DeveloperItem>>('/api/developers', {
      params: { page: 1, pageSize: 1000 },
    });
    return data.items;
  },

  createDeveloper: async (input: {
    name: string;
    email: string;
    seniority: string;
    cityId: string;
    observations?: string;
    programmingLanguageIds: string[];
  }) => {
    const { data } = await api.post<DeveloperItem>('/api/developers', input);
    return data;
  },

  updateDeveloper: async (
    id: string,
    input: {
      name: string;
      email: string;
      seniority: string;
      cityId: string;
      observations?: string;
      programmingLanguageIds: string[];
    },
  ) => {
    const { data } = await api.put<DeveloperItem>(`/api/developers/${id}`, { id, ...input });
    return data;
  },

  deleteDeveloper: async (id: string) => {
    await api.delete(`/api/developers/${id}`);
  },

  downloadDevelopersReport: async () => {
    const { data } = await api.get<Blob>('/api/reports/developers', { responseType: 'blob' });
    return data;
  },
};

export function mapLanguageNamesToIds(names: string[], languages: ProgrammingLanguageItem[]) {
  return names
    .map((name) => languages.find((entry) => entry.name.toLowerCase() === name.toLowerCase())?.id ?? '')
    .filter(Boolean);
}

export function developerUsesLanguage(developer: DeveloperItem, language: ProgrammingLanguageItem) {
  return developer.programmingLanguages.some((name) => name.toLowerCase() === language.name.toLowerCase());
}
