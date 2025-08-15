import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import http from '../lib/http';
import { createDocument } from '../api/documents';

interface OptionList {
  products: string[];
  teams: string[];
}

export default function CreateDocument() {
  const navigate = useNavigate();
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
      const id = res.id;
      navigate(`/edit/${id}`);
    } catch (err) {
      setError('Failed to create document');
    }
  };

  return (
    <div>
      <h2>Create New Document</h2>
      {error && <div style={{ color: 'red' }}>{error}</div>}
      <form className="metadata-form" onSubmit={handleSubmit}>
        <label>
          Title
          <input type="text" value={title} onChange={(e) => setTitle(e.target.value)} required />
        </label>
        <label>
          Product
          <select value={product} onChange={(e) => setProduct(e.target.value)} required>
            <option value="">Select a product</option>
            {options.products.map((p) => (
              <option key={p} value={p}>{p}</option>
            ))}
          </select>
        </label>
        <label>
          Team
          <select value={team} onChange={(e) => setTeam(e.target.value)} required>
            <option value="">Select a team</option>
            {options.teams.map((t) => (
              <option key={t} value={t}>{t}</option>
            ))}
          </select>
        </label>
        <label>
          Author
          <input
            type="text"
            value={author}
            onChange={(e) => setAuthor(e.target.value)}
            required
          />
        </label>
        <label>
          Task Link
          <input type="text" value={taskLink} onChange={(e) => setTaskLink(e.target.value)} />
        </label>
        <label>
          Git Repository URL
          <input
            type="text"
            value={gitRepo}
            onChange={(e) => setGitRepo(e.target.value)}
            required
          />
        </label>
        <label>
          Git File Path
          <input
            type="text"
            value={gitFilePath}
            onChange={(e) => setGitFilePath(e.target.value)}
            required
          />
        </label>
        <label>
          Content (Markdown)
          <textarea className="editor-textarea" value={content} onChange={(e) => setContent(e.target.value)} />
        </label>
        <button className="button" type="submit">Create</button>
      </form>
    </div>
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