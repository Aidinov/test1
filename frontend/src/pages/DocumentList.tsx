import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  Box,
  Button,
  MenuItem,
  Select,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material';
import { listDocuments } from '../api/documents';
import { DocumentStatus, DocumentSummary } from '../types';
import StatusChip from '../ui/StatusChip';
import LoadingOverlay from '../ui/LoadingOverlay';
import EmptyState from '../ui/EmptyState';
import ErrorState from '../ui/ErrorState';

export default function DocumentList() {
  const navigate = useNavigate();
  const renderCount = useRef(0);
  renderCount.current += 1;
  console.debug('[DocumentList render]', renderCount.current);

  const [inputs, setInputs] = useState<{ team: string; product: string; author: string; status: DocumentStatus | '' }>({ team: '', product: '', author: '', status: '' });
  const [query, setQuery] = useState<{ team: string; product: string; author: string; status: DocumentStatus | ''; page: number; pageSize: number }>({ team: '', product: '', author: '', status: '', page: 1, pageSize: 10 });

  const effectCount = useRef(0);
  useEffect(() => {
    effectCount.current += 1;
    console.debug('[DocumentList effect]', effectCount.current, new Error().stack);
  }, [query]);

  const { data: documents = [], isLoading, isError } = useQuery<DocumentSummary[]>({
    queryKey: ['documents', query],
    queryFn: () => {
      console.debug('[DocumentList query]', new Error().stack);
      return listDocuments({
        team: query.team || undefined,
        product: query.product || undefined,
        author: query.author || undefined,
        status: query.status || undefined,
        page: query.page,
        pageSize: query.pageSize
      });
    }
  });

  const applyFilters = () => {
    setQuery((q) => ({ ...q, ...inputs, page: 1 }));
  };

  if (isLoading) return <LoadingOverlay open />;
  if (isError) return <ErrorState message="Failed to load documents" />;

  return (
    <Box>
      <Typography variant="h4" sx={{ mb: 2 }}>
        Documents
      </Typography>
      <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
        <TextField label="Team" value={inputs.team} onChange={(e) => setInputs((i) => ({ ...i, team: e.target.value }))} />
        <TextField label="Product" value={inputs.product} onChange={(e) => setInputs((i) => ({ ...i, product: e.target.value }))} />
        <TextField label="Author" value={inputs.author} onChange={(e) => setInputs((i) => ({ ...i, author: e.target.value }))} />
        <Select
          displayEmpty
          value={inputs.status}
          onChange={(e) => setInputs((i) => ({ ...i, status: e.target.value as DocumentStatus | '' }))}
        >
          <MenuItem value="">
            <em>All</em>
          </MenuItem>
          <MenuItem value="Draft">Draft</MenuItem>
          <MenuItem value="UnderReview">UnderReview</MenuItem>
          <MenuItem value="Approved">Approved</MenuItem>
          <MenuItem value="Rejected">Rejected</MenuItem>
        </Select>
        <Button onClick={applyFilters}>Apply</Button>
      </Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
        <Button onClick={() => setQuery((q) => ({ ...q, page: Math.max(1, q.page - 1) }))} disabled={query.page === 1}>
          Prev
        </Button>
        <Typography>Page {query.page}</Typography>
        <Button onClick={() => setQuery((q) => ({ ...q, page: q.page + 1 }))}>Next</Button>
        <Select
          value={query.pageSize}
          onChange={(e) => {
            setQuery((q) => ({ ...q, pageSize: Number(e.target.value), page: 1 }));
          }}
        >
          <MenuItem value={5}>5</MenuItem>
          <MenuItem value={10}>10</MenuItem>
          <MenuItem value={20}>20</MenuItem>
        </Select>
      </Box>
      {documents.length === 0 ? (
        <EmptyState message="No documents found." />
      ) : (
        <Table size="small" sx={{ cursor: 'pointer' }}>
          <TableHead>
            <TableRow>
              <TableCell>Title</TableCell>
              <TableCell>Product</TableCell>
              <TableCell>Team</TableCell>
              <TableCell>Author</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>UpdatedAt</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {documents.map((doc) => (
              <TableRow key={doc.id} onClick={() => navigate(`/documents/${doc.id}`)}>
                <TableCell>{doc.title}</TableCell>
                <TableCell>{doc.product}</TableCell>
                <TableCell>{doc.team}</TableCell>
                <TableCell>{doc.author}</TableCell>
                <TableCell>
                  <StatusChip status={doc.status} />
                </TableCell>
                <TableCell>{new Date(doc.updatedAt).toLocaleString()}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
    </Box>
  );
}
