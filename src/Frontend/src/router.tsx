import { createBrowserRouter } from 'react-router-dom';
import MainLayout from '@/components/layout/MainLayout';
import ProtectedRoute from '@/components/auth/ProtectedRoute';
import LoginPage from '@/features/auth/LoginPage';
import RegisterPage from '@/features/auth/RegisterPage';
import DashboardPage from '@/features/dashboard/DashboardPage';
import TaskList from '@/features/tasks/TaskList';
import ProjectDetail from '@/features/projects/ProjectDetail';
import NotificationsPage from '@/features/notifications/NotificationsPage';

export const router = createBrowserRouter([
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    path: '/register',
    element: <RegisterPage />,
  },
  {
    element: <ProtectedRoute />,
    children: [
      {
        element: <MainLayout />,
        children: [
          { index: true, element: <DashboardPage /> },
          { path: 'tasks', element: <TaskList /> },
          { path: 'projects/:id', element: <ProjectDetail /> },
          { path: 'notifications', element: <NotificationsPage /> },
        ],
      },
    ],
  },
]);
