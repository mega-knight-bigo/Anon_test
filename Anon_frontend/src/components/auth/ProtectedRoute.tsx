import { useEffect } from "react";
import { Redirect, useLocation } from "wouter";
import { useRBAC } from "@/hooks/useRBAC";
import type { Permission } from "@/hooks/useRBAC";
import { useToast } from "@/hooks/use-toast";

interface ProtectedRouteProps {
  children: React.ReactNode;
  permission: Permission;
  fallbackPath?: string;
}

/**
 * A component that guards routes based on user permissions.
 * Redirects unauthorized users to fallback path (default: dashboard).
 */
export function ProtectedRoute({
  children,
  permission,
  fallbackPath = "/",
}: ProtectedRouteProps) {
  const { hasPermission, isLoading, user } = useRBAC();
  const { toast } = useToast();
  const [location] = useLocation();

  const isAllowed = hasPermission(permission);

  useEffect(() => {
    // Show toast when access is denied (after loading & user is authenticated)
    if (!isLoading && user && !isAllowed) {
      toast({
        title: "Access Denied",
        description: "You don't have permission to access this page.",
        variant: "destructive",
      });
    }
  }, [isLoading, user, isAllowed, toast, location]);

  // Show nothing while loading
  if (isLoading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>
    );
  }

  // Redirect if not allowed
  if (!isAllowed) {
    return <Redirect to={fallbackPath} />;
  }

  return <>{children}</>;
}

/**
 * A wrapper component for conditional rendering based on permissions.
 * Unlike ProtectedRoute, this doesn't redirect - it just hides content.
 */
interface RequirePermissionProps {
  children: React.ReactNode;
  permission: Permission;
  fallback?: React.ReactNode;
}

export function RequirePermission({
  children,
  permission,
  fallback = null,
}: RequirePermissionProps) {
  const { hasPermission, isLoading } = useRBAC();

  if (isLoading) {
    return null;
  }

  if (!hasPermission(permission)) {
    return <>{fallback}</>;
  }

  return <>{children}</>;
}
