import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom/vitest';
import { describe, it, expect } from 'vitest';
import CommentsPanel from './CommentsPanel';
import { CommentResponse, CommentType, RemarkSeverity } from '../types';
import { UserRoleProvider } from '../lib/UserRoleContext';

declare const document: any; // avoid TS DOM issues in tests

const baseComment: CommentResponse = {
  id: 'c1',
  documentId: 'd1',
  author: 'A',
  startIndex: 0,
  endIndex: 5,
  type: CommentType.Question,
  severity: RemarkSeverity.Opinion,
  content: 'test',
  createdAt: '',
  updatedAt: '',
  isResolved: false,
  replies: [],
};

describe('CommentsPanel', () => {
  it('gates resolve button by role', () => {
    const comment = { ...baseComment };
    const { rerender } = render(
      <UserRoleProvider role="Author">
        <CommentsPanel
          open
          onClose={() => {}}
          comments={[comment]}
          documentContent="hello"
          documentVersion="v1"
          onReply={() => {}}
          onResolve={() => {}}
        />
      </UserRoleProvider>,
    );
    expect(screen.queryByTestId('resolve-btn')).not.toBeInTheDocument();

    rerender(
      <UserRoleProvider role="Reviewer">
        <CommentsPanel
          open
          onClose={() => {}}
          comments={[comment]}
          documentContent="hello"
          documentVersion="v1"
          onReply={() => {}}
          onResolve={() => {}}
        />
      </UserRoleProvider>,
    );
    expect(screen.getByTestId('resolve-btn')).toBeInTheDocument();
  });

  it('shows dangling comments in second tab', () => {
    const dangling = {
      ...baseComment,
      id: 'c2',
      originalText: 'xyz',
      startIndex: 0,
      endIndex: 3,
      type: CommentType.Remark,
      severity: RemarkSeverity.Critical,
    };
    render(
      <UserRoleProvider role="Reviewer">
        <CommentsPanel
          open
          onClose={() => {}}
          comments={[dangling]}
          documentContent="abc"
          documentVersion="v1"
          onReply={() => {}}
          onResolve={() => {}}
        />
      </UserRoleProvider>,
    );
    fireEvent.click(screen.getByText('Dangling (1)'));
    expect(screen.getByText('xyz')).toBeInTheDocument();
  });

  it('shows outdated chip when versions differ', () => {
    const comment = {
      ...baseComment,
      documentVersion: 'abcdef1',
      originalText: 'hello',
    };
    render(
      <UserRoleProvider role="Reviewer">
        <CommentsPanel
          open
          onClose={() => {}}
          comments={[comment]}
          documentContent="hello world"
          documentVersion="1234567"
          onReply={() => {}}
          onResolve={() => {}}
        />
      </UserRoleProvider>,
    );
    expect(screen.getByText('Outdated since abcdef1')).toBeInTheDocument();
  });
});
