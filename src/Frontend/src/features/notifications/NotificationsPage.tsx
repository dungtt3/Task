import { Bell, CheckCheck } from 'lucide-react';
import { useNotifications, useMarkAsRead, useMarkAllAsRead, useUnreadCount } from '@/hooks/useNotifications';
import { NotificationType } from '@/types';
import Button from '@/components/ui/Button';
import Spinner from '@/components/ui/Spinner';
import EmptyState from '@/components/ui/EmptyState';
import { formatTimeAgo } from '@/lib/dates';
import { cn } from '@/lib/cn';

const typeLabels: Record<NotificationType, string> = {
  [NotificationType.TaskAssigned]: 'Task Assigned',
  [NotificationType.TaskCompleted]: 'Task Completed',
  [NotificationType.DueReminder]: 'Due Reminder',
  [NotificationType.ProjectUpdate]: 'Project Update',
};

export default function NotificationsPage() {
  const { data: notifications, isLoading } = useNotifications();
  const { data: unreadCount } = useUnreadCount();
  const markAsRead = useMarkAsRead();
  const markAllAsRead = useMarkAllAsRead();

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
      <div className="mb-6 flex items-center justify-between">
        <div className="flex items-center gap-3">
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Notifications</h1>
          {unreadCount && unreadCount > 0 ? (
            <span className="rounded-full bg-primary-100 px-2.5 py-0.5 text-xs font-medium text-primary-700 dark:bg-primary-900/30 dark:text-primary-400">
              {unreadCount} unread
            </span>
          ) : null}
        </div>
        {unreadCount && unreadCount > 0 ? (
          <Button
            variant="ghost"
            size="sm"
            onClick={() => markAllAsRead.mutate()}
            isLoading={markAllAsRead.isPending}
          >
            <CheckCheck className="h-4 w-4" aria-hidden="true" />
            Mark All as Read
          </Button>
        ) : null}
      </div>

      {!notifications || (Array.isArray(notifications) && notifications.length === 0) ? (
        <EmptyState
          icon={<Bell className="h-12 w-12" />}
          title="No Notifications"
          description="You\u2019re all caught up!"
        />
      ) : (
        <ul className="flex flex-col gap-2" role="list">
          {(Array.isArray(notifications) ? notifications : []).map((n) => (
            <li key={n.id}>
              <button
                onClick={() => { if (!n.isRead) markAsRead.mutate(n.id); }}
                className={cn(
                  'w-full rounded-lg border p-4 text-left transition-colors duration-150',
                  'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-500',
                  n.isRead
                    ? 'border-gray-200 bg-white dark:border-gray-700 dark:bg-gray-900'
                    : 'border-primary-200 bg-primary-50 dark:border-primary-800 dark:bg-primary-900/10'
                )}
              >
                <div className="flex items-start justify-between gap-3">
                  <div className="min-w-0 flex-1">
                    <div className="flex items-center gap-2">
                      <span className="text-xs font-medium text-primary-600 dark:text-primary-400">
                        {typeLabels[n.type]}
                      </span>
                      {!n.isRead && (
                        <span className="h-2 w-2 rounded-full bg-primary-500" />
                      )}
                    </div>
                    <h3 className="mt-1 text-sm font-medium text-gray-900 dark:text-gray-100">
                      {n.title}
                    </h3>
                    <p className="mt-0.5 text-sm text-gray-500 dark:text-gray-400">{n.message}</p>
                  </div>
                  <span className="shrink-0 text-xs text-gray-400">{formatTimeAgo(n.createdAt)}</span>
                </div>
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
