import { Chip } from '@mui/material';
import { DocumentStatus } from '../types';

type ChipColor = 'default' | 'success' | 'error' | 'warning';
const colorMap: Record<DocumentStatus, ChipColor> = {
  Draft: 'default',
  UnderReview: 'warning',
  Approved: 'success',
  Rejected: 'error',
};

export default function StatusChip({ status }: { status: DocumentStatus }) {
  return <Chip label={status} color={colorMap[status]} size="small" />;
}
