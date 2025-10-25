import { Chip } from '@mui/material';
import { RemarkSeverity } from '../types';

type ChipColor = 'default' | 'error' | 'warning';
const colorMap: Record<RemarkSeverity, ChipColor> = {
  Critical: 'error',
  Desirable: 'warning',
  Opinion: 'default',
};

export default function SeverityChip({ severity }: { severity: RemarkSeverity }) {
  return <Chip label={severity} color={colorMap[severity]} size="small" />;
}
