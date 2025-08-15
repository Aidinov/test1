import http from '../lib/http';
import {
  DocumentSummary,
  DocumentDetails,
  CreateDocumentRequest,
  UpdateDocumentRequest
} from '../types';
import { buildQuery, DocumentQuery } from './helpers/query';

export async function listDocuments(query?: DocumentQuery): Promise<DocumentSummary[]> {
  const res = await http.get<DocumentSummary[]>(`/documents${buildQuery(query)}`);
  return res.data;
}

export async function getDocument(id: string): Promise<DocumentDetails> {
  const res = await http.get<DocumentDetails>(`/documents/${id}`);
  return res.data;
}

export async function createDocument(req: CreateDocumentRequest): Promise<DocumentSummary> {
  const res = await http.post<DocumentSummary>('/documents', req);
  return res.data;
}

export async function updateDocument(id: string, req: UpdateDocumentRequest): Promise<DocumentSummary> {
  const res = await http.put<DocumentSummary>(`/documents/${id}`, req);
  return res.data;
}
