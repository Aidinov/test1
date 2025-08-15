import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import http from '../lib/http';
import Markdown, { DEFAULT_IFRAME_WHITELIST } from '../lib/markdown';
import { getDocument, updateDocument } from '../api/documents';
import { DocumentDetails, DocumentStatus, UpdateDocumentRequest } from '../types';
import {
  Box,
  Button,
  MenuItem,
  Select,
  TextField,
  Typography,
} from '@mui/material';
import FormRow from '../ui/FormRow';
import MarkdownToolbar from '../ui/MarkdownToolbar';
import LoadingOverlay from '../ui/LoadingOverlay';
import ErrorState from '../ui/ErrorState';
import IframeWhitelistNotice from '../ui/IframeWhitelistNotice';
import { useSnackbar } from 'notistack';

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
          <IframeWhitelistNotice hosts={DEFAULT_IFRAME_WHITELIST} />
          <Markdown markdown={doc.content} />
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