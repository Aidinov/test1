import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { listDocuments } from '../api/documents';
import { DocumentStatus, DocumentSummary } from '../types';

export default function DocumentList() {
  const navigate = useNavigate();
  const [documents, setDocuments] = useState<DocumentSummary[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [team, setTeam] = useState('');
  const [product, setProduct] = useState('');
  const [author, setAuthor] = useState('');
  const [status, setStatus] = useState<DocumentStatus | ''>('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  async function fetchDocs(overrides: Partial<{ page: number }> = {}) {
    setLoading(true);
    try {
      const data = await listDocuments({
        team: team || undefined,
        product: product || undefined,
        author: author || undefined,
        status: status || undefined,
        page: overrides.page ?? page,
        pageSize
      });
      setDocuments(data);
      setError(null);
    } catch {
      setError('Failed to load documents');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    fetchDocs();
  }, [page, pageSize]);

  function applyFilters() {
    setPage(1);
    fetchDocs({ page: 1 });
  }

  if (loading) return <div>Loading...</div>;
  if (error) return <div>{error}</div>;

  return (
    <div>
      <h2>Documents</h2>
      <div>
        <input
          placeholder="Team"
          value={team}
          onChange={(e) => setTeam(e.target.value)}
        />
        <input
          placeholder="Product"
          value={product}
          onChange={(e) => setProduct(e.target.value)}
        />
        <input
          placeholder="Author"
          value={author}
          onChange={(e) => setAuthor(e.target.value)}
        />
        <select
          aria-label="Status"
          value={status}
          onChange={(e) => setStatus(e.target.value as DocumentStatus | '')}
        >
          <option value="">All</option>
          <option value="Draft">Draft</option>
          <option value="UnderReview">UnderReview</option>
          <option value="Approved">Approved</option>
          <option value="Rejected">Rejected</option>
        </select>
        <button onClick={applyFilters}>Apply</button>
      </div>
      <div>
        <button
          onClick={() => setPage((p) => Math.max(1, p - 1))}
          disabled={page === 1}
        >
          Prev
        </button>
        <span>Page {page}</span>
        <button onClick={() => setPage((p) => p + 1)}>Next</button>
        <select
          aria-label="Page Size"
          value={pageSize}
          onChange={(e) => {
            setPageSize(Number(e.target.value));
            setPage(1);
          }}
        >
          <option value={5}>5</option>
          <option value={10}>10</option>
          <option value={20}>20</option>
        </select>
      </div>
      {documents.length === 0 ? (
        <p>No documents found.</p>
      ) : (
        <table className="table">
          <thead>
            <tr>
              <th>Title</th>
              <th>Product</th>
              <th>Team</th>
              <th>Author</th>
              <th>Status</th>
              <th>UpdatedAt</th>
            </tr>
          </thead>
          <tbody>
            {documents.map((doc) => (
              <tr
                key={doc.id}
                onClick={() => navigate(`/documents/${doc.id}`)}
                style={{ cursor: 'pointer' }}
              >
                <td>{doc.title}</td>
                <td>{doc.product}</td>
                <td>{doc.team}</td>
                <td>{doc.author}</td>
                <td>{doc.status}</td>
                <td>{new Date(doc.updatedAt).toLocaleString()}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
