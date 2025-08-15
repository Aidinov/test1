import { useState } from 'react';
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  MenuItem,
  Select,
  Tab,
  Tabs,
  TextField,
} from '@mui/material';
import { CommentType, RemarkSeverity } from '../types';

interface Props {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: { type: CommentType; severity: RemarkSeverity; content: string }) => void;
}

export default function CommentDialog({ open, onClose, onSubmit }: Props) {
  const [type, setType] = useState<CommentType>(CommentType.Question);
  const [severity, setSeverity] = useState<RemarkSeverity>(RemarkSeverity.Opinion);
  const [content, setContent] = useState('');
  const [touched, setTouched] = useState(false);

  const handleSave = () => {
    if (!content.trim()) {
      setTouched(true);
      return;
    }
    onSubmit({ type, severity, content: content.trim() });
    setContent('');
    setTouched(false);
    onClose();
  };

  const handleClose = () => {
    setContent('');
    setTouched(false);
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} aria-labelledby="comment-dialog-title">
      <DialogTitle id="comment-dialog-title">Add Comment</DialogTitle>
      <DialogContent sx={{ pt: 2, display: 'flex', flexDirection: 'column', gap: 2 }}>
        <Tabs value={type} onChange={(_, v) => setType(v)} aria-label="comment type">
          <Tab label="Question" value={CommentType.Question} />
          <Tab label="Remark" value={CommentType.Remark} />
        </Tabs>
        <Select value={severity} onChange={(e) => setSeverity(e.target.value as RemarkSeverity)}>
          {Object.values(RemarkSeverity).map((s) => (
            <MenuItem key={s} value={s}>
              {s}
            </MenuItem>
          ))}
        </Select>
        <TextField
          multiline
          minRows={3}
          label="Comment"
          value={content}
          onChange={(e) => setContent(e.target.value)}
          error={touched && !content.trim()}
          helperText={touched && !content.trim() ? 'Comment is required' : ' '}
        />
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose}>Cancel</Button>
        <Button onClick={handleSave}>Save</Button>
      </DialogActions>
    </Dialog>
  );
}
