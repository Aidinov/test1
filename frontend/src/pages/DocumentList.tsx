import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { listDocuments } from '../api/documents';
import { DocumentSummary } from '../types';

export default function DocumentList() {
  const [documents, setDocuments] = useState<DocumentSummary[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchDocs() {
      try {
        const data = await listDocuments();
        setDocuments(data);
      } catch (err) {
        setError('Failed to load documents');
      } finally {
        setLoading(false);
      }
    }
    fetchDocs();
  }, []);

  if (loading) return <div>Loading...</div>;
  if (error) return <div>{error}</div>;

  return (
    <div>
      <h2>Documents</h2>
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
              <th>Updated</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {documents.map((doc) => (
              <tr key={doc.id}>
                <td>{doc.title}</td>
                <td>{doc.product}</td>
                <td>{doc.team}</td>
                <td>{doc.author}</td>
                <td>{doc.status}</td>
                <td>{new Date(doc.updatedAt).toLocaleString()}</td>
                <td>
                  <Link to={`/documents/${doc.id}`}>View</Link>
                  {' | '}
                  <Link to={`/edit/${doc.id}`}>Edit</Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
