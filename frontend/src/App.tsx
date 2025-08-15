import { Routes, Route, Link } from 'react-router-dom';
import DocumentList from './pages/DocumentList';
import CreateDocument from './pages/CreateDocument';
import EditDocument from './pages/EditDocument';
import ViewDocument from './pages/ViewDocument';

export default function App() {
  return (
    <div className="app-container">
      <header>
        <nav className="navbar">
          <h1>Design Doc Service</h1>
          <div>
            <Link to="/" style={{ marginRight: '1rem' }}>Documents</Link>
            <Link to="/create">Create</Link>
          </div>
        </nav>
      </header>
      <main>
        <Routes>
          <Route path="/" element={<DocumentList />} />
          <Route path="/create" element={<CreateDocument />} />
          <Route path="/edit/:id" element={<EditDocument />} />
          <Route path="/documents/:id" element={<ViewDocument />} />
        </Routes>
      </main>
    </div>
  );
}