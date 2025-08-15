import { Drawer, Box } from '@mui/material';
import { ReactNode } from 'react';

interface Props {
  open: boolean;
  onClose: () => void;
  children: ReactNode;
}

export default function SidePanel({ open, onClose, children }: Props) {
  return (
    <Drawer anchor="right" open={open} onClose={onClose}>
      <Box sx={{ width: 320, p: 2 }}>{children}</Box>
    </Drawer>
  );
}
