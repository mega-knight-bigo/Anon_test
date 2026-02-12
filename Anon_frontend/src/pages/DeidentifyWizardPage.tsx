import { useState } from "react";
import { useConnections, useConnectionMetadata, useRefreshMetadata, useCreateJob } from "@/hooks/useApi";
import { Card, CardContent, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select } from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";
import { useLocation } from "wouter";
import { ArrowRight, ArrowLeft, Database, Shield, CheckCircle, RefreshCw } from "lucide-react";

type Step = "source" | "objects" | "rules" | "review";

const steps: { key: Step; label: string; icon: React.ElementType }[] = [
  { key: "source", label: "Select Source", icon: Database },
  { key: "objects", label: "Choose Objects", icon: Database },
  { key: "rules", label: "Define Rules", icon: Shield },
  { key: "review", label: "Review & Run", icon: CheckCircle },
];

export function DeidentifyWizardPage() {
  const [, navigate] = useLocation();
  const [step, setStep] = useState<Step>("source");
  const { data: connections } = useConnections();
  const [sourceId, setSourceId] = useState("");
  const { data: metadata, isLoading: metaLoading } = useConnectionMetadata(sourceId);
  const refreshMutation = useRefreshMetadata();
  const createJobMutation = useCreateJob();

  const [selectedObjects, setSelectedObjects] = useState<string[]>([]);
  const [jobName, setJobName] = useState("");
  const [rulesStr, setRulesStr] = useState("{}");

  const currentIdx = steps.findIndex((s) => s.key === step);

  const handleNext = () => {
    const next = steps[currentIdx + 1];
    if (next) setStep(next.key);
  };

  const handleBack = () => {
    const prev = steps[currentIdx - 1];
    if (prev) setStep(prev.key);
  };

  const handleRun = () => {
    try {
      const mappings = JSON.parse(rulesStr);
      createJobMutation.mutate(
        {
          name: jobName || "De-identification Job",
          type: "deidentify",
          sourceConnectionId: sourceId,
          sourceObjects: { tables: selectedObjects },
          mappings,
          operation: "deidentify",
        },
        { onSuccess: () => navigate("/jobs") }
      );
    } catch {
      alert("Invalid JSON in rules");
    }
  };

  const toggleObject = (name: string) => {
    setSelectedObjects((prev) =>
      prev.includes(name) ? prev.filter((o) => o !== name) : [...prev, name]
    );
  };

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div>
        <h1 className="text-3xl font-bold">De-Identify Data</h1>
        <p className="text-muted-foreground mt-1">Walk through the steps to anonymize your data</p>
      </div>

      {/* Step indicator */}
      <div className="flex items-center gap-2">
        {steps.map((s, i) => (
          <div key={s.key} className="flex items-center gap-2">
            <div
              className={`flex items-center gap-2 px-3 py-1.5 rounded-full text-sm font-medium ${
                i <= currentIdx ? "bg-primary text-primary-foreground" : "bg-muted text-muted-foreground"
              }`}
            >
              <s.icon className="h-4 w-4" />
              {s.label}
            </div>
            {i < steps.length - 1 && <ArrowRight className="h-4 w-4 text-muted-foreground" />}
          </div>
        ))}
      </div>

      {/* Step Content */}
      <Card>
        <CardContent className="pt-6">
          {step === "source" && (
            <div className="space-y-4">
              <CardTitle className="text-lg">Select Source Connection</CardTitle>
              <CardDescription>Choose the database or storage containing data to de-identify</CardDescription>
              <Select value={sourceId} onChange={(e) => setSourceId(e.target.value)}>
                <option value="">Select a connection...</option>
                {connections?.map((c) => (
                  <option key={c.id} value={c.id}>{c.name} ({c.type})</option>
                ))}
              </Select>
              {sourceId && (
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => refreshMutation.mutate(sourceId)}
                  disabled={refreshMutation.isPending}
                >
                  <RefreshCw className={`h-4 w-4 ${refreshMutation.isPending ? "animate-spin" : ""}`} />
                  Refresh Schema
                </Button>
              )}
            </div>
          )}

          {step === "objects" && (
            <div className="space-y-4">
              <CardTitle className="text-lg">Select Objects</CardTitle>
              <CardDescription>Choose tables or objects to include in the de-identification job</CardDescription>
              {metaLoading ? (
                <p className="text-muted-foreground">Loading schema...</p>
              ) : !metadata?.length ? (
                <p className="text-muted-foreground">No objects found. Try refreshing the schema on the previous step.</p>
              ) : (
                <div className="space-y-2 max-h-80 overflow-auto">
                  {metadata.map((m) => (
                    <label
                      key={m.id}
                      className="flex items-center gap-3 p-3 rounded-md border hover:bg-muted/50 cursor-pointer"
                    >
                      <input
                        type="checkbox"
                        checked={selectedObjects.includes(m.objectName)}
                        onChange={() => toggleObject(m.objectName)}
                        className="rounded"
                      />
                      <div>
                        <p className="text-sm font-medium">{m.objectName}</p>
                        <p className="text-xs text-muted-foreground">{m.objectType}</p>
                      </div>
                    </label>
                  ))}
                </div>
              )}
              <div className="text-sm text-muted-foreground">
                {selectedObjects.length} object(s) selected
              </div>
            </div>
          )}

          {step === "rules" && (
            <div className="space-y-4">
              <CardTitle className="text-lg">Define De-identification Rules</CardTitle>
              <CardDescription>Configure how each column should be anonymized</CardDescription>
              <div>
                <Label>Job Name</Label>
                <Input
                  value={jobName}
                  onChange={(e) => setJobName(e.target.value)}
                  placeholder="My De-identification Job"
                />
              </div>
              <div>
                <Label>Column Mappings (JSON)</Label>
                <textarea
                  className="flex min-h-[200px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm font-mono"
                  value={rulesStr}
                  onChange={(e) => setRulesStr(e.target.value)}
                  placeholder={`{
  "columns": [
    { "name": "email", "method": "mask", "options": { "character": "*" } },
    { "name": "phone", "method": "hash" },
    { "name": "ssn", "method": "redact" }
  ]
}`}
                />
              </div>
            </div>
          )}

          {step === "review" && (
            <div className="space-y-4">
              <CardTitle className="text-lg">Review & Run</CardTitle>
              <CardDescription>Confirm your de-identification job settings</CardDescription>
              <div className="space-y-3 rounded-md border p-4 bg-muted/30">
                <div className="flex justify-between">
                  <span className="text-sm text-muted-foreground">Source</span>
                  <span className="text-sm font-medium">
                    {connections?.find((c) => c.id === sourceId)?.name ?? "—"}
                  </span>
                </div>
                <div className="flex justify-between">
                  <span className="text-sm text-muted-foreground">Objects</span>
                  <span className="text-sm font-medium">{selectedObjects.length} selected</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-sm text-muted-foreground">Job Name</span>
                  <span className="text-sm font-medium">{jobName || "De-identification Job"}</span>
                </div>
              </div>
              {selectedObjects.length > 0 && (
                <div className="flex flex-wrap gap-2">
                  {selectedObjects.map((obj) => (
                    <Badge key={obj} variant="secondary">{obj}</Badge>
                  ))}
                </div>
              )}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Navigation */}
      <div className="flex justify-between">
        <Button
          variant="outline"
          onClick={handleBack}
          disabled={currentIdx === 0}
        >
          <ArrowLeft className="h-4 w-4" /> Back
        </Button>
        {step === "review" ? (
          <Button onClick={handleRun} disabled={createJobMutation.isPending}>
            {createJobMutation.isPending ? "Starting..." : "Run De-identification"}
          </Button>
        ) : (
          <Button
            onClick={handleNext}
            disabled={step === "source" && !sourceId}
          >
            Next <ArrowRight className="h-4 w-4" />
          </Button>
        )}
      </div>
    </div>
  );
}
