import { useParams } from 'react-router-dom';
import { useEffect, useState } from 'react';
import api from '../api';

export default function EditDoc() {
  const { id } = useParams();
  const [content, setContent] = useState('');
  useEffect(() => {
    api.get(`/api/docs/${id}/content`).then(r => setContent(r.data));
  }, [id]);
  return (
    <div>
      <h1>Edit Document</h1>
      <textarea value={content} onChange={e => setContent(e.target.value)} style={{ width: '100%', height: '400px' }} />
    </div>
  );
}
