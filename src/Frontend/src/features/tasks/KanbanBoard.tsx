import { TaskItemStatus, type TaskResponse } from '@/types';
import { useUpdateTaskStatus } from '@/hooks/useTasks';
import TaskCard from './TaskCard';
import { cn } from '@/lib/cn';

const columns: { status: TaskItemStatus; label: string; color: string }[] = [
  { status: TaskItemStatus.Todo, label: 'To Do', color: 'border-t-status-todo' },
  { status: TaskItemStatus.InProgress, label: 'In Progress', color: 'border-t-status-inprogress' },
  { status: TaskItemStatus.Review, label: 'Review', color: 'border-t-status-review' },
  { status: TaskItemStatus.Done, label: 'Done', color: 'border-t-status-done' },
];

interface KanbanBoardProps {
  tasks: TaskResponse[];
  onTaskClick: (task: TaskResponse) => void;
}

export default function KanbanBoard({ tasks, onTaskClick }: KanbanBoardProps) {
  const updateStatus = useUpdateTaskStatus();

  const handleDrop = (e: React.DragEvent, status: TaskItemStatus) => {
    e.preventDefault();
    const taskId = e.dataTransfer.getData('text/plain');
    if (taskId) {
      updateStatus.mutate({ id: taskId, status });
    }
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
  };

  const handleDragStart = (e: React.DragEvent, taskId: string) => {
    e.dataTransfer.setData('text/plain', taskId);
    e.dataTransfer.effectAllowed = 'move';
  };

  return (
    <div className="flex gap-4 overflow-x-auto pb-4">
      {columns.map((col) => {
        const columnTasks = tasks.filter((t) => t.status === col.status);
        return (
          <div
            key={col.status}
            className={cn(
              'flex w-72 shrink-0 flex-col rounded-lg border-t-2 bg-gray-100/50 dark:bg-gray-900/50',
              col.color
            )}
            onDrop={(e) => handleDrop(e, col.status)}
            onDragOver={handleDragOver}
          >
            <div className="flex items-center justify-between px-4 py-3">
              <h3 className="text-sm font-semibold text-gray-700 dark:text-gray-300">
                {col.label}
              </h3>
              <span className="text-xs text-gray-400">{columnTasks.length}</span>
            </div>
            <div className="flex flex-col gap-2 px-2 pb-2">
              {columnTasks.map((task) => (
                <div
                  key={task.id}
                  draggable
                  onDragStart={(e) => handleDragStart(e, task.id)}
                  className="cursor-grab will-change-transform active:cursor-grabbing"
                >
                  <TaskCard task={task} onClick={onTaskClick} />
                </div>
              ))}
            </div>
          </div>
        );
      })}
    </div>
  );
}
