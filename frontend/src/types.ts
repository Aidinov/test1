export type DocumentStatus =
  | 'Draft'
  | 'UnderReview'
  | 'Approved'
  | 'Rejected';

export enum CommentType {
  Question = 'Question',
  Remark = 'Remark'
}

export enum RemarkSeverity {
  Critical = 'Critical',
  Desirable = 'Desirable',
  Opinion = 'Opinion'
}

export interface DocumentSummary {
  id: string;
  title: string;
  product: string;
  team: string;
  author: string;
  taskLink: string;
  gitRepository: string;
  gitFilePath: string;
  gitCommitHash: string;
  status: DocumentStatus;
  createdAt: string;
  updatedAt: string;
}

export interface DocumentDetails extends DocumentSummary {
  content: string;
  comments: CommentResponse[];
}

export interface CreateDocumentRequest {
  title: string;
  product: string;
  team: string;
  author: string;
  taskLink: string;
  content: string;
  gitRepository: string;
  gitFilePath: string;
  gitCommitHash?: string;
  status?: DocumentStatus;
}

export interface UpdateDocumentRequest {
  title: string;
  product: string;
  team: string;
  author: string;
  taskLink: string;
  content: string;
  gitRepository: string;
  gitFilePath: string;
  gitCommitHash?: string;
  status: DocumentStatus;
}

export interface Comment {
  id: string;
  documentId: string;
  author: string;
  startIndex: number;
  endIndex: number;
  type: CommentType;
  severity: RemarkSeverity;
  content: string;
  createdAt: string;
  updatedAt: string;
  isResolved: boolean;
  resolvedBy?: string;
  resolvedAt?: string;
  documentVersion?: string;
  originalText?: string;
}

export interface CommentRequest {
  startIndex: number;
  endIndex: number;
  type: CommentType;
  severity: RemarkSeverity;
  content: string;
  author: string;
}

export interface CommentReply {
  id: string;
  author: string;
  content: string;
  createdAt: string;
}

export interface CommentResponse extends Comment {
  replies: CommentReply[];
}

export type UserRole = 'Author' | 'Reviewer' | 'Approver';
