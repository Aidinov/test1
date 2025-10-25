import { Dialog, DialogTitle, DialogContent, DialogActions, Button, Typography } from '@mui/material';
interface Props {
  open: boolean;
  title: string;
  content?: string;
  onConfirm: () => void;
  onCancel: () => void;
}
export default function ConfirmDialog({ open, title, content, onConfirm, onCancel }: Props) {
  return (
    <Dialog open={open} onClose={onCancel} aria-labelledby="confirm-dialog-title">
      <DialogTitle id="confirm-dialog-title">{title}</DialogTitle>
      {content && (
        <DialogContent>
          <Typography>{content}</Typography>
        </DialogContent>
      )}
      <DialogActions>
        <Button onClick={onCancel}>Cancel</Button>
        <Button onClick={onConfirm} color="error">Confirm</Button>
      </DialogActions>
    </Dialog>
  );
}
