import { ReactNode } from 'react';
import { Stack, Typography } from '@mui/material';

interface Props {
  label: string;
  children: ReactNode;
}

export default function FormRow({ label, children }: Props) {
  return (
    <Stack direction="row" spacing={2} alignItems="center" sx={{ mb: 2 }}>
      <Typography sx={{ width: 120 }}>{label}</Typography>
      {children}
    </Stack>
  );
}
