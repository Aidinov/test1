import { useEffect, useRef, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import http from '../lib/http';
import { DEFAULT_IFRAME_WHITELIST } from '../lib/markdown';
import { getDocument, updateDocument } from '../api/documents';
import { addComment as apiAddComment, listComments, resolveComment as apiResolve } from '../api/comments';
import {
  DocumentDetails,
  DocumentStatus,
  UpdateDocumentRequest,
  CommentType,
  RemarkSeverity,
  CommentResponse,
} from '../types';
import {
  Box,
  Button,
  MenuItem,
  Select,
  TextField,
  Typography,
  Popover,
  SpeedDial,
  SpeedDialAction,
} from '@mui/material';
import { useTheme, alpha } from '@mui/material/styles';
import AddCommentIcon from '@mui/icons-material/AddComment';
import FormRow from '../ui/FormRow';
import MarkdownToolbar from '../ui/MarkdownToolbar';
import LoadingOverlay from '../ui/LoadingOverlay';
import ErrorState from '../ui/ErrorState';
import IframeWhitelistNotice from '../ui/IframeWhitelistNotice';
import { useSnackbar } from 'notistack';
import CommentDialog from '../ui/CommentDialog';
import SeverityChip from '../ui/SeverityChip';
import StatusChip from '../ui/StatusChip';
import CommentsPanel from '../ui/CommentsPanel';
import { useUserRole } from '../lib/UserRoleContext';
import { selectionToOffsets } from '../utils/selectionToOffsets';

interface OptionList {
  products: string[];
  teams: string[];
}

export default function EditDocument() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { enqueueSnackbar } = useSnackbar();
  const [doc, setDoc] = useState<DocumentDetails | null>(null);
  const [options, setOptions] = useState<OptionList>({ products: [], teams: [] });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [dirty, setDirty] = useState(false);
  const [selection, setSelection] =
    useState<{ start: number; end: number } | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null);
  const [hovered, setHovered] = useState<CommentResponse | null>(null);
  const theme = useTheme();
  const contentRef = useRef<HTMLDivElement>(null);
  const role = useUserRole();
  const [panelOpen, setPanelOpen] = useState(false);

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

  useUnsavedChangesGuard(dirty);

  if (loading) return <LoadingOverlay open />;
  if (error || !doc) return <ErrorState message={error ?? 'Document not found'} />;

  const handleSave = async () => {
    try {
      const req: UpdateDocumentRequest = {
        title: doc.title,
        product: doc.product,
        team: doc.team,
        author: doc.author,
        taskLink: doc.taskLink,
        content: doc.content,
        gitRepository: doc.gitRepository,
        gitFilePath: doc.gitFilePath,
        gitCommitHash: doc.gitCommitHash,
        status: doc.status,
      };
      const res = await updateDocument(doc.id, req);
      setDoc({ ...doc, ...res, comments: doc.comments });
      setDirty(false);
      enqueueSnackbar('Saved', { variant: 'success' });
    } catch (err) {
      enqueueSnackbar('Failed to save', { variant: 'error' });
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
      enqueueSnackbar('Sent for review', { variant: 'success' });
    } catch (err) {
      enqueueSnackbar('Failed to update status', { variant: 'error' });
    }
  };

  const handleMouseUp = () => {
    if (!doc || !contentRef.current) return;
    const sel = window.getSelection();
    if (!sel || sel.isCollapsed) {
      setSelection(null);
      return;
    }
    const range = sel.getRangeAt(0);
    const offsets = selectionToOffsets(doc.content, range, contentRef.current);
    if (!offsets) {
      setSelection(null);
      return;
    }
    setSelection({ start: offsets.startIndex, end: offsets.endIndex });
  };

  function renderHighlightedContent() {
    if (!doc) return null;
    const text = doc.content;
    if (!doc.comments || doc.comments.length === 0) {
      return <pre>{text}</pre>;
    }
    const sorted = [...doc.comments].sort(
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
    if (!doc || !selection) return;
    try {
      await apiAddComment(doc.id, {
        startIndex: selection.start,
        endIndex: selection.end,
        type: data.type,
        severity: data.severity,
        content: data.content,
        author: doc.author,
      });
      const updated = await listComments(doc.id);
      setDoc({ ...doc, comments: updated });
      setSelection(null);
      enqueueSnackbar('Comment added', { variant: 'success' });
    } catch {
      enqueueSnackbar('Failed to add comment', { variant: 'error' });
    }
  };

  const resolveComment = async (comment: CommentResponse) => {
    try {
      await apiResolve(comment.id, role);
      const updated = await listComments(doc.id);
      setDoc({ ...doc, comments: updated });
    } catch (err: any) {
      if (err?.response?.status === 403) {
        enqueueSnackbar('Not allowed to resolve', { variant: 'error' });
      } else {
        enqueueSnackbar('Failed to resolve comment', { variant: 'error' });
      }
    }
  };

  const addReply = (commentId: string, content: string) => {
    setDoc({
      ...doc,
      comments: doc.comments.map((c) =>
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
    });
  };

  return (
    <Box>
      <Typography variant="h4" sx={{ mb: 2 }}>
        Edit Document
      </Typography>
      <Box component="form" onSubmit={(e) => e.preventDefault()}>
        <FormRow label="Title">
          <TextField
            value={doc.title}
            onChange={(e) => {
              setDoc({ ...doc, title: e.target.value });
              setDirty(true);
            }}
          />
        </FormRow>
        <FormRow label="Product">
          <Select
            value={doc.product}
            onChange={(e) => {
              setDoc({ ...doc, product: e.target.value });
              setDirty(true);
            }}
            displayEmpty
          >
            <MenuItem value="">
              <em>Select a product</em>
            </MenuItem>
            {options.products.map((p) => (
              <MenuItem key={p} value={p}>{p}</MenuItem>
            ))}
          </Select>
        </FormRow>
        <FormRow label="Team">
          <Select
            value={doc.team}
            onChange={(e) => {
              setDoc({ ...doc, team: e.target.value });
              setDirty(true);
            }}
            displayEmpty
          >
            <MenuItem value="">
              <em>Select a team</em>
            </MenuItem>
            {options.teams.map((t) => (
              <MenuItem key={t} value={t}>{t}</MenuItem>
            ))}
          </Select>
        </FormRow>
        <FormRow label="Author">
          <TextField
            value={doc.author}
            onChange={(e) => {
              setDoc({ ...doc, author: e.target.value });
              setDirty(true);
            }}
          />
        </FormRow>
        <FormRow label="Task Link">
          <TextField
            value={doc.taskLink}
            onChange={(e) => {
              setDoc({ ...doc, taskLink: e.target.value });
              setDirty(true);
            }}
          />
        </FormRow>
        <FormRow label="Git Repository URL">
          <TextField
            value={doc.gitRepository}
            onChange={(e) => {
              setDoc({ ...doc, gitRepository: e.target.value });
              setDirty(true);
            }}
          />
        </FormRow>
        <FormRow label="Git File Path">
          <TextField
            value={doc.gitFilePath}
            onChange={(e) => {
              setDoc({ ...doc, gitFilePath: e.target.value });
              setDirty(true);
            }}
          />
        </FormRow>
        <FormRow label="Status">
          <Select
            value={doc.status}
            onChange={(e) => {
              setDoc({ ...doc, status: e.target.value as DocumentStatus });
              setDirty(true);
            }}
          >
            <MenuItem value="Draft">Draft</MenuItem>
            <MenuItem value="UnderReview">UnderReview</MenuItem>
            <MenuItem value="Approved">Approved</MenuItem>
            <MenuItem value="Rejected">Rejected</MenuItem>
          </Select>
        </FormRow>
        <FormRow label="Content">
          <Box sx={{ flex: 1 }}>
            <MarkdownToolbar onInsert={(s) => {
              setDoc({ ...doc, content: doc.content + s });
              setDirty(true);
            }} />
            <TextField
              multiline
              minRows={10}
              fullWidth
              value={doc.content}
              onChange={(e) => {
                setDoc({ ...doc, content: e.target.value });
                setDirty(true);
              }}
            />
          </Box>
        </FormRow>
        <Box sx={{ mb: 2 }}>
          <Typography variant="h6">Preview</Typography>
          <Button onClick={() => setPanelOpen(true)}>Comments</Button>
          <IframeWhitelistNotice hosts={DEFAULT_IFRAME_WHITELIST} />
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
        </Box>
        <Typography sx={{ mb: 2 }}>
          Commit: <span data-testid="commit-hash">{doc.gitCommitHash}</span>
        </Typography>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <Button onClick={handleSave}>Save</Button>
          <Button onClick={handleSendForReview} disabled={doc.status !== 'Draft'}>
            Send for review
          </Button>
          <Button onClick={() => navigate(`/documents/${doc.id}`)}>View</Button>
        </Box>
      </Box>
      <CommentsPanel
        open={panelOpen}
        onClose={() => setPanelOpen(false)}
        comments={doc.comments}
        documentContent={doc.content}
        documentVersion={doc.gitCommitHash}
        onReply={addReply}
        onResolve={resolveComment}
      />
    </Box>
  );
}

function useUnsavedChangesGuard(when: boolean) {
  useEffect(() => {
    const handler = (e: BeforeUnloadEvent) => {
      if (!when) return;
      e.preventDefault();
      e.returnValue = '';
    };
    window.addEventListener('beforeunload', handler);
    return () => window.removeEventListener('beforeunload', handler);
  }, [when]);
}