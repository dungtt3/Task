import { Calendar, MessageSquare } from 'lucide-react';
import { cn } from '@/lib/cn';
import Badge from '@/components/ui/Badge';
import { Priority, TaskItemStatus, type TaskResponse } from '@/types';
import { formatRelativeDate, isOverdue } from '@/lib/dates';

const priorityLabels: Record<Priority, string> = {
  [Priority.Low]: 'Low',
  [Priority.Medium]: 'Medium',
  [Priority.High]: 'High',
  [Priority.Urgent]: 'Urgent',
};

const statusLabels: Record<TaskItemStatus, string> = {
  [TaskItemStatus.Todo]: 'To Do',
  [TaskItemStatus.InProgress]: 'In Progress',
  [TaskItemStatus.Review]: 'Review',
  [TaskItemStatus.Done]: 'Done',
};

interface TaskCardProps {
  task: TaskResponse;
  onClick: (task: TaskResponse) => void;
}

export default function TaskCard({ task, onClick }: TaskCardProps) {
  const overdue = task.status !== TaskItemStatus.Done && isOverdue(task.dueDate);

  return (
    <button
      onClick={() => onClick(task)}
      className={cn(
        'w-full rounded-lg border border-gray-200 bg-white p-4 text-left transition-shadow, transition-border-color duration-150',
        'hover:shadow-md hover:border-primary-200',
        'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-500',
        'dark:border-gray-700 dark:bg-gray-900 dark:hover:border-primary-800'
      )}
    >
      {/* Title */}
      <h3 className="line-clamp-2 text-sm font-medium text-gray-900 dark:text-gray-100">
        {task.title}
      </h3>

      {/* Description */}
      {task.description && (
        <p className="mt-1 line-clamp-2 text-xs text-gray-500 dark:text-gray-400">
          {task.description}
        </p>
      )}

      {/* Meta row */}
      <div className="mt-3 flex flex-wrap items-center gap-2">
        <Badge variant="priority" priority={task.priority}>
          {priorityLabels[task.priority]}
        </Badge>
        <Badge variant="status" status={task.status}>
          {statusLabels[task.status]}
        </Badge>
      </div>

      {/* Footer */}
      <div className="mt-3 flex items-center gap-3 text-xs text-gray-400">
        {task.dueDate && (
          <span className={cn('flex items-center gap-1', overdue && 'font-medium text-red-500')}>
            <Calendar className="h-3 w-3" aria-hidden="true" />
            {overdue ? 'Overdue \u00b7 ' : ''}
            {formatRelativeDate(task.dueDate)}
          </span>
        )}
        {task.comments.length > 0 && (
          <span className="flex items-center gap-1">
            <MessageSquare className="h-3 w-3" aria-hidden="true" />
            {task.comments.length}
          </span>
        )}
        {task.tags.length > 0 && (
          <div className="flex min-w-0 gap-1 overflow-x-auto">
            {task.tags.slice(0, 3).map((tag) => (
              <span key={tag} className="shrink-0 rounded bg-gray-100 px-1.5 py-0.5 text-xs text-gray-600 dark:bg-gray-800 dark:text-gray-400">
                {tag}
              </span>
            ))}
            {task.tags.length > 3 && (
              <span className="text-gray-400">+{task.tags.length - 3}</span>
            )}
          </div>
        )}
      </div>
    </button>
  );
}
