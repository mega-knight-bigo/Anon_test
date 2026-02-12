import { useActivityLogs } from "@/hooks/useApi";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Activity, Clock } from "lucide-react";
import { formatDateTime } from "@/lib/utils";

const actionColors: Record<string, "default" | "success" | "destructive" | "secondary" | "warning"> = {
  created: "success",
  updated: "warning",
  deleted: "destructive",
};

export function ActivityPage() {
  const { data: logs, isLoading } = useActivityLogs(100);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Activity Log</h1>
        <p className="text-muted-foreground mt-1">Track all changes and actions in the platform</p>
      </div>

      {isLoading ? (
        <p className="text-muted-foreground">Loading...</p>
      ) : !logs?.length ? (
        <Card>
          <CardContent className="py-12 text-center">
            <Activity className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
            <h3 className="text-lg font-semibold">No activity yet</h3>
            <p className="text-muted-foreground mt-1">Activity will be recorded as you use the platform</p>
          </CardContent>
        </Card>
      ) : (
        <Card>
          <CardContent className="pt-6">
            <div className="space-y-4">
              {logs.map((log) => (
                <div key={log.id} className="flex items-start gap-4 pb-4 border-b last:border-0 last:pb-0">
                  <div className="mt-1">
                    <Clock className="h-4 w-4 text-muted-foreground" />
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 flex-wrap">
                      <Badge variant={actionColors[log.action.toLowerCase()] ?? "secondary"}>
                        {log.action}
                      </Badge>
                      <span className="text-sm font-medium">{log.entityType}</span>
                      {log.entityName && (
                        <span className="text-sm text-muted-foreground">— {log.entityName}</span>
                      )}
                    </div>
                    {log.details && (
                      <p className="text-sm text-muted-foreground mt-1">{log.details}</p>
                    )}
                  </div>
                  <span className="text-xs text-muted-foreground whitespace-nowrap">
                    {formatDateTime(log.timestamp)}
                  </span>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
