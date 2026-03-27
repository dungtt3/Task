import { cn } from '@/lib/cn';
import { Priority, TaskItemStatus } from '@/types';

interface BadgeProps {
  children: React.ReactNode;
  variant?: 'default' | 'priority' | 'status';
  priority?: Priority;
  status?: TaskItemStatus;
  className?: string;
}

const priorityStyles: Record<Priority, string> = {
  [Priority.Urgent]: 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400',
  [Priority.High]: 'bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400',
  [Priority.Medium]: 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400',
  [Priority.Low]: 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400',
};

const statusStyles: Record<TaskItemStatus, string> = {
  [TaskItemStatus.Todo]: 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400',
  [TaskItemStatus.InProgress]: 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400',
  [TaskItemStatus.Review]: 'bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400',
  [TaskItemStatus.Done]: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400',
};

export default function Badge({ children, variant = 'default', priority, status, className }: BadgeProps) {
  const variantStyle =
    variant === 'priority' && priority !== undefined
      ? priorityStyles[priority]
      : variant === 'status' && status !== undefined
        ? statusStyles[status]
        : 'bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300';

  return (
    <span
      className={cn(
        'inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium',
        variantStyle,
        className
      )}
    >
      {children}
    </span>
  );
}
