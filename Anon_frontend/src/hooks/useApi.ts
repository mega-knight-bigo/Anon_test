import { useState, useEffect } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useAuth0 } from "@auth0/auth0-react";
import { api, setAccessToken } from "@/lib/api";
import type {
  Connection, CreateConnection, UpdateConnection,
  Configuration, CreateConfiguration, UpdateConfiguration,
  Job, CreateJob, UpdateJob,
  User, CreateUser, UpdateUser,
  ActivityLog, DashboardStats, ConnectionMetadata,
} from "@/types";

// ── Auth ────────────────────────────────────────────────────
export function useAuth() {
  const { isAuthenticated, isLoading, getAccessTokenSilently } = useAuth0();
  const [ready, setReady] = useState(false);
  
  // Wait a moment after Auth0 reports ready to ensure token is actually available
  useEffect(() => {
    if (isAuthenticated && !isLoading) {
      // Small delay to let Auth0 finish internal processing
      const timer = setTimeout(() => setReady(true), 500);
      return () => clearTimeout(timer);
    } else {
      setReady(false);
    }
  }, [isAuthenticated, isLoading]);
  
  return useQuery<User>({
    queryKey: ["auth", "user"],
    queryFn: async () => {
      // Get fresh token directly from Auth0
      const token = await getAccessTokenSilently({
        authorizationParams: {
          audience: import.meta.env.VITE_AUTH0_AUDIENCE,
        },
      });
      if (!token) {
        throw new Error("No token received");
      }
      setAccessToken(token);
      return api.get("/auth/user");
    },
    retry: 3,
    retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 5000),
    staleTime: 5 * 60 * 1000,
    enabled: ready,
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
  });
}

// ── Dashboard ───────────────────────────────────────────────
export function useDashboardStats() {
  return useQuery<DashboardStats>({
    queryKey: ["dashboard", "stats"],
    queryFn: () => api.get("/dashboard/stats"),
  });
}

// ── Connections ─────────────────────────────────────────────
export function useConnections() {
  return useQuery<Connection[]>({
    queryKey: ["connections"],
    queryFn: () => api.get("/connections"),
  });
}

export function useConnection(id: string) {
  return useQuery<Connection>({
    queryKey: ["connections", id],
    queryFn: () => api.get(`/connections/${id}`),
    enabled: !!id,
  });
}

export function useCreateConnection() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateConnection) => api.post<Connection>("/connections", data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["connections"] }),
  });
}

export function useUpdateConnection() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateConnection }) =>
      api.patch<Connection>(`/connections/${id}`, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["connections"] }),
  });
}

export function useDeleteConnection() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.delete(`/connections/${id}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["connections"] }),
  });
}

export function useTestConnection() {
  return useMutation({
    mutationFn: (id: string) => api.post(`/connections/${id}/test`),
  });
}

export interface TestConnectionResult {
  success: boolean;
  message: string;
  details?: string;
}

export function useTestConnectionConfig() {
  return useMutation<TestConnectionResult, Error, { type: string; config: Record<string, unknown> }>({
    mutationFn: (data) => api.post<TestConnectionResult>("/connections/test-config", data),
  });
}

export function useConnectionMetadata(id: string) {
  return useQuery<ConnectionMetadata[]>({
    queryKey: ["connections", id, "metadata"],
    queryFn: () => api.get(`/connections/${id}/metadata`),
    enabled: !!id,
  });
}

export function useRefreshMetadata() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.post(`/connections/${id}/refresh-metadata`),
    onSuccess: (_d, id) => qc.invalidateQueries({ queryKey: ["connections", id, "metadata"] }),
  });
}

// ── Configurations ──────────────────────────────────────────
export function useConfigurations(type?: string) {
  return useQuery<Configuration[]>({
    queryKey: ["configurations", { type }],
    queryFn: () => api.get(`/configurations${type ? `?type=${type}` : ""}`),
  });
}

export function useConfiguration(id: string) {
  return useQuery<Configuration>({
    queryKey: ["configurations", id],
    queryFn: () => api.get(`/configurations/${id}`),
    enabled: !!id,
  });
}

export function useCreateConfiguration() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateConfiguration) => api.post<Configuration>("/configurations", data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["configurations"] }),
  });
}

export function useUpdateConfiguration() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateConfiguration }) =>
      api.patch<Configuration>(`/configurations/${id}`, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["configurations"] }),
  });
}

export function useDeleteConfiguration() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.delete(`/configurations/${id}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["configurations"] }),
  });
}

// ── Jobs ────────────────────────────────────────────────────
export function useJobs() {
  return useQuery<Job[]>({
    queryKey: ["jobs"],
    queryFn: () => api.get("/jobs"),
  });
}

export function useJob(id: string) {
  return useQuery<Job>({
    queryKey: ["jobs", id],
    queryFn: () => api.get(`/jobs/${id}`),
    enabled: !!id,
  });
}

export function useCreateJob() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateJob) => api.post<Job>("/jobs", data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["jobs"] }),
  });
}

export function useUpdateJob() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateJob }) =>
      api.patch<Job>(`/jobs/${id}`, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["jobs"] }),
  });
}

export function useDeleteJob() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.delete(`/jobs/${id}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["jobs"] }),
  });
}

// ── Users ───────────────────────────────────────────────────
export function useUsers(status?: string, search?: string) {
  return useQuery<User[]>({
    queryKey: ["users", { status, search }],
    queryFn: () => {
      const params = new URLSearchParams();
      if (status) params.set("status", status);
      if (search) params.set("search", search);
      const qs = params.toString();
      return api.get(`/users${qs ? `?${qs}` : ""}`);
    },
  });
}

export function useCreateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateUser) => api.post<User>("/users", data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["users"] }),
  });
}

export function useUpdateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateUser }) =>
      api.put<User>(`/users/${id}`, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["users"] }),
  });
}

export function useDeleteUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.delete(`/users/${id}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["users"] }),
  });
}

// ── Activity Logs ───────────────────────────────────────────
export function useActivityLogs(limit = 50) {
  return useQuery<ActivityLog[]>({
    queryKey: ["activity", { limit }],
    queryFn: () => api.get(`/activity?limit=${limit}`),
  });
}
