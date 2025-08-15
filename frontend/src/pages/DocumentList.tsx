import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
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
  const [documents, setDocuments] = useState<DocumentSummary[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [team, setTeam] = useState('');
  const [product, setProduct] = useState('');
  const [author, setAuthor] = useState('');
  const [status, setStatus] = useState<DocumentStatus | ''>('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  async function fetchDocs(overrides: Partial<{ page: number }> = {}) {
    setLoading(true);
    try {
      const data = await listDocuments({
        team: team || undefined,
        product: product || undefined,
        author: author || undefined,
        status: status || undefined,
        page: overrides.page ?? page,
        pageSize
      });
      setDocuments(data);
      setError(null);
    } catch {
      setError('Failed to load documents');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    fetchDocs();
  }, [page, pageSize]);

  function applyFilters() {
    setPage(1);
    fetchDocs({ page: 1 });
  }

  if (loading) return <LoadingOverlay open />;
  if (error) return <ErrorState message={error} />;

  return (
    <Box>
      <Typography variant="h4" sx={{ mb: 2 }}>
        Documents
      </Typography>
      <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
        <TextField label="Team" value={team} onChange={(e) => setTeam(e.target.value)} />
        <TextField label="Product" value={product} onChange={(e) => setProduct(e.target.value)} />
        <TextField label="Author" value={author} onChange={(e) => setAuthor(e.target.value)} />
        <Select
          displayEmpty
          value={status}
          onChange={(e) => setStatus(e.target.value as DocumentStatus | '')}
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
        <Button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1}>
          Prev
        </Button>
        <Typography>Page {page}</Typography>
        <Button onClick={() => setPage((p) => p + 1)}>Next</Button>
        <Select
          value={pageSize}
          onChange={(e) => {
            setPageSize(Number(e.target.value));
            setPage(1);
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
