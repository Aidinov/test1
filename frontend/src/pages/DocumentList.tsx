import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import axios from 'axios';

export interface DocumentSummary {
  id: string;
  title: string;
  product: string;
  team: string;
  author: string;
  status: string;
  updatedAt: string;
}

export default function DocumentList() {
  const [documents, setDocuments] = useState<DocumentSummary[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchDocs() {
      try {
        const res = await axios.get<DesignDocument[]>('/api/documents');
        const data = res.data.map((doc: DesignDocument) => ({
          id: doc.id,
          title: doc.title,
          product: doc.product,
          team: doc.team,
          author: doc.author,
          status: doc.status,
          updatedAt: doc.updatedAt,
        }));
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

// Type representing the server‑side document; used for casting response.
export interface DesignDocument {
  id: string;
  title: string;
  product: string;
  team: string;
  author: string;
  taskLink: string;
  content: string;
  status: string;
  createdAt: string;
  updatedAt: string;
  gitRepository: string;
  gitFilePath: string;
  gitCommitHash: string;
}