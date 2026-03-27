import { useState } from 'react';
import { Outlet } from 'react-router-dom';
import Sidebar from './Sidebar';
import Header from './Header';
import CreateProjectModal from '@/features/projects/CreateProjectModal';

export default function MainLayout() {
  const [showCreateProject, setShowCreateProject] = useState(false);

  return (
    <div className="flex h-screen overflow-hidden">
      {/* Skip link */}
      <a href="#main-content" className="skip-link">
        Skip to main content
      </a>

      <Sidebar onCreateProject={() => setShowCreateProject(true)} />

      <div className="flex min-w-0 flex-1 flex-col">
        <Header />
        <main id="main-content" className="flex-1 overflow-y-auto bg-gray-50 p-6 dark:bg-gray-950">
          <Outlet />
        </main>
      </div>

      <CreateProjectModal
        isOpen={showCreateProject}
        onClose={() => setShowCreateProject(false)}
      />
    </div>
  );
}
