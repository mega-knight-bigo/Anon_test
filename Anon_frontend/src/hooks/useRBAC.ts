import { useMemo } from "react";
import { useAuth0 } from "@auth0/auth0-react";
import { useAuth } from "./useApi";

export type UserRole = "admin" | "developer" | "scheduler" | "viewer";

export type Permission =
  | "read:dashboard"
  | "manage:users"
  | "manage:connections"
  | "manage:configurations"
  | "manage:jobs"
  | "read:activity";

// Permission matrix - maps roles to their permissions
const ROLE_PERMISSIONS: Record<UserRole, Permission[]> = {
  admin: [
    "read:dashboard",
    "manage:users",
    "manage:connections",
    "manage:configurations",
    "manage:jobs",
    "read:activity",
  ],
  developer: [
    "read:dashboard",
    "manage:configurations",
    "manage:jobs",
  ],
  scheduler: [
    "read:dashboard",
    "manage:jobs",
  ],
  viewer: [
    "read:dashboard",
  ],
};

// Route-to-permission mapping for route guards
export const ROUTE_PERMISSIONS: Record<string, Permission> = {
  "/": "read:dashboard",
  "/connections": "manage:connections",
  "/configurations": "manage:configurations",
  "/configurations/new": "manage:configurations",
  "/deidentify": "manage:jobs",
  "/jobs": "manage:jobs",
  "/users": "manage:users",
  "/activity": "read:activity",
};

// Navigation items with required permissions
export interface NavItem {
  name: string;
  href: string;
  permission: Permission;
}

export const NAVIGATION_ITEMS: NavItem[] = [
  { name: "Dashboard", href: "/", permission: "read:dashboard" },
  { name: "Connections", href: "/connections", permission: "manage:connections" },
  { name: "Configurations", href: "/configurations", permission: "manage:configurations" },
  { name: "De-Identify", href: "/deidentify", permission: "manage:jobs" },
  { name: "Jobs", href: "/jobs", permission: "manage:jobs" },
  { name: "Users", href: "/users", permission: "manage:users" },
  { name: "Activity", href: "/activity", permission: "read:activity" },
];

/**
 * Get permissions for a given role
 */
export function getPermissionsForRole(role: UserRole): Permission[] {
  return ROLE_PERMISSIONS[role] || [];
}

/**
 * Check if a role has a specific permission
 */
export function roleHasPermission(role: UserRole, permission: Permission): boolean {
  return ROLE_PERMISSIONS[role]?.includes(permission) ?? false;
}

/**
 * Get allowed roles for a specific permission
 */
export function getRolesWithPermission(permission: Permission): UserRole[] {
  return (Object.keys(ROLE_PERMISSIONS) as UserRole[]).filter(role =>
    ROLE_PERMISSIONS[role].includes(permission)
  );
}

/**
 * Hook to access RBAC utilities based on current user
 */
export function useRBAC() {
  const { isAuthenticated, isLoading: authLoading } = useAuth0();
  const { data: user, isLoading: userLoading, error } = useAuth();

  const isLoading = authLoading || userLoading;
  
  // Only assign role if authenticated and user data loaded
  const role: UserRole = (isAuthenticated && user?.role as UserRole) || "viewer";

  // No permissions if not authenticated
  const permissions = useMemo(() => {
    if (!isAuthenticated) return [];
    return getPermissionsForRole(role);
  }, [role, isAuthenticated]);

  const hasPermission = useMemo(
    () => (permission: Permission) => {
      if (!isAuthenticated) return false;
      return permissions.includes(permission);
    },
    [permissions, isAuthenticated]
  );

  const canAccessRoute = useMemo(
    () => (route: string) => {
      // Check for exact match first
      if (ROUTE_PERMISSIONS[route]) {
        return hasPermission(ROUTE_PERMISSIONS[route]);
      }
      // Check for pattern matches (e.g., /configurations/:id/edit)
      for (const [pattern, permission] of Object.entries(ROUTE_PERMISSIONS)) {
        if (route.startsWith(pattern.replace(/\/:[^/]+/g, ""))) {
          return hasPermission(permission);
        }
      }
      // Default: allow access
      return true;
    },
    [hasPermission]
  );

  const allowedNavItems = useMemo(
    () => NAVIGATION_ITEMS.filter(item => hasPermission(item.permission)),
    [hasPermission]
  );

  return {
    user,
    role,
    permissions,
    hasPermission,
    canAccessRoute,
    allowedNavItems,
    isLoading,
    error,
    isAdmin: role === "admin",
    isDeveloper: role === "developer",
    isScheduler: role === "scheduler",
    isViewer: role === "viewer",
  };
}

/**
 * Hook to check if user has specific permission
 */
export function useHasPermission(permission: Permission): boolean {
  const { hasPermission } = useRBAC();
  return hasPermission(permission);
}

/**
 * Hook to check if user can access a specific route
 */
export function useCanAccessRoute(route: string): boolean {
  const { canAccessRoute } = useRBAC();
  return canAccessRoute(route);
}
