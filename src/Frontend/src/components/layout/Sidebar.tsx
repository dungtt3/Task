import { NavLink } from 'react-router-dom';
import { cn } from '@/lib/cn';
import { CheckSquare, FolderKanban, Bell, LayoutDashboard, Plus } from 'lucide-react';
import { useMyProjects } from '@/hooks/useProjects';
import { useUnreadCount } from '@/hooks/useNotifications';

interface SidebarProps {
  onCreateProject: () => void;
}

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  cn(
    'flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors duration-150',
    'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-500',
    isActive
      ? 'bg-primary-50 text-primary-700 dark:bg-primary-900/20 dark:text-primary-400'
      : 'text-gray-700 hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-800'
  );

export default function Sidebar({ onCreateProject }: SidebarProps) {
  const { data: projects } = useMyProjects();
  const { data: unreadCount } = useUnreadCount();

  return (
    <aside className="flex h-full w-64 flex-col border-r border-gray-200 bg-sidebar-light dark:border-gray-700 dark:bg-sidebar-dark">
      {/* Logo */}
      <div className="flex h-14 items-center gap-2 border-b border-gray-200 px-4 dark:border-gray-700">
        <CheckSquare className="h-6 w-6 text-primary-500" aria-hidden="true" />
        <span className="text-lg font-bold text-gray-900 dark:text-white">TaskManager</span>
      </div>

      {/* Navigation */}
      <nav className="flex-1 overflow-y-auto px-3 py-4" aria-label="Main navigation">
        <ul className="flex flex-col gap-1">
          <li>
            <NavLink to="/" className={navLinkClass} end>
              <LayoutDashboard className="h-4 w-4" aria-hidden="true" />
              Dashboard
            </NavLink>
          </li>
          <li>
            <NavLink to="/tasks" className={navLinkClass}>
              <CheckSquare className="h-4 w-4" aria-hidden="true" />
              My Tasks
            </NavLink>
          </li>
          <li>
            <NavLink to="/notifications" className={navLinkClass}>
              <Bell className="h-4 w-4" aria-hidden="true" />
              Notifications
              {unreadCount && unreadCount > 0 ? (
                <span className="ml-auto inline-flex h-5 min-w-5 items-center justify-center rounded-full bg-primary-500 px-1.5 text-xs font-medium text-white">
                  {unreadCount > 99 ? '99+' : unreadCount}
                </span>
              ) : null}
            </NavLink>
          </li>
        </ul>

        {/* Projects section */}
        <div className="mt-6">
          <div className="flex items-center justify-between px-3 py-2">
            <h3 className="text-xs font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400">
              Projects
            </h3>
            <button
              onClick={onCreateProject}
              className="rounded p-0.5 text-gray-400 transition-colors duration-150 hover:bg-gray-100 hover:text-gray-600 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 dark:hover:bg-gray-800"
              aria-label="Create Project"
            >
              <Plus className="h-4 w-4" aria-hidden="true" />
            </button>
          </div>
          <ul className="flex flex-col gap-0.5">
            {projects && Array.isArray(projects) && projects.map((project) => (
              <li key={project.id}>
                <NavLink
                  to={`/projects/${project.id}`}
                  className={navLinkClass}
                >
                  <FolderKanban className="h-4 w-4" aria-hidden="true" />
                  <span className="min-w-0 truncate">{project.name}</span>
                </NavLink>
              </li>
            ))}
            {(!projects || (Array.isArray(projects) && projects.length === 0)) && (
              <li className="px-3 py-2 text-xs text-gray-400">No projects yet</li>
            )}
          </ul>
        </div>
      </nav>
    </aside>
  );
}
