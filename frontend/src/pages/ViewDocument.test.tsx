import { render, waitFor, fireEvent } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { SnackbarProvider } from 'notistack';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import '@testing-library/jest-dom/vitest';
import ViewDocument from './ViewDocument';
import { getDocument } from '../api/documents';
import { DocumentDetails, CommentType, RemarkSeverity } from '../types';
import { UserRoleProvider } from '../lib/UserRoleContext';

vi.mock('../api/documents');
const mockGet = vi.mocked(getDocument);

function renderView() {
  const client = new QueryClient({
    defaultOptions: {
      queries: { retry: false, refetchOnWindowFocus: false, refetchOnMount: false }
    }
  });
  return render(
    <QueryClientProvider client={client}>
      <SnackbarProvider>
        <UserRoleProvider role="Reviewer">
          <MemoryRouter initialEntries={["/documents/1"]}>
            <Routes>
              <Route path="/documents/:id" element={<ViewDocument />} />
            </Routes>
          </MemoryRouter>
        </UserRoleProvider>
      </SnackbarProvider>
    </QueryClientProvider>
  );
}

describe('ViewDocument', () => {
  beforeEach(() => {
    mockGet.mockReset();
  });

  it('loads document only once', async () => {
    const doc: DocumentDetails = {
      id: '1',
      title: 'Doc',
      product: 'Prod',
      team: 'Team',
      author: 'Alice',
      taskLink: '',
      gitRepository: '',
      gitFilePath: '',
      gitCommitHash: 'abc',
      status: 'Draft',
      createdAt: '',
      updatedAt: '',
      content: 'Hello',
      comments: []
    };
    mockGet.mockResolvedValue(doc);
    renderView();
    await waitFor(() => expect(mockGet).toHaveBeenCalledTimes(1));
  });

  it('shows comment summary on hover', async () => {
    const doc: DocumentDetails = {
      id: '1',
      title: 'Doc',
      product: 'Prod',
      team: 'Team',
      author: 'Alice',
      taskLink: '',
      gitRepository: '',
      gitFilePath: '',
      gitCommitHash: 'abc',
      status: 'Draft',
      createdAt: '',
      updatedAt: '',
      content: 'Hello world',
      comments: [
        {
          id: 'c1',
          documentId: '1',
          startIndex: 0,
          endIndex: 5,
          type: CommentType.Question,
          severity: RemarkSeverity.Opinion,
          content: 'Why?',
          author: 'Bob',
          createdAt: '',
          updatedAt: '',
          replies: [],
          isResolved: false,
        },
      ],
    };
    mockGet.mockResolvedValue(doc);
    const { getByText, queryByText } = renderView();
    await waitFor(() => getByText('Doc'));
    expect(queryByText('0 replies')).toBeNull();
    const span = getByText('Hello');
    fireEvent.mouseEnter(span);
    await waitFor(() => expect(getByText('0 replies')).toBeInTheDocument());
  });
});

