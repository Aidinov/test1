import { useState } from 'react';
import {
  Drawer,
  Box,
  Tabs,
  Tab,
  Typography,
  Card,
  CardContent,
  TextField,
  Button,
  Chip,
  Badge,
  Tooltip,
} from '@mui/material';
import { CommentResponse, CommentType } from '../types';
import SeverityChip from './SeverityChip';
import { useUserRole } from '../lib/UserRoleContext';

interface Props {
  open: boolean;
  onClose: () => void;
  comments: CommentResponse[];
  documentContent: string;
  documentVersion: string;
  onReply: (id: string, content: string) => void;
  onResolve: (c: CommentResponse) => void;
}

export default function CommentsPanel({
  open,
  onClose,
  comments,
  documentContent,
  documentVersion,
  onReply,
  onResolve,
}: Props) {
  const role = useUserRole();
  const [tab, setTab] = useState(0);
  const [inputs, setInputs] = useState<Record<string, string>>({});

  const isDangling = (c: CommentResponse) => {
    if (!c.originalText) return false;
    const snippet = documentContent.slice(c.startIndex, c.endIndex);
    return snippet !== c.originalText;
  };

  const active = comments.filter((c) => !isDangling(c));
  const dangling = comments.filter(isDangling);
  const questions = active.filter((c) => c.type === CommentType.Question);
  const remarks = active.filter((c) => c.type === CommentType.Remark);

  const handleSend = (id: string) => {
    const text = inputs[id];
    if (!text) return;
    onReply(id, text);
    setInputs((p) => ({ ...p, [id]: '' }));
  };

  const renderComment = (c: CommentResponse) => {
    const outdated =
      !!c.documentVersion &&
      c.documentVersion !== documentVersion &&
      !isDangling(c);
    return (
      <Card key={c.id} sx={{ mb: 2, opacity: c.isResolved ? 0.6 : 1 }}>
        <CardContent sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            {c.type === CommentType.Remark && (
              <SeverityChip severity={c.severity} />
            )}
            {c.isResolved && <Chip size="small" label="Resolved" />}
            {outdated && (
              <Chip
                size="small"
                color="warning"
                label={`Outdated since ${c.documentVersion!.slice(0, 7)}`}
              />
            )}
            {!c.isResolved && (role === 'Reviewer' || role === 'Approver') && (
              <Button
                size="small"
                onClick={() => onResolve(c)}
                data-testid="resolve-btn"
              >
                Resolve
              </Button>
            )}
            <Badge color="primary" badgeContent={c.replies.length} sx={{ ml: 'auto' }} />
          </Box>
          <Typography variant="body2">{c.content}</Typography>
          {c.replies.map((r) => (
            <Typography
              key={r.id}
              variant="body2"
              sx={{ ml: 2 }}
            >
              {r.content}
            </Typography>
          ))}
          <Box sx={{ display: 'flex', gap: 1 }}>
            <TextField
              size="small"
              fullWidth
              value={inputs[c.id] ?? ''}
              onChange={(e) =>
                setInputs((p) => ({ ...p, [c.id]: e.target.value }))
              }
              label="Reply"
            />
            <Button size="small" onClick={() => handleSend(c.id)}>
              Send
            </Button>
          </Box>
        </CardContent>
      </Card>
    );
  };

  return (
    <Drawer anchor="right" open={open} onClose={onClose} keepMounted>
      <Box sx={{ width: 360, p: 2 }} role="tabpanel">
        <Tabs value={tab} onChange={(_, v) => setTab(v)}>
          <Tab label={`Comments (${active.length})`} />
          <Tab label={`Dangling (${dangling.length})`} />
        </Tabs>
        <Box sx={{ mt: 2 }}>
          {tab === 0 ? (
            <Box>
              {questions.length > 0 && (
                <Typography variant="subtitle2" sx={{ mb: 1 }}>
                  Questions
                </Typography>
              )}
              {questions.map(renderComment)}
              {remarks.length > 0 && (
                <Typography variant="subtitle2" sx={{ mb: 1, mt: 2 }}>
                  Remarks
                </Typography>
              )}
              {remarks.map(renderComment)}
            </Box>
          ) : (
            <Box>
              {dangling.map((c) => (
                <Card key={c.id} sx={{ mb: 2 }}>
                  <CardContent>
                    <Tooltip title={c.originalText ?? ''}>
                      <Typography
                        variant="body2"
                        sx={{
                          fontFamily: 'monospace',
                          whiteSpace: 'nowrap',
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                        }}
                      >
                        {c.originalText}
                      </Typography>
                    </Tooltip>
                    <Chip
                      size="small"
                      variant="outlined"
                      label="Removed"
                      sx={{ mt: 1 }}
                    />
                    {c.documentVersion && (
                      <Typography variant="caption" sx={{ ml: 1 }}>
                        {c.documentVersion.slice(0, 7)}
                      </Typography>
                    )}
                  </CardContent>
                </Card>
              ))}
            </Box>
          )}
        </Box>
      </Box>
    </Drawer>
  );
}
