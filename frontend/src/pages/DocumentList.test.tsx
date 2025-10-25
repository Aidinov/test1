import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import '@testing-library/jest-dom/vitest';
import DocumentList from './DocumentList';
import { listDocuments } from '../api/documents';
import { DocumentSummary } from '../types';

vi.mock('../api/documents');
const mockList = vi.mocked(listDocuments);

function renderList() {
  const client = new QueryClient({
    defaultOptions: {
      queries: { retry: false, refetchOnWindowFocus: false, refetchOnMount: false }
    }
  });
  return render(
    <QueryClientProvider client={client}>
      <MemoryRouter>
        <DocumentList />
      </MemoryRouter>
    </QueryClientProvider>
  );
}

describe('DocumentList', () => {
  beforeEach(() => {
    mockList.mockReset();
  });

  it('fetches once on mount', async () => {
    mockList.mockResolvedValue([]);
    renderList();
    await waitFor(() => expect(mockList).toHaveBeenCalledTimes(1));
  });

  it('does not request content field', async () => {
    mockList.mockResolvedValue([]);
    renderList();
    await waitFor(() => expect(mockList).toHaveBeenCalled());
    expect(mockList.mock.calls[0][0]).not.toHaveProperty('content');
  });

  it('builds query from filters and pagination', async () => {
    mockList.mockResolvedValue([]);
    renderList();
    await screen.findByText('No documents found.');
    mockList.mockClear();
    fireEvent.change(screen.getAllByLabelText('Team')[0], { target: { value: 'T1' } });
    fireEvent.change(screen.getAllByLabelText('Product')[0], { target: { value: 'P1' } });
    fireEvent.change(screen.getAllByLabelText('Author')[0], { target: { value: 'A1' } });
    fireEvent.click(screen.getByText('Apply'));
    await waitFor(() => expect(mockList).toHaveBeenCalledTimes(1));
    await screen.findByText('No documents found.');
    expect(mockList).toHaveBeenCalledWith({
      team: 'T1',
      product: 'P1',
      author: 'A1',
      status: undefined,
      page: 1,
      pageSize: 10
    });
    fireEvent.click(screen.getByText('Next'));
    await waitFor(() => expect(mockList).toHaveBeenCalledTimes(2));
    expect(mockList).toHaveBeenLastCalledWith({
      team: 'T1',
      product: 'P1',
      author: 'A1',
      status: undefined,
      page: 2,
      pageSize: 10
    });
  });

  it('renders rows', async () => {
    const docs: DocumentSummary[] = [
      {
        id: '1',
        title: 'Doc1',
        product: 'Prod',
        team: 'Team',
        author: 'Alice',
        taskLink: '',
        gitRepository: '',
        gitFilePath: '',
        gitCommitHash: '',
        status: 'Draft',
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      }
    ];
    mockList.mockResolvedValue(docs);
    renderList();
    expect(await screen.findByText('Doc1')).toBeInTheDocument();
  });

  it('shows empty state', async () => {
    mockList.mockResolvedValue([]);
    renderList();
    expect(await screen.findByText('No documents found.')).toBeInTheDocument();
  });
});
