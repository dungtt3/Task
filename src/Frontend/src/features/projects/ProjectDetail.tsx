import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { Plus, Settings, Users } from 'lucide-react';
import { useProject } from '@/hooks/useProjects';
import { useTasksByProject } from '@/hooks/useTasks';
import TaskCard from '@/features/tasks/TaskCard';
import TaskDetailModal from '@/features/tasks/TaskDetailModal';
import CreateTaskModal from '@/features/tasks/CreateTaskModal';
import Button from '@/components/ui/Button';
import Spinner from '@/components/ui/Spinner';
import EmptyState from '@/components/ui/EmptyState';
import Badge from '@/components/ui/Badge';
import { type TaskResponse, ProjectRole } from '@/types';

const roleLabels: Record<ProjectRole, string> = {
  [ProjectRole.Member]: 'Member',
  [ProjectRole.Admin]: 'Admin',
  [ProjectRole.Owner]: 'Owner',
};

export default function ProjectDetail() {
  const { id } = useParams<{ id: string }>();
  const { data: project, isLoading: projectLoading } = useProject(id || '');
  const { data: tasks, isLoading: tasksLoading } = useTasksByProject(id || '');

  const [selectedTask, setSelectedTask] = useState<TaskResponse | null>(null);
  const [showCreate, setShowCreate] = useState(false);

  if (projectLoading) {
    return (
      <div className="flex items-center justify-center py-20">
        <Spinner size="lg" />
        <span className="ml-3 text-gray-500">Loading…</span>
      </div>
    );
  }

  if (!project) {
    return (
      <EmptyState title="Project Not Found" description="This project doesn\u2019t exist or you don\u2019t have access." />
    );
  }

  return (
    <div>
      {/* Header */}
      <div className="mb-6 flex items-start justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white">{project.name}</h1>
          {project.description && (
            <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">{project.description}</p>
          )}
        </div>
        <Button onClick={() => setShowCreate(true)} size="sm">
          <Plus className="h-4 w-4" aria-hidden="true" />
          Create Task
        </Button>
      </div>

      {/* Members */}
      <div className="mb-6 flex items-center gap-2">
        <Users className="h-4 w-4 text-gray-400" aria-hidden="true" />
        <span className="text-sm text-gray-500">{project.members.length} members</span>
        <div className="flex gap-1">
          {project.members.slice(0, 5).map((m) => (
            <Badge key={m.userId}>{roleLabels[m.role]}</Badge>
          ))}
        </div>
      </div>

      {/* Tasks */}
      {tasksLoading ? (
        <div className="flex items-center justify-center py-10">
          <Spinner />
          <span className="ml-2 text-sm text-gray-500">Loading tasks…</span>
        </div>
      ) : !tasks || (Array.isArray(tasks) && tasks.length === 0) ? (
        <EmptyState
          icon={<Plus className="h-12 w-12" />}
          title="No Tasks in This Project"
          description="Create your first task for this project."
          action={
            <Button onClick={() => setShowCreate(true)}>
              <Plus className="h-4 w-4" aria-hidden="true" />
              Create Task
            </Button>
          }
        />
      ) : (
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          {(Array.isArray(tasks) ? tasks : []).map((task: TaskResponse) => (
            <TaskCard key={task.id} task={task} onClick={setSelectedTask} />
          ))}
        </div>
      )}

      {/* Modals */}
      <TaskDetailModal task={selectedTask} isOpen={!!selectedTask} onClose={() => setSelectedTask(null)} />
      <CreateTaskModal isOpen={showCreate} onClose={() => setShowCreate(false)} defaultProjectId={id} />
    </div>
  );
}
