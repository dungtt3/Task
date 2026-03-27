import { useState, type FormEvent } from 'react';
import Modal from '@/components/ui/Modal';
import Badge from '@/components/ui/Badge';
import Button from '@/components/ui/Button';
import Input from '@/components/ui/Input';
import Avatar from '@/components/ui/Avatar';
import { useAddComment, useDeleteTask, useUpdateTaskStatus } from '@/hooks/useTasks';
import { Priority, TaskItemStatus, type TaskResponse } from '@/types';
import { formatRelativeDate, formatTimeAgo, isOverdue } from '@/lib/dates';
import ConfirmDialog from '@/components/ui/ConfirmDialog';
import { Calendar, Clock, Trash2, Send } from 'lucide-react';
import { cn } from '@/lib/cn';

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

interface TaskDetailModalProps {
  task: TaskResponse | null;
  isOpen: boolean;
  onClose: () => void;
}

export default function TaskDetailModal({ task, isOpen, onClose }: TaskDetailModalProps) {
  const addComment = useAddComment();
  const deleteTask = useDeleteTask();
  const updateStatus = useUpdateTaskStatus();
  const [comment, setComment] = useState('');
  const [showDelete, setShowDelete] = useState(false);

  if (!task) return null;

  const overdue = task.status !== TaskItemStatus.Done && isOverdue(task.dueDate);

  const handleAddComment = async (e: FormEvent) => {
    e.preventDefault();
    if (!comment.trim()) return;
    await addComment.mutateAsync({ taskId: task.id, content: comment });
    setComment('');
  };

  const handleDelete = async () => {
    await deleteTask.mutateAsync(task.id);
    setShowDelete(false);
    onClose();
  };

  return (
    <>
      <Modal isOpen={isOpen} onClose={onClose} title={task.title} size="lg">
        <div className="flex flex-col gap-6">
          {/* Status + Priority + Actions */}
          <div className="flex flex-wrap items-center gap-2">
            <select
              value={task.status}
              onChange={(e) => updateStatus.mutate({ id: task.id, status: Number(e.target.value) as TaskItemStatus })}
              className="rounded-lg border border-gray-300 bg-white px-2 py-1 text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 dark:border-gray-600 dark:bg-gray-900 dark:text-gray-100"
              aria-label="Task status"
            >
              {Object.entries(statusLabels).map(([val, label]) => (
                <option key={val} value={val}>{label}</option>
              ))}
            </select>
            <Badge variant="priority" priority={task.priority}>
              {priorityLabels[task.priority]}
            </Badge>
            <button
              onClick={() => setShowDelete(true)}
              className="ml-auto rounded-lg p-2 text-gray-400 transition-colors duration-150 hover:bg-red-50 hover:text-red-500 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 dark:hover:bg-red-900/20"
              aria-label="Delete task"
            >
              <Trash2 className="h-4 w-4" aria-hidden="true" />
            </button>
          </div>

          {/* Description */}
          {task.description && (
            <div>
              <h4 className="mb-1 text-sm font-medium text-gray-500 dark:text-gray-400">Description</h4>
              <p className="text-sm text-gray-700 dark:text-gray-300 whitespace-pre-wrap">{task.description}</p>
            </div>
          )}

          {/* Meta */}
          <div className="grid grid-cols-2 gap-4 text-sm">
            {task.dueDate && (
              <div className="flex items-center gap-2">
                <Calendar className="h-4 w-4 text-gray-400" aria-hidden="true" />
                <span className={cn(overdue && 'font-medium text-red-500')}>
                  {overdue ? 'Overdue \u00b7 ' : 'Due '}
                  {formatRelativeDate(task.dueDate)}
                </span>
              </div>
            )}
            <div className="flex items-center gap-2">
              <Clock className="h-4 w-4 text-gray-400" aria-hidden="true" />
              <span className="text-gray-500">Created {formatTimeAgo(task.createdAt)}</span>
            </div>
          </div>

          {/* Tags */}
          {task.tags.length > 0 && (
            <div className="flex flex-wrap gap-2">
              {task.tags.map((tag) => (
                <Badge key={tag}>{tag}</Badge>
              ))}
            </div>
          )}

          {/* Comments */}
          <div>
            <h4 className="mb-3 text-sm font-medium text-gray-500 dark:text-gray-400">
              Comments ({task.comments.length})
            </h4>
            <div className="flex flex-col gap-3">
              {task.comments.map((c) => (
                <div key={c.id} className="flex gap-3">
                  <Avatar name={c.userId} size="sm" />
                  <div className="min-w-0 flex-1">
                    <div className="flex items-center gap-2">
                      <span className="text-xs font-medium text-gray-700 dark:text-gray-300">{c.userId}</span>
                      <span className="text-xs text-gray-400">{formatTimeAgo(c.createdAt)}</span>
                    </div>
                    <p className="mt-0.5 break-words text-sm text-gray-600 dark:text-gray-400">{c.content}</p>
                  </div>
                </div>
              ))}
            </div>

            {/* Add comment */}
            <form onSubmit={handleAddComment} className="mt-4 flex gap-2">
              <Input
                name="comment"
                placeholder="Add a comment…"
                value={comment}
                onChange={(e) => setComment(e.target.value)}
                className="flex-1"
              />
              <Button type="submit" size="sm" isLoading={addComment.isPending} aria-label="Send comment">
                <Send className="h-4 w-4" aria-hidden="true" />
              </Button>
            </form>
          </div>
        </div>
      </Modal>

      <ConfirmDialog
        isOpen={showDelete}
        onClose={() => setShowDelete(false)}
        onConfirm={handleDelete}
        title="Delete Task"
        message={`Are you sure you want to delete "${task.title}"? This action cannot be undone.`}
        isLoading={deleteTask.isPending}
      />
    </>
  );
}
