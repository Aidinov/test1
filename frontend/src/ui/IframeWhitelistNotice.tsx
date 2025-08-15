import { Typography } from '@mui/material';
export default function IframeWhitelistNotice({ hosts }: { hosts: string[] }) {
  return (
    <Typography variant="caption" color="text.secondary">
      Allowed iframe hosts: {hosts.join(', ')}
    </Typography>
  );
}
