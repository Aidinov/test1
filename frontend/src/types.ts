export enum CommentType {
  Question = 'Question',
  Remark = 'Remark'
}

export enum RemarkSeverity {
  Critical = 'Critical',
  Desirable = 'Desirable',
  Opinion = 'Opinion'
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

  /**
   * The original text snippet this comment was attached to when it was created.  This
   * allows the UI to display context for the comment even if the underlying
   * document has since changed or the text was removed.
   */
  originalText?: string;
}