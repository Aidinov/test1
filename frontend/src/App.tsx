import { Routes, Route, Link } from 'react-router-dom';
import Catalog from './pages/Catalog';
import NewDoc from './pages/NewDoc';
import EditDoc from './pages/EditDoc';
import ReviewDoc from './pages/ReviewDoc';

export default function App() {
  return (
    <div>
      <nav>
        <Link to="/">Catalog</Link> | <Link to="/docs/new">New Doc</Link>
      </nav>
      <Routes>
        <Route path="/" element={<Catalog />} />
        <Route path="/docs/new" element={<NewDoc />} />
        <Route path="/docs/:id/edit" element={<EditDoc />} />
        <Route path="/docs/:id/review" element={<ReviewDoc />} />
      </Routes>
    </div>
  );
}
