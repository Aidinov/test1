import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import '@testing-library/jest-dom/vitest';
import EditDocument from './EditDocument';
import { getDocument, updateDocument } from '../api/documents';
import http from '../lib/http';
import { DocumentDetails, DocumentSummary } from '../types';

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
  return render(
    <MemoryRouter initialEntries={['/documents/1/edit']}>
      <Routes>
        <Route path="/documents/:id/edit" element={<EditDocument />} />
      </Routes>
    </MemoryRouter>
  );
}

describe.skip('EditDocument', () => {
  beforeEach(() => {
    mockGet.mockReset();
    mockUpdate.mockReset();
    mockHttp.get.mockReset();
  });

  it('loads content', async () => {
    mockGet.mockResolvedValue(baseDoc);
    mockHttp.get.mockResolvedValue({ data: [] });
    renderEdit();
    expect(
      await screen.findByText((_, node) => node?.textContent === 'Hello **world**')
    ).toBeInTheDocument();
  });

  it('saves and shows new commit hash', async () => {
    mockGet.mockResolvedValue(baseDoc);
    mockHttp.get.mockResolvedValue({ data: [] });
    const updated: DocumentSummary = { ...baseDoc, gitCommitHash: 'def' };
    mockUpdate.mockResolvedValue(updated);
    renderEdit();
    await screen.findByText((_, node) => node?.textContent === 'Hello **world**');
    fireEvent.click(screen.getByText('Save'));
    await waitFor(() => expect(mockUpdate).toHaveBeenCalled());
    expect(await screen.findByTestId('commit-hash')).toHaveTextContent('def');
  });

  it('warns on unsaved changes', async () => {
    mockGet.mockResolvedValue(baseDoc);
    mockHttp.get.mockResolvedValue({ data: [] });
    renderEdit();
    await screen.findByText((_, node) => node?.textContent === 'Hello **world**');
    fireEvent.change(screen.getByLabelText('Title'), { target: { value: 'New' } });
    const ev = new Event('beforeunload');
    Object.defineProperty(ev, 'preventDefault', { value: vi.fn() });
    window.dispatchEvent(ev);
    expect((ev as any).preventDefault).toHaveBeenCalled();
  });
});
