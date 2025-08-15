import { useEffect, useState, useRef } from 'react';
import { useParams } from 'react-router-dom';
import axios from 'axios';
import { DesignDocument } from './DocumentList';
import { RemarkSeverity, CommentType, Comment } from '../types';

interface ViewState {
  doc: DesignDocument | null;
  comments: Comment[];
}

export default function ViewDocument() {
  const { id } = useParams();
  const [state, setState] = useState<ViewState>({ doc: null, comments: [] });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selection, setSelection] = useState<{ start: number; end: number } | null>(null);
  const [commentFormVisible, setCommentFormVisible] = useState(false);
  const [formData, setFormData] = useState<{
    type: CommentType;
    severity: RemarkSeverity;
    content: string;
  }>({ type: CommentType.Question, severity: RemarkSeverity.Opinion, content: '' });

  const contentRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    async function load() {
      try {
        const docRes = await axios.get<DesignDocument>(`/api/documents/${id}`);
        const commentsRes = await axios.get<Comment[]>(`/api/documents/${id}/comments`);
        setState({ doc: docRes.data, comments: commentsRes.data });
      } catch (err) {
        setError('Failed to load document or comments');
      } finally {
        setLoading(false);
      }
    }
    if (id) load();
  }, [id]);

  // Handler to track text selection and compute start/end indices
  const handleMouseUp = () => {
    if (!state.doc) return;
    const selectionObj = window.getSelection();
    if (!selectionObj || selectionObj.isCollapsed) {
      setSelection(null);
      return;
    }
    const selectedText = selectionObj.toString();
    if (!selectedText) {
      setSelection(null);
      return;
    }
    // Compute the first occurrence of selected text within the content
    const contentString = state.doc.content;
    const startIdx = contentString.indexOf(selectedText);
    if (startIdx === -1) {
      setSelection(null);
      return;
    }
    const endIdx = startIdx + selectedText.length;
    setSelection({ start: startIdx, end: endIdx });
  };

  // Highlight logic: build spans around commented ranges
  function renderHighlightedContent() {
    if (!state.doc) return null;
    const text = state.doc.content;
    if (state.comments.length === 0) {
      return <pre>{text}</pre>;
    }
    // Sort comments by start index
    const sorted = [...state.comments].sort((a, b) => a.startIndex - b.startIndex);
    const elements: JSX.Element[] = [];
    let pointer = 0;
    sorted.forEach((comment, idx) => {
      if (comment.startIndex > text.length) return; // skip invalid
      // Add text before comment
      if (comment.startIndex > pointer) {
        const substr = text.slice(pointer, comment.startIndex);
        elements.push(<span key={`text-${pointer}`}>{substr}</span>);
        pointer = comment.startIndex;
      }
      // Add commented section
      const end = Math.min(comment.endIndex, text.length);
      if (end > pointer) {
        const substr = text.slice(pointer, end);
        const className = getCommentClass(comment);
        elements.push(
          <span
            key={`comment-${comment.id}`}
            className={className}
            onClick={() => scrollToComment(comment.id)}
          >
            {substr}
          </span>
        );
        pointer = end;
      }
    });
    // Add remaining text
    if (pointer < text.length) {
      elements.push(<span key={`tail-${pointer}`}>{text.slice(pointer)}</span>);
    }
    return <pre>{elements}</pre>;
  }

  function getCommentClass(comment: Comment) {
    if (comment.isResolved) return 'comment-resolved';
    if (comment.type === CommentType.Question) return 'comment-question';
    switch (comment.severity) {
      case RemarkSeverity.Critical:
        return 'comment-critical';
      case RemarkSeverity.Desirable:
        return 'comment-desirable';
      case RemarkSeverity.Opinion:
      default:
        return 'comment-opinion';
    }
  }

  function scrollToComment(commentId: string) {
    const el = document.getElementById(`comment-${commentId}`);
    if (el) {
      el.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
  }

  const addComment = async () => {
    if (!id || !selection) return;
    try {
      const payload = {
        startIndex: selection.start,
        endIndex: selection.end,
        type: formData.type,
        severity: formData.severity,
        content: formData.content,
        author: 'Reviewer'
      };
      await axios.post(`/api/documents/${id}/comments`, payload);
      // Reload comments
      const res = await axios.get<Comment[]>(`/api/documents/${id}/comments`);
      setState((prev) => ({ ...prev, comments: res.data }));
      // Reset state
      setCommentFormVisible(false);
      setSelection(null);
      setFormData({ type: CommentType.Question, severity: RemarkSeverity.Opinion, content: '' });
    } catch (err) {
      alert('Failed to add comment');
    }
  };

  const resolveComment = async (comment: Comment) => {
    if (!id) return;
    try {
      await axios.post(`/api/documents/${id}/comments/${comment.id}/resolve`, null, {
        params: { resolvedBy: 'Reviewer' }
      });
      // Reload comments
      const res = await axios.get<Comment[]>(`/api/documents/${id}/comments`);
      setState((prev) => ({ ...prev, comments: res.data }));
    } catch (err) {
      alert('Failed to resolve comment');
    }
  };

  if (loading) return <div>Loading...</div>;
  if (error || !state.doc) return <div>{error ?? 'Document not found'}</div>;

  return (
    <div style={{ display: 'flex', gap: '1rem' }}>
      <div style={{ flex: 1 }}>
        <h2>{state.doc.title}</h2>
        <div
          ref={contentRef}
          onMouseUp={handleMouseUp}
          style={{ whiteSpace: 'pre-wrap', position: 'relative' }}
        >
          {renderHighlightedContent()}
        </div>
        {selection && (
          <div style={{ marginTop: '1rem' }}>
            <button className="button" onClick={() => setCommentFormVisible(!commentFormVisible)}>
              Add Comment
            </button>
            <span style={{ marginLeft: '0.5rem' }}>
              Selected {selection.end - selection.start} chars
            </span>
          </div>
        )}
        {commentFormVisible && selection && (
          <div style={{ marginTop: '1rem', border: '1px solid #ddd', padding: '1rem' }}>
            <h3>New Comment</h3>
            <div>
              <label>
                Type
                <select
                  value={formData.type}
                  onChange={(e) =>
                    setFormData({ ...formData, type: e.target.value as CommentType })
                  }
                >
                  <option value={CommentType.Question}>Question</option>
                  <option value={CommentType.Remark}>Remark</option>
                </select>
              </label>
            </div>
            {formData.type === CommentType.Remark && (
              <div>
                <label>
                  Severity
                  <select
                    value={formData.severity}
                    onChange={(e) =>
                      setFormData({ ...formData, severity: e.target.value as RemarkSeverity })
                    }
                  >
                    <option value={RemarkSeverity.Critical}>Critical</option>
                    <option value={RemarkSeverity.Desirable}>Desirable</option>
                    <option value={RemarkSeverity.Opinion}>Opinion</option>
                  </select>
                </label>
              </div>
            )}
            <div>
              <label>
                Content
                <textarea
                  className="editor-textarea"
                  value={formData.content}
                  onChange={(e) => setFormData({ ...formData, content: e.target.value })}
                />
              </label>
            </div>
            <div>
              <button className="button" onClick={addComment}>Save Comment</button>
              <button
                className="button"
                onClick={() => setCommentFormVisible(false)}
                style={{ backgroundColor: '#ccc', color: '#333' }}
              >
                Cancel
              </button>
            </div>
          </div>
        )}
      </div>
      <div className="comment-panel">
        <h3>Comments</h3>
        {state.comments.length === 0 && <p>No comments yet.</p>}
        {state.comments.map((c) => (
          <div
            key={c.id}
            id={`comment-${c.id}`}
            className={`comment ${c.isResolved ? 'resolved' : ''}`}
          >
            <p>
              <strong>{c.type === CommentType.Question ? 'Question' : 'Remark'}</strong>
              {c.type === CommentType.Remark && ` • ${c.severity}`}
            </p>
            <p>{c.content}</p>
            <p style={{ fontSize: '0.8rem', color: '#666' }}>By {c.author}</p>
            {!c.isResolved && (
              <button className="button" onClick={() => resolveComment(c)}>
                Resolve
              </button>
            )}
            {c.isResolved && (
              <p style={{ fontSize: '0.8rem', color: '#666' }}>
                Resolved by {c.resolvedBy}
              </p>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}