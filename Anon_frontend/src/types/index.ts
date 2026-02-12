// ── Connection ───────────────────────────────────────────────
export interface Connection {
  id: string;
  name: string;
  type: "s3" | "database" | "azure_blob" | "snowflake";
  config: Record<string, unknown>;
  status: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateConnection {
  name: string;
  type: string;
  config: Record<string, unknown>;
  status?: string;
}

export interface UpdateConnection {
  name?: string;
  type?: string;
  config?: Record<string, unknown>;
  status?: string;
}

// ── Configuration ───────────────────────────────────────────
export interface Configuration {
  id: string;
  name: string;
  type: "internal_consistency" | "referential_integrity" | "deidentification";
  description?: string;
  connectionId?: string;
  connectionName?: string;
  sourceConnectionId?: string;
  sourceConnectionName?: string;
  targetConnectionId?: string;
  targetConnectionName?: string;
  objectName?: string;
  sourceObjectName?: string;
  targetObjectName?: string;
  rules: Record<string, unknown>;
  createdAt: string;
  updatedAt: string;
}

export interface CreateConfiguration {
  name: string;
  type: string;
  description?: string;
  connectionId?: string;
  sourceConnectionId?: string;
  targetConnectionId?: string;
  objectName?: string;
  sourceObjectName?: string;
  targetObjectName?: string;
  rules?: Record<string, unknown>;
}

export interface UpdateConfiguration extends Partial<CreateConfiguration> {}

// ── Job ─────────────────────────────────────────────────────
export interface Job {
  id: string;
  name: string;
  type: "deidentify" | "subset" | "synthesize";
  status: "pending" | "running" | "completed" | "failed";
  sourceConnectionId: string;
  sourceConnectionName?: string;
  sourceObjects?: Record<string, unknown>;
  mappings?: Record<string, unknown>;
  integrityRules?: Record<string, unknown>;
  operation?: string;
  progress: number;
  recordsProcessed: number;
  recordsTotal: number;
  outputLocation?: string;
  errorMessage?: string;
  createdAt: string;
  completedAt?: string;
}

export interface CreateJob {
  name: string;
  type: string;
  sourceConnectionId: string;
  sourceObjects?: Record<string, unknown>;
  mappings?: Record<string, unknown>;
  integrityRules?: Record<string, unknown>;
  operation?: string;
}

export interface UpdateJob extends Partial<CreateJob> {
  status?: string;
  progress?: number;
  recordsProcessed?: number;
  recordsTotal?: number;
  outputLocation?: string;
  errorMessage?: string;
}

// ── User ────────────────────────────────────────────────────
export interface User {
  id: string;
  email: string;
  firstName?: string;
  lastName?: string;
  profileImageUrl?: string;
  name: string;
  role: "admin" | "developer" | "scheduler" | "viewer";
  status: "active" | "inactive";
  inactiveDate?: string;
  department?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateUser {
  email: string;
  firstName?: string;
  lastName?: string;
  profileImageUrl?: string;
  name: string;
  role: string;
  status: string;
  inactiveDate?: string;
  department?: string;
}

export interface UpdateUser extends Partial<CreateUser> {}

// ── Activity Log ────────────────────────────────────────────
export interface ActivityLog {
  id: string;
  action: string;
  entityType: string;
  entityId?: string;
  entityName?: string;
  details?: string;
  timestamp: string;
}

// ── Dashboard ───────────────────────────────────────────────
export interface DashboardStats {
  activeConnections: number;
  jobsProcessed: number;
  securityAlerts: number;
  runningJobs: number;
}

// ── Connection Metadata ─────────────────────────────────────
export interface ConnectionMetadata {
  id: string;
  connectionId: string;
  objectType: string;
  objectName: string;
  objectPath?: string;
  columns?: Record<string, unknown>;
  fetchedAt: string;
}
