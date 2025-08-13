import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import api from '../api';

export default function Catalog() {
  const [docs, setDocs] = useState<any[]>([]);
  useEffect(() => {
    api.get('/api/docs').then(r => setDocs(r.data));
  }, []);
  return (
    <div>
      <h1>Catalog</h1>
      <ul>
        {docs.map(d => (
          <li key={d.id}><Link to={`/docs/${d.id}/edit`}>{d.title}</Link></li>
        ))}
      </ul>
    </div>
  );
}
