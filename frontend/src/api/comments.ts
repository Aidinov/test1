import http from '../lib/http';
import { Comment, CommentRequest } from '../types';

export async function listComments(documentId: string): Promise<Comment[]> {
  const res = await http.get<Comment[]>(`/documents/${documentId}/comments`);
  return res.data;
}

export async function addComment(documentId: string, req: CommentRequest): Promise<Comment> {
  const res = await http.post<Comment>(`/documents/${documentId}/comments`, req);
  return res.data;
}

export async function resolveComment(documentId: string, commentId: string, resolvedBy: string): Promise<Comment> {
  const res = await http.post<Comment>(
    `/documents/${documentId}/comments/${commentId}/resolve`,
    null,
    { params: { resolvedBy } }
  );
  return res.data;
}
