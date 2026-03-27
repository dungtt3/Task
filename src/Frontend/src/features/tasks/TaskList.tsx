import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Plus, ListTodo, LayoutGrid } from 'lucide-react';
import { useMyTasks } from '@/hooks/useTasks';
import TaskCard from './TaskCard';
import TaskDetailModal from './TaskDetailModal';
import CreateTaskModal from './CreateTaskModal';
import KanbanBoard from './KanbanBoard';
import Button from '@/components/ui/Button';
import Spinner from '@/components/ui/Spinner';
import EmptyState from '@/components/ui/EmptyState';
import { type TaskResponse, TaskItemStatus, Priority } from '@/types';
import { cn } from '@/lib/cn';

export default function TaskList() {
  const [searchParams, setSearchParams] = useSearchParams();
  const view = searchParams.get('view') || 'list';
  const statusFilter = searchParams.get('status');
  const priorityFilter = searchParams.get('priority');

  const { data: tasks, isLoading } = useMyTasks();
  const [selectedTask, setSelectedTask] = useState<TaskResponse | null>(null);
  const [showCreate, setShowCreate] = useState(false);

  const setView = (v: string) => {
    const params = new URLSearchParams(searchParams);
    if (v === 'list') params.delete('view');
    else params.set('view', v);
    setSearchParams(params);
  };

  // Filter tasks
  const filteredTasks = (tasks || []).filter((t) => {
    if (statusFilter) {
      const statuses = statusFilter.split(',').map(Number);
      if (!statuses.includes(t.status)) return false;
    }
    if (priorityFilter) {
      const priorities = priorityFilter.split(',').map(Number);
      if (!priorities.includes(t.priority)) return false;
    }
    return true;
  });

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-20">
        <Spinner size="lg" />
        <span className="ml-3 text-gray-500">Loading…</span>
      </div>
    );
  }

  return (
    <div>
      {/* Toolbar */}
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">My Tasks</h1>
        <div className="flex items-center gap-2">
          {/* View toggle */}
          <div className="flex rounded-lg border border-gray-200 dark:border-gray-700">
            <button
              onClick={() => setView('list')}
              className={cn(
                'rounded-l-lg p-2 transition-colors duration-150 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-500',
                view === 'list'
                  ? 'bg-primary-50 text-primary-600 dark:bg-primary-900/20'
                  : 'text-gray-500 hover:bg-gray-50 dark:hover:bg-gray-800'
              )}
              aria-label="List view"
              aria-pressed={view === 'list'}
            >
              <ListTodo className="h-4 w-4" aria-hidden="true" />
            </button>
            <button
              onClick={() => setView('kanban')}
              className={cn(
                'rounded-r-lg p-2 transition-colors duration-150 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-500',
                view === 'kanban'
                  ? 'bg-primary-50 text-primary-600 dark:bg-primary-900/20'
                  : 'text-gray-500 hover:bg-gray-50 dark:hover:bg-gray-800'
              )}
              aria-label="Kanban view"
              aria-pressed={view === 'kanban'}
            >
              <LayoutGrid className="h-4 w-4" aria-hidden="true" />
            </button>
          </div>

          <Button onClick={() => setShowCreate(true)} size="sm">
            <Plus className="h-4 w-4" aria-hidden="true" />
            Create Task
          </Button>
        </div>
      </div>

      {/* Content */}
      {filteredTasks.length === 0 ? (
        <EmptyState
          icon={<ListTodo className="h-12 w-12" />}
          title="No Tasks Yet"
          description="Create your first task to get started."
          action={
            <Button onClick={() => setShowCreate(true)}>
              <Plus className="h-4 w-4" aria-hidden="true" />
              Create Task
            </Button>
          }
        />
      ) : view === 'kanban' ? (
        <KanbanBoard tasks={filteredTasks} onTaskClick={setSelectedTask} />
      ) : (
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          {filteredTasks.map((task) => (
            <TaskCard key={task.id} task={task} onClick={setSelectedTask} />
          ))}
        </div>
      )}

      {/* Modals */}
      <TaskDetailModal
        task={selectedTask}
        isOpen={!!selectedTask}
        onClose={() => setSelectedTask(null)}
      />
      <CreateTaskModal isOpen={showCreate} onClose={() => setShowCreate(false)} />
    </div>
  );
}
