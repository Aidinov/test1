import { useStore } from '../store';

export default function ThreadPanel() {
  const threads = useStore(s => s.threads);
  return (
    <div>
      <h3>Threads</h3>
      <ul>
        {threads.map(t => (
          <li key={t.id}>{t.text} - {t.status}</li>
        ))}
      </ul>
    </div>
  );
}
