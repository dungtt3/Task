import { formatDistanceToNow, isToday, isYesterday, isThisWeek, format, isSameYear } from 'date-fns';

export function formatRelativeDate(dateString: string): string {
  const date = new Date(dateString);
  if (isToday(date)) return 'Today';
  if (isYesterday(date)) return 'Yesterday';
  if (isThisWeek(date)) return format(date, 'EEEE');
  if (isSameYear(date, new Date())) return format(date, 'MMM d');
  return format(date, 'MMM d, yyyy');
}

export function formatTimeAgo(dateString: string): string {
  return formatDistanceToNow(new Date(dateString), { addSuffix: true });
}

export function isOverdue(dateString?: string): boolean {
  if (!dateString) return false;
  return new Date(dateString) < new Date();
}
