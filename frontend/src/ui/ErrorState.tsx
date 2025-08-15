import { Typography } from '@mui/material';
export default function ErrorState({ message }: { message: string }) {
  return <Typography color="error" align="center" sx={{ mt: 4 }}>{message}</Typography>;
}
