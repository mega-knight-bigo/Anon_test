import { Link, useLocation } from "wouter";
import {
  LayoutDashboard,
  Database,
  Settings,
  Play,
  Users,
  Activity,
  Shield,
} from "lucide-react";
import { useAuth0 } from "@auth0/auth0-react";
import { useRBAC } from "@/hooks/useRBAC";
import type { Permission } from "@/hooks/useRBAC";
import { cn } from "@/lib/utils";
import { LoginButton } from "./auth/LoginButton";
import { LogoutButton } from "./auth/LogoutButton";

// Navigation items with icons and required permissions
const navigationConfig = [
  { name: "Dashboard", href: "/", icon: LayoutDashboard, permission: "read:dashboard" as Permission },
  { name: "Connections", href: "/connections", icon: Database, permission: "manage:connections" as Permission },
  { name: "Configurations", href: "/configurations", icon: Settings, permission: "manage:configurations" as Permission },
  { name: "De-Identify", href: "/deidentify", icon: Shield, permission: "manage:jobs" as Permission },
  { name: "Jobs", href: "/jobs", icon: Play, permission: "manage:jobs" as Permission },
  { name: "Users", href: "/users", icon: Users, permission: "manage:users" as Permission },
  { name: "Activity", href: "/activity", icon: Activity, permission: "read:activity" as Permission },
];

export function Layout({ children }: { children: React.ReactNode }) {
  const [location] = useLocation();
  const { isAuthenticated, isLoading: authLoading, logout } = useAuth0();
  const { user, hasPermission, isLoading: rbacLoading, error: authError } = useRBAC();

  // Check if we're in Auth0 callback (code exchange in progress)
  const isCallback = typeof window !== "undefined" && 
    new URLSearchParams(window.location.search).has("code");

  const isLoading = authLoading || (isAuthenticated && rbacLoading) || isCallback;

  // Filter navigation items based on user permissions
  const allowedNavigation = navigationConfig.filter(item => hasPermission(item.permission));

  if (isLoading) {
    return (
      <div className="flex h-screen items-center justify-center">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>
    );
  }

  // Handle auth errors - but give the user options instead of just "session expired"
  if (isAuthenticated && authError && !user) {
    return (
      <div className="flex h-screen items-center justify-center bg-muted/30">
        <div className="text-center space-y-4 p-8 bg-card rounded-lg border shadow-sm max-w-sm w-full">
          <div className="flex justify-center mb-4">
            <Shield className="h-12 w-12 text-primary" />
          </div>
          <h1 className="text-2xl font-bold">Authentication Issue</h1>
          <p className="text-muted-foreground text-sm">There was a problem loading your session.</p>
          <div className="pt-4 space-y-2">
            <button
              onClick={() => window.location.reload()}
              className="w-full px-4 py-2 bg-primary text-primary-foreground rounded-md hover:bg-primary/90"
            >
              Retry
            </button>
            <button
              onClick={() => logout({ logoutParams: { returnTo: window.location.origin } })}
              className="w-full px-4 py-2 bg-muted text-muted-foreground rounded-md hover:bg-muted/80"
            >
              Sign Out & Try Again
            </button>
          </div>
        </div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return (
      <div className="flex h-screen items-center justify-center bg-muted/30">
        <div className="text-center space-y-4 p-8 bg-card rounded-lg border shadow-sm max-w-sm w-full">
          <div className="flex justify-center mb-4">
            <Shield className="h-12 w-12 text-primary" />
          </div>
          <h1 className="text-2xl font-bold">Welcome to DataGuard</h1>
          <p className="text-muted-foreground">Please sign in to access the platform.</p>
          <div className="pt-4 w-full">
            <LoginButton />
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="flex h-screen">
      {/* Sidebar */}
      <aside className="w-64 bg-sidebar border-r flex flex-col">
        <div className="p-6">
          <h1 className="text-xl font-bold text-foreground flex items-center gap-2">
            <Shield className="h-6 w-6 text-primary" />
            DataGuard
          </h1>
          <p className="text-xs text-muted-foreground mt-1">Data Anonymization Platform</p>
        </div>

        <nav className="flex-1 px-3 space-y-1">
          {allowedNavigation.map((item) => {
            const isActive = location === item.href ||
              (item.href !== "/" && location.startsWith(item.href));
            return (
              <Link key={item.name} href={item.href}>
                <div
                  className={cn(
                    "flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors cursor-pointer",
                    isActive
                      ? "bg-primary text-primary-foreground"
                      : "text-sidebar-foreground hover:bg-accent"
                  )}
                >
                  <item.icon className="h-4 w-4" />
                  {item.name}
                </div>
              </Link>
            );
          })}
        </nav>

        {/* User section */}
        <div className="border-t p-4">
          {user ? (
            <div className="flex items-center gap-3">
              {user.profileImageUrl ? (
                <img src={user.profileImageUrl} alt="" className="h-8 w-8 rounded-full" />
              ) : (
                <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center text-xs font-medium text-primary">
                  {user.name?.charAt(0)?.toUpperCase() ?? "U"}
                </div>
              )}
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium truncate">{user.name}</p>
                <p className="text-xs text-muted-foreground truncate">{user.role}</p>
              </div>
              <LogoutButton />
            </div>
          ) : (
            <div className="flex items-center justify-between">
              <span className="text-sm text-muted-foreground">Loading...</span>
              <LogoutButton />
            </div>
          )}
        </div>
      </aside>

      {/* Main content */}
      <main className="flex-1 overflow-auto bg-muted/30">
        <div className="p-8">{children}</div>
      </main>
    </div>
  );
}
