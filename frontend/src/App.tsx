import { Routes, Route } from 'react-router-dom';
import DocumentList from './pages/DocumentList';
import CreateDocument from './pages/CreateDocument';
import EditDocument from './pages/EditDocument';
import ViewDocument from './pages/ViewDocument';
import PageLayout from './ui/PageLayout';

export default function App() {
  return (
    <PageLayout>
      <Routes>
        <Route path="/" element={<DocumentList />} />
        <Route path="/create" element={<CreateDocument />} />
        <Route path="/edit/:id" element={<EditDocument />} />
        <Route path="/documents/:id" element={<ViewDocument />} />
      </Routes>
    </PageLayout>
  );
}