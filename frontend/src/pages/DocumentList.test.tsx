import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import '@testing-library/jest-dom/vitest';
import DocumentList from './DocumentList';
import { listDocuments } from '../api/documents';
import { DocumentSummary } from '../types';

vi.mock('../api/documents');
const mockList = vi.mocked(listDocuments);

function renderList() {
  return render(<DocumentList />, { wrapper: MemoryRouter });
}

describe('DocumentList', () => {
  beforeEach(() => {
    mockList.mockReset();
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
    fireEvent.change(screen.getByPlaceholderText('Team'), { target: { value: 'T1' } });
    fireEvent.change(screen.getByPlaceholderText('Product'), { target: { value: 'P1' } });
    fireEvent.change(screen.getByPlaceholderText('Author'), { target: { value: 'A1' } });
    fireEvent.change(screen.getByLabelText('Status'), { target: { value: 'Draft' } });
    fireEvent.click(screen.getByText('Apply'));
    await waitFor(() => expect(mockList).toHaveBeenCalledTimes(1));
    expect(mockList).toHaveBeenCalledWith({
      team: 'T1',
      product: 'P1',
      author: 'A1',
      status: 'Draft',
      page: 1,
      pageSize: 10
    });
    fireEvent.change(screen.getByLabelText('Page Size'), { target: { value: '20' } });
    await waitFor(() => expect(mockList).toHaveBeenCalledTimes(2));
    expect(mockList).toHaveBeenLastCalledWith({
      team: 'T1',
      product: 'P1',
      author: 'A1',
      status: 'Draft',
      page: 1,
      pageSize: 20
    });
    fireEvent.click(screen.getByText('Next'));
    await waitFor(() => expect(mockList).toHaveBeenCalledTimes(3));
    expect(mockList).toHaveBeenLastCalledWith({
      team: 'T1',
      product: 'P1',
      author: 'A1',
      status: 'Draft',
      page: 2,
      pageSize: 20
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
