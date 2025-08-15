import http from '../lib/http';
import { CommentResponse, CommentRequest, CommentReply } from '../types';

export async function listComments(documentId: string): Promise<CommentResponse[]> {
  const res = await http.get<CommentResponse[]>(`/documents/${documentId}/comments`);
  return res.data;
}

export async function addComment(
  documentId: string,
  req: CommentRequest,
): Promise<CommentResponse> {
  const res = await http.post<CommentResponse>(`/documents/${documentId}/comments`, req);
  return res.data;
}

export async function resolveComment(commentId: string, resolvedBy: string): Promise<CommentResponse> {
  const res = await http.post<CommentResponse>(`/comments/${commentId}/resolve`, {
    resolvedBy,
  });
  return res.data;
}

export async function addReply(
  commentId: string,
  content: string,
  author: string,
): Promise<CommentReply> {
  // Stubbed API; backend may implement this endpoint later
  const res = await http.post<CommentReply>(`/comments/${commentId}/replies`, {
    content,
    author,
  });
  return res.data;
}
