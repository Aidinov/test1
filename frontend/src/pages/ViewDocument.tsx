import { useEffect, useState, useRef } from 'react';
import { useParams } from 'react-router-dom';
import { getDocument } from '../api/documents';
import { addComment as apiAddComment, listComments, resolveComment as apiResolve } from '../api/comments';
import { RemarkSeverity, CommentType, CommentResponse, DocumentDetails } from '../types';
import { Box, Popover, SpeedDial, SpeedDialAction, Typography, Button } from '@mui/material';
import { useTheme, alpha } from '@mui/material/styles';
import AddCommentIcon from '@mui/icons-material/AddComment';
import { useSnackbar } from 'notistack';
import SeverityChip from '../ui/SeverityChip';
import StatusChip from '../ui/StatusChip';
import CommentDialog from '../ui/CommentDialog';
import CommentsPanel from '../ui/CommentsPanel';
import { useUserRole } from '../lib/UserRoleContext';
import { selectionToOffsets } from '../utils/selectionToOffsets';

interface ViewState {
  doc: DocumentDetails | null;
  comments: CommentResponse[];
}

export default function ViewDocument() {
  const { id } = useParams();
  const [state, setState] = useState<ViewState>({ doc: null, comments: [] });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selection, setSelection] =
    useState<{ start: number; end: number } | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null);
  const [hovered, setHovered] = useState<CommentResponse | null>(null);
  const { enqueueSnackbar } = useSnackbar();
  const theme = useTheme();
  const role = useUserRole();
  const [panelOpen, setPanelOpen] = useState(false);

  const contentRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    async function load() {
      try {
        const doc = await getDocument(id!);
        setState({ doc, comments: doc.comments ?? [] });
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
    if (!state.doc || !contentRef.current) return;
    const sel = window.getSelection();
    if (!sel || sel.isCollapsed) {
      setSelection(null);
      return;
    }
    const range = sel.getRangeAt(0);
    const offsets = selectionToOffsets(
      state.doc.content,
      range,
      contentRef.current
    );
    if (!offsets) {
      setSelection(null);
      return;
    }
    setSelection({ start: offsets.startIndex, end: offsets.endIndex });
  };

  // Highlight logic: build spans around commented ranges
  function renderHighlightedContent() {
    if (!state.doc) return null;
    const text = state.doc.content;
    if (state.comments.length === 0) {
      return <pre>{text}</pre>;
    }
    const sorted = [...state.comments].sort(
      (a, b) => a.startIndex - b.startIndex
    );
    const elements: JSX.Element[] = [];
    let pointer = 0;
    sorted.forEach((comment) => {
      if (comment.startIndex > text.length) return;
      if (comment.startIndex > pointer) {
        const substr = text.slice(pointer, comment.startIndex);
        elements.push(<span key={`text-${pointer}`}>{substr}</span>);
        pointer = comment.startIndex;
      }
      const end = Math.min(comment.endIndex, text.length);
      if (end > pointer) {
        const substr = text.slice(pointer, end);
        const color = highlightColor(comment);
        elements.push(
          <Box
            component="span"
            key={`comment-${comment.id}`}
            tabIndex={0}
            onMouseEnter={(e) => {
              setAnchorEl(e.currentTarget);
              setHovered(comment);
            }}
            onMouseLeave={() => {
              setAnchorEl(null);
              setHovered(null);
            }}
            sx={{
              backgroundColor: color,
              borderRadius: 1,
            }}
          >
            {substr}
          </Box>
        );
        pointer = end;
      }
    });
    if (pointer < text.length) {
      elements.push(<span key={`tail-${pointer}`}>{text.slice(pointer)}</span>);
    }
    return <pre>{elements}</pre>;
  }

  function highlightColor(comment: CommentResponse) {
    const base = comment.isResolved
      ? theme.palette.success.light
      : comment.type === CommentType.Question
      ? theme.palette.info.light
      : comment.severity === RemarkSeverity.Critical
      ? theme.palette.error.light
      : comment.severity === RemarkSeverity.Desirable
      ? theme.palette.warning.light
      : theme.palette.info.light;
    return alpha(base, 0.3);
  }

  const handleAddComment = async (data: {
    type: CommentType;
    severity: RemarkSeverity;
    content: string;
  }) => {
    if (!id || !selection) return;
    try {
      await apiAddComment(id, {
        startIndex: selection.start,
        endIndex: selection.end,
        type: data.type,
        severity: data.severity,
        content: data.content,
        author: 'Reviewer',
      });
      const res = await listComments(id);
      setState((prev) => ({ ...prev, comments: res }));
      setSelection(null);
      enqueueSnackbar('Comment added', { variant: 'success' });
    } catch {
      enqueueSnackbar('Failed to add comment', { variant: 'error' });
    }
  };

  const resolveComment = async (comment: CommentResponse) => {
    try {
      await apiResolve(comment.id, role);
      const res = await listComments(id!);
      setState((prev) => ({ ...prev, comments: res }));
    } catch (err: any) {
      if (err?.response?.status === 403) {
        enqueueSnackbar('Not allowed to resolve', { variant: 'error' });
      } else {
        enqueueSnackbar('Failed to resolve comment', { variant: 'error' });
      }
    }
  };

  const addReply = (commentId: string, content: string) => {
    setState((prev) => ({
      ...prev,
      comments: prev.comments.map((c) =>
        c.id === commentId
          ? {
              ...c,
              replies: [
                ...c.replies,
                {
                  id: Math.random().toString(),
                  author: role,
                  content,
                  createdAt: new Date().toISOString(),
                },
              ],
            }
          : c,
      ),
    }));
  };

  if (loading) return <div>Loading...</div>;
  if (error || !state.doc) return <div>{error ?? 'Document not found'}</div>;

  return (
    <div style={{ display: 'flex', gap: '1rem' }}>
      <div style={{ flex: 1 }}>
        <h2>{state.doc.title}</h2>
        <Button onClick={() => setPanelOpen(true)}>Comments</Button>
        <Box
          ref={contentRef}
          onMouseUp={handleMouseUp}
          sx={{ whiteSpace: 'pre-wrap', position: 'relative' }}
        >
          {renderHighlightedContent()}
        </Box>
        {selection && (
          <SpeedDial
            ariaLabel="add comment"
            open
            sx={{ position: 'fixed', bottom: 16, right: 16 }}
            icon={<AddCommentIcon />}
          >
            <SpeedDialAction
              icon={<AddCommentIcon />}
              tooltipTitle="Add comment"
              onClick={() => setDialogOpen(true)}
            />
          </SpeedDial>
        )}
        <CommentDialog
          open={dialogOpen}
          onClose={() => setDialogOpen(false)}
          onSubmit={handleAddComment}
        />
        <Popover
          open={Boolean(anchorEl)}
          anchorEl={anchorEl}
          onClose={() => {
            setAnchorEl(null);
            setHovered(null);
          }}
          anchorOrigin={{ vertical: 'bottom', horizontal: 'left' }}
        >
          {hovered && (
            <Box sx={{ p: 1, display: 'flex', gap: 1, alignItems: 'center' }}>
              <SeverityChip severity={hovered.severity} />
              <StatusChip status={hovered.isResolved ? 'Approved' : 'Draft'} />
              <Typography variant="body2">0 replies</Typography>
            </Box>
          )}
        </Popover>
        <CommentsPanel
          open={panelOpen}
          onClose={() => setPanelOpen(false)}
          comments={state.comments}
          documentContent={state.doc.content}
          documentVersion={state.doc.gitCommitHash}
          onReply={addReply}
          onResolve={resolveComment}
        />
      </div>
    </div>
  );
}