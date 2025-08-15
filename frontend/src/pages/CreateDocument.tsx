import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import http from '../lib/http';
import { createDocument } from '../api/documents';
import {
  Box,
  Button,
  MenuItem,
  Select,
  TextField,
  Typography,
} from '@mui/material';
import FormRow from '../ui/FormRow';
import MarkdownToolbar from '../ui/MarkdownToolbar';
import ErrorState from '../ui/ErrorState';
import { useSnackbar } from 'notistack';

interface OptionList {
  products: string[];
  teams: string[];
}

export default function CreateDocument() {
  const navigate = useNavigate();
  const { enqueueSnackbar } = useSnackbar();
  const [options, setOptions] = useState<OptionList>({ products: [], teams: [] });
  const [title, setTitle] = useState('');
  const [product, setProduct] = useState('');
  const [team, setTeam] = useState('');
  const [taskLink, setTaskLink] = useState('');
  const [author, setAuthor] = useState('');
  const [gitRepo, setGitRepo] = useState('');
  const [gitFilePath, setGitFilePath] = useState('');
  const [content, setContent] = useState(DEFAULT_TEMPLATE);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchOptions() {
      try {
        const [productsRes, teamsRes] = await Promise.all([
          http.get<string[]>('/products'),
          http.get<string[]>('/teams'),
        ]);
        setOptions({ products: productsRes.data, teams: teamsRes.data });
      } catch (err) {
        setError('Failed to load products or teams');
      }
    }
    fetchOptions();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await createDocument({
        title,
        product,
        team,
        author,
        taskLink,
        content,
        status: 'Draft',
        gitRepository: gitRepo,
        gitFilePath: gitFilePath
      });
      enqueueSnackbar('Document created', { variant: 'success' });
      const id = res.id;
      navigate(`/edit/${id}`);
    } catch (err) {
      setError('Failed to create document');
      enqueueSnackbar('Failed to create document', { variant: 'error' });
    }
  };

  return (
    <Box>
      <Typography variant="h4" sx={{ mb: 2 }}>
        Create New Document
      </Typography>
      {error && <ErrorState message={error} />}
      <Box component="form" onSubmit={handleSubmit}>
        <FormRow label="Title">
          <TextField value={title} onChange={(e) => setTitle(e.target.value)} required />
        </FormRow>
        <FormRow label="Product">
          <Select value={product} onChange={(e) => setProduct(e.target.value)} required displayEmpty>
            <MenuItem value="">
              <em>Select a product</em>
            </MenuItem>
            {options.products.map((p) => (
              <MenuItem key={p} value={p}>{p}</MenuItem>
            ))}
          </Select>
        </FormRow>
        <FormRow label="Team">
          <Select value={team} onChange={(e) => setTeam(e.target.value)} required displayEmpty>
            <MenuItem value="">
              <em>Select a team</em>
            </MenuItem>
            {options.teams.map((t) => (
              <MenuItem key={t} value={t}>{t}</MenuItem>
            ))}
          </Select>
        </FormRow>
        <FormRow label="Author">
          <TextField value={author} onChange={(e) => setAuthor(e.target.value)} required />
        </FormRow>
        <FormRow label="Task Link">
          <TextField value={taskLink} onChange={(e) => setTaskLink(e.target.value)} />
        </FormRow>
        <FormRow label="Git Repository URL">
          <TextField value={gitRepo} onChange={(e) => setGitRepo(e.target.value)} required />
        </FormRow>
        <FormRow label="Git File Path">
          <TextField value={gitFilePath} onChange={(e) => setGitFilePath(e.target.value)} required />
        </FormRow>
        <FormRow label="Content">
          <Box sx={{ flex: 1 }}>
            <MarkdownToolbar onInsert={(s) => setContent((c) => c + s)} />
            <TextField
              multiline
              minRows={10}
              fullWidth
              value={content}
              onChange={(e) => setContent(e.target.value)}
            />
          </Box>
        </FormRow>
        <Button type="submit">Create</Button>
      </Box>
    </Box>
  );
}

/**
 * Default Markdown template for newly created documents.
 */
const DEFAULT_TEMPLATE = `# Дизайн-документ

## Общая информация
**Название:** ...

**Ссылка на продукт в реестре:** [Product Link](https://...)

**Ссылка на задачу:** [Задача в трекере](https://...)

## Терминология и контекст
...

## Описание проблемы
...

## Анализ проблемы
### Требования
...
#### Функциональные требования
...
#### Нефункциональные требования
...

### Ограничения
...

### Предыдущие попытки решения
...

## Анализ вариантов решений
...

## Предлагаемое решение
...

## Сквозные аспекты
...`;