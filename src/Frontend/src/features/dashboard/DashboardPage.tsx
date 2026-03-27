import { useState } from 'react';
import { Link } from 'react-router-dom';
import { CheckSquare, FolderKanban, Clock, AlertTriangle, Plus } from 'lucide-react';
import { useMyTasks } from '@/hooks/useTasks';
import { useMyProjects } from '@/hooks/useProjects';
import { TaskItemStatus, Priority, type TaskResponse } from '@/types';
import TaskCard from '@/features/tasks/TaskCard';
import TaskDetailModal from '@/features/tasks/TaskDetailModal';
import Spinner from '@/components/ui/Spinner';
import { cn } from '@/lib/cn';

function StatCard({ icon, label, value, color }: { icon: React.ReactNode; label: string; value: number; color: string }) {
  return (
    <div className="flex items-center gap-4 rounded-xl border border-gray-200 bg-white p-5 dark:border-gray-700 dark:bg-gray-900">
      <div className={cn('flex h-10 w-10 items-center justify-center rounded-lg', color)}>
        {icon}
      </div>
      <div>
        <p className="text-2xl font-bold text-gray-900 dark:text-white">{value}</p>
        <p className="text-sm text-gray-500 dark:text-gray-400">{label}</p>
      </div>
    </div>
  );
}

export default function DashboardPage() {
  const { data: tasks, isLoading: tasksLoading } = useMyTasks();
  const { data: projects, isLoading: projectsLoading } = useMyProjects();
  const [selectedTask, setSelectedTask] = useState<TaskResponse | null>(null);

  if (tasksLoading || projectsLoading) {
    return (
      <div className="flex items-center justify-center py-20">
        <Spinner size="lg" />
        <span className="ml-3 text-gray-500">Loading…</span>
      </div>
    );
  }

  const allTasks = tasks || [];
  const allProjects = (projects && Array.isArray(projects)) ? projects : [];

  const todoCount = allTasks.filter((t) => t.status === TaskItemStatus.Todo).length;
  const inProgressCount = allTasks.filter((t) => t.status === TaskItemStatus.InProgress).length;
  const doneCount = allTasks.filter((t) => t.status === TaskItemStatus.Done).length;
  const overdueCount = allTasks.filter((t) =>
    t.status !== TaskItemStatus.Done && t.dueDate && new Date(t.dueDate) < new Date()
  ).length;

  const recentTasks = [...allTasks]
    .sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime())
    .slice(0, 6);

  const urgentTasks = allTasks.filter(
    (t) => t.priority === Priority.Urgent && t.status !== TaskItemStatus.Done
  );

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold text-gray-900 dark:text-white">Dashboard</h1>

      {/* Stats */}
      <div className="mb-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard
          icon={<CheckSquare className="h-5 w-5 text-blue-600" aria-hidden="true" />}
          label="To Do"
          value={todoCount}
          color="bg-blue-100 dark:bg-blue-900/30"
        />
        <StatCard
          icon={<Clock className="h-5 w-5 text-amber-600" aria-hidden="true" />}
          label="In Progress"
          value={inProgressCount}
          color="bg-amber-100 dark:bg-amber-900/30"
        />
        <StatCard
          icon={<CheckSquare className="h-5 w-5 text-emerald-600" aria-hidden="true" />}
          label="Completed"
          value={doneCount}
          color="bg-emerald-100 dark:bg-emerald-900/30"
        />
        <StatCard
          icon={<AlertTriangle className="h-5 w-5 text-red-600" aria-hidden="true" />}
          label="Overdue"
          value={overdueCount}
          color="bg-red-100 dark:bg-red-900/30"
        />
      </div>

      {/* Urgent tasks */}
      {urgentTasks.length > 0 && (
        <div className="mb-8">
          <h2 className="mb-3 flex items-center gap-2 text-lg font-semibold text-gray-900 dark:text-white">
            <AlertTriangle className="h-5 w-5 text-red-500" aria-hidden="true" />
            Urgent Tasks
          </h2>
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {urgentTasks.map((task) => (
              <TaskCard key={task.id} task={task} onClick={setSelectedTask} />
            ))}
          </div>
        </div>
      )}

      {/* Recent tasks */}
      {recentTasks.length > 0 && (
        <div className="mb-8">
          <div className="mb-3 flex items-center justify-between">
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white">Recent Tasks</h2>
            <Link
              to="/tasks"
              className="text-sm font-medium text-primary-600 hover:text-primary-500 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 rounded"
            >
              View All →
            </Link>
          </div>
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {recentTasks.map((task) => (
              <TaskCard key={task.id} task={task} onClick={setSelectedTask} />
            ))}
          </div>
        </div>
      )}

      {/* Projects */}
      <div>
        <div className="mb-3 flex items-center justify-between">
          <h2 className="text-lg font-semibold text-gray-900 dark:text-white">Projects</h2>
        </div>
        {allProjects.length === 0 ? (
          <p className="text-sm text-gray-500">No projects yet. Create one from the sidebar.</p>
        ) : (
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {allProjects.map((p) => (
              <Link
                key={p.id}
                to={`/projects/${p.id}`}
                className="rounded-xl border border-gray-200 bg-white p-5 transition-shadow, transition-border-color duration-150 hover:shadow-md hover:border-primary-200 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 dark:border-gray-700 dark:bg-gray-900 dark:hover:border-primary-800"
              >
                <div className="flex items-center gap-3">
                  <FolderKanban className="h-5 w-5 text-primary-500" aria-hidden="true" />
                  <h3 className="font-medium text-gray-900 dark:text-white">{p.name}</h3>
                </div>
                {p.description && (
                  <p className="mt-2 line-clamp-2 text-sm text-gray-500 dark:text-gray-400">
                    {p.description}
                  </p>
                )}
                <div className="mt-3 text-xs text-gray-400">
                  {p.members.length} member{p.members.length !== 1 ? 's' : ''}
                </div>
              </Link>
            ))}
          </div>
        )}
      </div>

      <TaskDetailModal task={selectedTask} isOpen={!!selectedTask} onClose={() => setSelectedTask(null)} />
    </div>
  );
}
