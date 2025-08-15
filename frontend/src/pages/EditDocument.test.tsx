import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { SnackbarProvider } from 'notistack';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import '@testing-library/jest-dom/vitest';
import EditDocument from './EditDocument';
import { getDocument, updateDocument } from '../api/documents';
import http from '../lib/http';
import { DocumentDetails, DocumentSummary } from '../types';
import { UserRoleProvider } from '../lib/UserRoleContext';

vi.mock('../api/documents');
vi.mock('../lib/http', () => ({
  default: { get: vi.fn(), put: vi.fn() }
}));

const mockGet = vi.mocked(getDocument);
const mockUpdate = vi.mocked(updateDocument);
const mockHttp = http as unknown as {
  get: ReturnType<typeof vi.fn> & { mockReset: () => void; mockResolvedValue: (...args: any[]) => void };
  put: ReturnType<typeof vi.fn>;
};

const baseDoc: DocumentDetails = {
  id: '1',
  title: 'Doc',
  product: 'Prod',
  team: 'Team',
  author: 'Alice',
  taskLink: 'link',
  gitRepository: '',
  gitFilePath: '',
  gitCommitHash: 'abc',
  status: 'Draft',
  createdAt: '',
  updatedAt: '',
  content: 'Hello **world**',
  comments: []
};

function renderEdit() {
  const client = new QueryClient({
    defaultOptions: {
      queries: { retry: false, refetchOnWindowFocus: false, refetchOnMount: false }
    }
  });
  return render(
    <QueryClientProvider client={client}>
      <SnackbarProvider>
        <UserRoleProvider role="Reviewer">
          <MemoryRouter initialEntries={['/documents/1/edit']}>
            <Routes>
              <Route path="/documents/:id/edit" element={<EditDocument />} />
            </Routes>
          </MemoryRouter>
        </UserRoleProvider>
      </SnackbarProvider>
    </QueryClientProvider>
  );
}

describe('EditDocument', () => {
  beforeEach(() => {
    mockGet.mockReset();
    mockUpdate.mockReset();
    mockHttp.get.mockReset();
  });

  it('loads content', async () => {
    mockGet.mockResolvedValue(baseDoc);
    mockHttp.get.mockResolvedValueOnce({ data: ['Prod'] });
    mockHttp.get.mockResolvedValueOnce({ data: ['Team'] });
    renderEdit();
    expect(await screen.findByDisplayValue('Doc')).toBeInTheDocument();
  });

  it('saves and shows new commit hash', async () => {
    mockGet.mockResolvedValue(baseDoc);
    mockHttp.get.mockResolvedValueOnce({ data: ['Prod'] });
    mockHttp.get.mockResolvedValueOnce({ data: ['Team'] });
    const updated: DocumentSummary = { ...baseDoc, gitCommitHash: 'def' };
    mockUpdate.mockResolvedValue(updated);
    renderEdit();
    await screen.findByDisplayValue('Doc');
    fireEvent.click(screen.getByText('Save'));
    await waitFor(() => expect(mockUpdate).toHaveBeenCalled());
    expect(await screen.findByTestId('commit-hash')).toHaveTextContent('def');
  });

  it('warns on unsaved changes', async () => {
    mockGet.mockResolvedValue(baseDoc);
    mockHttp.get.mockResolvedValueOnce({ data: ['Prod'] });
    mockHttp.get.mockResolvedValueOnce({ data: ['Team'] });
    renderEdit();
    const title = await screen.findByDisplayValue('Doc');
    fireEvent.change(title, { target: { value: 'New' } });
    const ev = new Event('beforeunload');
    Object.defineProperty(ev, 'preventDefault', { value: vi.fn() });
    window.dispatchEvent(ev);
    expect((ev as any).preventDefault).toHaveBeenCalled();
  });
});
