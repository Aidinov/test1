import { useParams } from 'react-router-dom';

export default function ReviewDoc() {
  const { id } = useParams();
  return (
    <div>
      <h1>Review {id}</h1>
      <p>Review mode with comments.</p>
    </div>
  );
}
