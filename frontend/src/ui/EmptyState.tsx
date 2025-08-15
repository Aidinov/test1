import { Typography } from '@mui/material';
export default function EmptyState({ message = 'No data' }: { message?: string }) {
  return <Typography align="center" sx={{ mt: 4 }}>{message}</Typography>;
}
