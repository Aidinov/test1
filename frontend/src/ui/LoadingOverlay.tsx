import { Backdrop, CircularProgress } from '@mui/material';
export default function LoadingOverlay({ open }: { open: boolean }) {
  return (
    <Backdrop open={open} sx={{ color: '#fff', zIndex: (t) => t.zIndex.drawer + 1 }}>
      <CircularProgress color="inherit" />
    </Backdrop>
  );
}
