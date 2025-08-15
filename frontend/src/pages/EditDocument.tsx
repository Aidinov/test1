import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import http from '../lib/http';
import { getDocument, updateDocument } from '../api/documents';
import { DocumentDetails } from '../types';

interface OptionList {
  products: string[];
  teams: string[];
}

export default function EditDocument() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [doc, setDoc] = useState<DocumentDetails | null>(null);
  const [options, setOptions] = useState<OptionList>({ products: [], teams: [] });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function load() {
      try {
        const [docRes, productsRes, teamsRes] = await Promise.all([
          getDocument(id!),
          http.get<string[]>('/products'),
          http.get<string[]>('/teams'),
        ]);
        setDoc(docRes);
        setOptions({ products: productsRes.data, teams: teamsRes.data });
      } catch (err) {
        setError('Failed to load document');
      } finally {
        setLoading(false);
      }
    }
    if (id) load();
  }, [id]);

  if (loading) return <div>Loading...</div>;
  if (error || !doc) return <div>{error ?? 'Document not found'}</div>;

  const handleSave = async () => {
    try {
      await updateDocument(doc.id, doc);
      alert('Saved');
    } catch (err) {
      alert('Failed to save');
    }
  };

  const handleSendForReview = async () => {
    try {
      await http.put(
        `/documents/${doc.id}/status`,
        null,
        { params: { status: 'UnderReview' } }
      );
      setDoc({ ...doc, status: 'UnderReview' });
      alert('Sent for review');
    } catch (err) {
      alert('Failed to update status');
    }
  };

  return (
    <div>
      <h2>Edit Document</h2>
      <form className="metadata-form" onSubmit={(e) => e.preventDefault()}>
        <label>
          Title
          <input
            type="text"
            value={doc.title}
            onChange={(e) => setDoc({ ...doc, title: e.target.value })}
          />
        </label>
        <label>
          Product
          <select
            value={doc.product}
            onChange={(e) => setDoc({ ...doc, product: e.target.value })}
          >
            <option value="">Select a product</option>
            {options.products.map((p) => (
              <option key={p} value={p}>{p}</option>
            ))}
          </select>
        </label>
        <label>
          Team
          <select
            value={doc.team}
            onChange={(e) => setDoc({ ...doc, team: e.target.value })}
          >
            <option value="">Select a team</option>
            {options.teams.map((t) => (
              <option key={t} value={t}>{t}</option>
            ))}
          </select>
        </label>
        <label>
          Author
          <input
            type="text"
            value={doc.author}
            onChange={(e) => setDoc({ ...doc, author: e.target.value })}
          />
        </label>
        <label>
          Task Link
          <input
            type="text"
            value={doc.taskLink}
            onChange={(e) => setDoc({ ...doc, taskLink: e.target.value })}
          />
        </label>
        <label>
          Git Repository URL
          <input
            type="text"
            value={doc.gitRepository}
            onChange={(e) => setDoc({ ...doc, gitRepository: e.target.value })}
          />
        </label>
        <label>
          Git File Path
          <input
            type="text"
            value={doc.gitFilePath}
            onChange={(e) => setDoc({ ...doc, gitFilePath: e.target.value })}
          />
        </label>
        <label>
          Content
          <textarea
            className="editor-textarea"
            value={doc.content}
            onChange={(e) => setDoc({ ...doc, content: e.target.value })}
          />
        </label>
        <div>
          <button className="button" onClick={handleSave}>Save</button>
          <button
            className="button"
            onClick={handleSendForReview}
            disabled={doc.status !== 'Draft'}
          >
            Send for review
          </button>
          <button
            className="button"
            onClick={() => navigate(`/documents/${doc.id}`)}
          >
            View
          </button>
        </div>
      </form>
    </div>
  );
}