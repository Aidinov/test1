import { DocumentStatus } from '../../types';

export interface DocumentQuery {
  team?: string;
  product?: string;
  author?: string;
  status?: DocumentStatus;
  page?: number;
  pageSize?: number;
}

export function buildQuery(params: DocumentQuery = {}): string {
  const search = new URLSearchParams();
  if (params.team) search.set('team', params.team);
  if (params.product) search.set('product', params.product);
  if (params.author) search.set('author', params.author);
  if (params.status) search.set('status', params.status);
  if (params.page !== undefined) search.set('page', String(params.page));
  if (params.pageSize !== undefined) search.set('pageSize', String(params.pageSize));
  const qs = search.toString();
  return qs ? `?${qs}` : '';
}
