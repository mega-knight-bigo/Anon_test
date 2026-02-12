import { useJobs, useDeleteJob } from "@/hooks/useApi";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from "@/components/ui/table";
import { Play, Trash2 } from "lucide-react";
import { formatDateTime } from "@/lib/utils";

const statusVariant = (status: string) => {
  switch (status) {
    case "completed": return "success" as const;
    case "running": return "default" as const;
    case "failed": return "destructive" as const;
    default: return "secondary" as const;
  }
};

export function JobsPage() {
  const { data: jobs, isLoading } = useJobs();
  const deleteMutation = useDeleteJob();

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Jobs</h1>
        <p className="text-muted-foreground mt-1">Monitor and manage data processing jobs</p>
      </div>

      {isLoading ? (
        <p className="text-muted-foreground">Loading...</p>
      ) : !jobs?.length ? (
        <Card>
          <CardContent className="py-12 text-center">
            <Play className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
            <h3 className="text-lg font-semibold">No jobs yet</h3>
            <p className="text-muted-foreground mt-1">Jobs will appear here when you run de-identification tasks</p>
          </CardContent>
        </Card>
      ) : (
        <Card>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Name</TableHead>
                <TableHead>Type</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Progress</TableHead>
                <TableHead>Records</TableHead>
                <TableHead>Created</TableHead>
                <TableHead className="w-[60px]"></TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {jobs.map((job) => (
                <TableRow key={job.id}>
                  <TableCell className="font-medium">{job.name}</TableCell>
                  <TableCell>
                    <Badge variant="secondary">{job.type}</Badge>
                  </TableCell>
                  <TableCell>
                    <Badge variant={statusVariant(job.status)}>{job.status}</Badge>
                  </TableCell>
                  <TableCell>
                    <div className="flex items-center gap-2">
                      <div className="w-24 h-2 bg-muted rounded-full overflow-hidden">
                        <div
                          className="h-full bg-primary rounded-full transition-all"
                          style={{ width: `${job.progress}%` }}
                        />
                      </div>
                      <span className="text-xs text-muted-foreground">{job.progress}%</span>
                    </div>
                  </TableCell>
                  <TableCell className="text-muted-foreground">
                    {job.recordsProcessed.toLocaleString()} / {job.recordsTotal.toLocaleString()}
                  </TableCell>
                  <TableCell className="text-muted-foreground">
                    {formatDateTime(job.createdAt)}
                  </TableCell>
                  <TableCell>
                    <Button
                      size="icon"
                      variant="ghost"
                      className="text-destructive"
                      onClick={() => {
                        if (confirm("Delete this job?"))
                          deleteMutation.mutate(job.id);
                      }}
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Card>
      )}
    </div>
  );
}
