import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '@/lib/api';
import type { TaskResponse, CreateTaskRequest, UpdateTaskRequest, TaskItemStatus } from '@/types';

export function useTasksByProject(projectId: string, page = 1, pageSize = 20) {
  return useQuery({
    queryKey: ['tasks', 'project', projectId, page, pageSize],
    queryFn: async () => {
      const { data } = await api.get<TaskResponse[]>(
        `/tasks/project/${projectId}?page=${page}&pageSize=${pageSize}`
      );
      return data;
    },
    enabled: !!projectId,
  });
}

export function useMyTasks() {
  return useQuery({
    queryKey: ['tasks', 'my'],
    queryFn: async () => {
      const { data } = await api.get<TaskResponse[]>('/tasks/my');
      return data;
    },
  });
}

export function useTask(id: string) {
  return useQuery({
    queryKey: ['tasks', id],
    queryFn: async () => {
      const { data } = await api.get<TaskResponse>(`/tasks/${id}`);
      return data;
    },
    enabled: !!id,
  });
}

export function useCreateTask() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (request: CreateTaskRequest) => {
      const { data } = await api.post<TaskResponse>('/tasks', request);
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
    },
  });
}

export function useUpdateTask() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, ...request }: UpdateTaskRequest & { id: string }) => {
      const { data } = await api.put<TaskResponse>(`/tasks/${id}`, request);
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
    },
  });
}

export function useUpdateTaskStatus() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, status }: { id: string; status: TaskItemStatus }) => {
      const { data } = await api.patch<TaskResponse>(`/tasks/${id}/status`, status);
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
    },
  });
}

export function useDeleteTask() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`/tasks/${id}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
    },
  });
}

export function useAddComment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ taskId, content }: { taskId: string; content: string }) => {
      const { data } = await api.post(`/tasks/${taskId}/comments`, { content });
      return data;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['tasks', variables.taskId] });
    },
  });
}
