import { useState, useMemo } from "react";
import { useLocation } from "wouter";
import { 
  ArrowLeft, 
  GitMerge, 
  Link2, 
  Shield, 
  RefreshCw, 
  Database,
  Plus, 
  Trash2
} from "lucide-react";

import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select } from "@/components/ui/select"; // This is a native select wrapper

// Hooks
import { 
  useConnections, 
  useConnectionMetadata, 
  useRefreshMetadata, 
  useCreateConfiguration 
} from "@/hooks/useApi";

// Constants
const DEIDENTIFICATION_STRATEGIES = [
  { value: "mask_all", label: "Mask All", description: "Replace with *****" },
  { value: "mask_partial_email", label: "Mask Email", description: "j***@domain.com" },
  { value: "mask_partial_phone", label: "Mask Phone", description: "***-***-1234" },
  { value: "hashing_sha256", label: "SHA-256 Hash", description: "One-way hash" },
  { value: "encryption_aes", label: "AES Encryption", description: "Reversible encryption" },
  { value: "nullify", label: "Nullify", description: "Set to NULL" },
  { value: "perturbation_date", label: "Date Shift", description: "Add random offset to dates" },
  { value: "perturbation_numeric", label: "Numeric Noise", description: "Add random noise to numbers" },
  { value: "generalization_age", label: "Age Bucketing", description: "18-24, 25-34, etc." },
];

interface ColumnStrategy {
  columnName: string;
  strategy: string;
}

interface ConsistencyMapping {
  id: number;
  determinantColumn: string;
  dependentColumn: string;
}

interface IntegrityMapping {
  id: number;
  sourceColumn: string;
  targetColumn: string;
}

interface ColumnDef {
  name: string;
  type: string;
  nullable: string;
}

export function ConfigurationFormPage() {
  const [, setLocation] = useLocation();
  
  // Data
  const { data: connections } = useConnections();
  const refreshMetadata = useRefreshMetadata();
  const createConfiguration = useCreateConfiguration();

  // State
  const [configType, setConfigType] = useState<"consistency" | "integrity" | "deidentify">("consistency");
  const [ruleName, setRuleName] = useState("");
  const [description, setDescription] = useState("");

  // Consistency State
  const [consistencyConn, setConsistencyConn] = useState("");
  const [consistencyObj, setConsistencyObj] = useState("");
  const [consistencyMappings, setConsistencyMappings] = useState<ConsistencyMapping[]>([
    { id: 1, determinantColumn: "", dependentColumn: "" }
  ]);

  // Integrity State
  const [integritySourceConn, setIntegritySourceConn] = useState("");
  const [integritySourceObj, setIntegritySourceObj] = useState("");
  const [integrityTargetConn, setIntegrityTargetConn] = useState("");
  const [integrityTargetObj, setIntegrityTargetObj] = useState("");
  const [integrityMappings, setIntegrityMappings] = useState<IntegrityMapping[]>([
    { id: 1, sourceColumn: "", targetColumn: "" }
  ]);

  // De-identification State
  const [deidConn, setDeidConn] = useState("");
  const [deidObj, setDeidObj] = useState("");
  const [columnStrategies, setColumnStrategies] = useState<ColumnStrategy[]>([
    { columnName: "", strategy: "" }
  ]);

  // Helper to extract columns from metadata
  const getColumns = (metadataList: any[] = [], objName: string): string[] => {
    if (!objName) return [];
    const meta = metadataList?.find(m => m.objectName === objName);
    if (!meta || !meta.columns) return [];
    // Assuming columns is an array of objects from backend
    if (Array.isArray(meta.columns)) {
      return meta.columns.map((c: ColumnDef) => c.name);
    }
    return [];
  };

  // Metadata Queries
  const { data: consistencyMeta } = useConnectionMetadata(consistencyConn);
  const { data: integritySourceMeta } = useConnectionMetadata(integritySourceConn);
  const { data: integrityTargetMeta } = useConnectionMetadata(integrityTargetConn);
  const { data: deidMeta } = useConnectionMetadata(deidConn);

  // Derived lists
  const consistencyColumns = useMemo(() => getColumns(consistencyMeta, consistencyObj), [consistencyMeta, consistencyObj]);
  const integritySourceColumns = useMemo(() => getColumns(integritySourceMeta, integritySourceObj), [integritySourceMeta, integritySourceObj]);
  const integrityTargetColumns = useMemo(() => getColumns(integrityTargetMeta, integrityTargetObj), [integrityTargetMeta, integrityTargetObj]);
  const deidColumns = useMemo(() => getColumns(deidMeta, deidObj), [deidMeta, deidObj]);

  // Handlers
  const handleRefreshMetadata = async (connId: string) => {
    if (!connId) return;
    try {
      await refreshMetadata.mutateAsync(connId);
      alert("Schema refreshed successfully");
    } catch {
      alert("Failed to refresh schema");
    }
  };

  const addConsistencyMapping = () => {
    setConsistencyMappings([...consistencyMappings, { id: Date.now(), determinantColumn: "", dependentColumn: "" }]);
  };
  const removeConsistencyMapping = (id: number) => {
    setConsistencyMappings(consistencyMappings.filter(m => m.id !== id));
  };
  const updateConsistencyMapping = (id: number, field: keyof ConsistencyMapping, value: string) => {
    setConsistencyMappings(consistencyMappings.map(m => m.id === id ? { ...m, [field]: value } : m));
  };

  const addIntegrityMapping = () => {
    setIntegrityMappings([...integrityMappings, { id: Date.now(), sourceColumn: "", targetColumn: "" }]);
  };
  const removeIntegrityMapping = (id: number) => {
    setIntegrityMappings(integrityMappings.filter(m => m.id !== id));
  };
  const updateIntegrityMapping = (id: number, field: keyof IntegrityMapping, value: string) => {
    setIntegrityMappings(integrityMappings.map(m => m.id === id ? { ...m, [field]: value } : m));
  };

  const addColumnStrategy = () => {
    setColumnStrategies([...columnStrategies, { columnName: "", strategy: "" }]);
  };
  const removeColumnStrategy = (index: number) => {
    const newStrategies = [...columnStrategies];
    newStrategies.splice(index, 1);
    setColumnStrategies(newStrategies);
  };
  const updateColumnStrategy = (index: number, field: keyof ColumnStrategy, value: string) => {
    const newStrategies = [...columnStrategies];
    newStrategies[index] = { ...newStrategies[index], [field]: value };
    setColumnStrategies(newStrategies);
  };

  const handleSubmit = async () => {
    if (!ruleName.trim()) {
      alert("Rule Name is required");
      return;
    }

    try {
      if (configType === 'consistency') {
        if (!consistencyConn || !consistencyObj) throw new Error("Connection and Object are required");
        const validMappings = consistencyMappings.filter(m => m.determinantColumn && m.dependentColumn);
        if (validMappings.length === 0) throw new Error("At least one valid mapping is required");

        await createConfiguration.mutateAsync({
          name: ruleName,
          type: "internal_consistency",
          description,
          connectionId: consistencyConn,
          objectName: consistencyObj,
          rules: {
            mappings: validMappings.map(m => ({
              determinantColumn: m.determinantColumn,
              dependentColumn: m.dependentColumn
            }))
          }
        });
      } else if (configType === 'integrity') {
        if (!integritySourceConn || !integritySourceObj || !integrityTargetConn || !integrityTargetObj) 
          throw new Error("Source/Target Connection and Object are required");
        
        const validMappings = integrityMappings.filter(m => m.sourceColumn && m.targetColumn);
        if (validMappings.length === 0) throw new Error("At least one valid mapping is required");

        await createConfiguration.mutateAsync({
            name: ruleName,
            type: "referential_integrity",
            description,
            connectionId: integritySourceConn, // treating source as primary
            objectName: integritySourceObj,
            rules: {
                targetConnectionId: integrityTargetConn,
                targetObjectName: integrityTargetObj,
                mappings: validMappings.map(m => ({
                    sourceColumn: m.sourceColumn,
                    targetColumn: m.targetColumn
                }))
            }
        });
      } else if (configType === 'deidentify') {
        if (!deidConn || !deidObj) throw new Error("Connection and Object are required");
        const validStrategies = columnStrategies.filter(s => s.columnName && s.strategy);
        if (validStrategies.length === 0) throw new Error("At least one valid strategy is required");

        await createConfiguration.mutateAsync({
            name: ruleName,
            type: "deidentification",
            description,
            connectionId: deidConn,
            objectName: deidObj,
            rules: {
                columnStrategies: validStrategies
            }
        });
      }
      
      alert("Configuration created successfully");
      setLocation("/configurations");
    } catch (err: any) {
        alert(err.message || "Failed to create configuration");
    }
  };

  const connList = connections || [];

  return (
    <div className="container mx-auto py-8">
      <div className="flex items-center gap-4 mb-6">
        <Button variant="ghost" size="icon" onClick={() => setLocation("/configurations")}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h2 className="text-2xl font-bold tracking-tight">New Configuration</h2>
          <p className="text-muted-foreground">Create a new consistency or integrity rule.</p>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {/* Left Panel: Scope Selection */}
        <Card className="md:col-span-1 h-fit">
          <CardContent className="pt-6 space-y-6">
            <div className="space-y-3">
              <Label className="text-base font-semibold">1. Configuration Type</Label>
              <div className="grid gap-2">
                <div 
                  className={`flex items-center gap-3 p-3 rounded-md border cursor-pointer transition-colors ${configType === 'consistency' ? 'bg-orange-50 border-orange-200 ring-1 ring-orange-200 dark:bg-orange-900/20 dark:border-orange-800' : 'hover:bg-muted'}`}
                  onClick={() => setConfigType('consistency')}
                >
                  <GitMerge className="h-5 w-5 text-orange-600" />
                  <div>
                    <div className="text-sm font-medium">Internal Consistency</div>
                    <div className="text-xs text-muted-foreground">Rules within one object</div>
                  </div>
                </div>
                <div 
                  className={`flex items-center gap-3 p-3 rounded-md border cursor-pointer transition-colors ${configType === 'integrity' ? 'bg-blue-50 border-blue-200 ring-1 ring-blue-200 dark:bg-blue-900/20 dark:border-blue-800' : 'hover:bg-muted'}`}
                  onClick={() => setConfigType('integrity')}
                >
                  <Link2 className="h-5 w-5 text-blue-600" />
                  <div>
                    <div className="text-sm font-medium">Referential Integrity</div>
                    <div className="text-xs text-muted-foreground">Rules across multiple objects</div>
                  </div>
                </div>
                <div 
                  className={`flex items-center gap-3 p-3 rounded-md border cursor-pointer transition-colors ${configType === 'deidentify' ? 'bg-purple-50 border-purple-200 ring-1 ring-purple-200 dark:bg-purple-900/20 dark:border-purple-800' : 'hover:bg-muted'}`}
                  onClick={() => setConfigType('deidentify')}
                >
                  <Shield className="h-5 w-5 text-purple-600" />
                  <div>
                    <div className="text-sm font-medium">De-identification</div>
                    <div className="text-xs text-muted-foreground">Column anonymization strategies</div>
                  </div>
                </div>
              </div>
            </div>

            <div className="space-y-4 pt-4 border-t">
              <Label className="text-base font-semibold">2. Select Scope</Label>

              {/* Consistency Scope */}
              {configType === 'consistency' && (
                <div className="space-y-4 animate-in fade-in">
                  <div className="space-y-2">
                    <div className="flex items-center justify-between">
                        <Label>Connection</Label>
                        {consistencyConn && (
                            <Button variant="ghost" size="sm" className="h-6 px-2 text-xs" onClick={() => handleRefreshMetadata(consistencyConn)} disabled={refreshMetadata.isPending}>
                                <RefreshCw className={`w-3 h-3 mr-1 ${refreshMetadata.isPending ? 'animate-spin' : ''}`} /> Refresh
                            </Button>
                        )}
                    </div>
                    <Select value={consistencyConn} onChange={(e) => setConsistencyConn(e.target.value)}>
                      <option value="">Select Source</option>
                      {connList.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label>Object (Table)</Label>
                    <Select value={consistencyObj} onChange={(e) => setConsistencyObj(e.target.value)} disabled={!consistencyConn}>
                        <option value="">Select Object</option>
                        {GetObjectsItems(consistencyMeta)}
                    </Select>
                  </div>
                </div>
              )}

              {/* Integrity Scope */}
              {configType === 'integrity' && (
                  <div className="space-y-6 animate-in fade-in">
                      <div className="space-y-3 p-3 bg-muted/30 rounded-lg border">
                          <Label className="text-xs font-semibold uppercase text-muted-foreground">Source Object</Label>
                          <div className="space-y-2">
                              <Select value={integritySourceConn} onChange={(e) => setIntegritySourceConn(e.target.value)}>
                                  <option value="">Connection</option>
                                  {connList.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                              </Select>
                          </div>
                          <div className="space-y-2">
                              <Select value={integritySourceObj} onChange={(e) => setIntegritySourceObj(e.target.value)} disabled={!integritySourceConn}>
                                  <option value="">Object</option>
                                  {GetObjectsItems(integritySourceMeta)}
                              </Select>
                          </div>
                      </div>
                      <div className="space-y-3 p-3 bg-muted/30 rounded-lg border">
                          <Label className="text-xs font-semibold uppercase text-muted-foreground">Target Object</Label>
                          <div className="space-y-2">
                              <Select value={integrityTargetConn} onChange={(e) => setIntegrityTargetConn(e.target.value)}>
                                  <option value="">Connection</option>
                                  {connList.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                              </Select>
                          </div>
                          <div className="space-y-2">
                              <Select value={integrityTargetObj} onChange={(e) => setIntegrityTargetObj(e.target.value)} disabled={!integrityTargetConn}>
                                  <option value="">Object</option>
                                  {GetObjectsItems(integrityTargetMeta)}
                              </Select>
                          </div>
                      </div>
                  </div>
              )}

              {/* Deidentify Scope */}
              {configType === 'deidentify' && (
                <div className="space-y-4 animate-in fade-in">
                  <div className="space-y-2">
                     <div className="flex items-center justify-between">
                         <Label>Connection</Label>
                         {deidConn && (
                             <Button variant="ghost" size="sm" className="h-6 px-2 text-xs" onClick={() => handleRefreshMetadata(deidConn)} disabled={refreshMetadata.isPending}>
                                 <RefreshCw className={`w-3 h-3 mr-1 ${refreshMetadata.isPending ? 'animate-spin' : ''}`} /> Refresh
                             </Button>
                         )}
                     </div>
                     <Select value={deidConn} onChange={(e) => setDeidConn(e.target.value)}>
                       <option value="">Select Source</option>
                       {connList.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                     </Select>
                  </div>
                  <div className="space-y-2">
                    <Label>Object (Table)</Label>
                    <Select value={deidObj} onChange={(e) => setDeidObj(e.target.value)} disabled={!deidConn}>
                        <option value="">Select Object</option>
                        {GetObjectsItems(deidMeta)}
                    </Select>
                  </div>
                </div>
              )}

            </div>
          </CardContent>
        </Card>

        {/* Right Panel: Rule Configuration */}
        <Card className="md:col-span-2">
            <CardContent className="pt-6 space-y-6">
                <div className="space-y-2">
                    <Label>{configType === 'deidentify' ? 'Preference Name' : 'Rule Name'}</Label>
                    <Input 
                        placeholder="e.g. My Rule" 
                        value={ruleName} 
                        onChange={e => setRuleName(e.target.value)} 
                    />
                </div>
                <div className="space-y-2">
                    <Label>Description (Optional)</Label>
                    <Input 
                        placeholder="Brief description..." 
                        value={description} 
                        onChange={e => setDescription(e.target.value)} 
                    />
                </div>

                {/* Validation before showing rules */}
                {((configType === 'consistency' && !consistencyObj) || 
                  (configType === 'integrity' && (!integritySourceObj || !integrityTargetObj)) ||
                  (configType === 'deidentify' && !deidObj)) ? (
                    <div className="flex flex-col items-center justify-center py-12 text-center text-muted-foreground border-2 border-dashed rounded-lg bg-muted/5">
                        <Database className="h-8 w-8 mb-2 opacity-50" />
                        <p>Select the configuration scope to continue.</p>
                    </div>
                ) : (
                    <div className="space-y-6 animate-in fade-in">
                        {/* Consistency Rules */}
                        {configType === 'consistency' && (
                            <div className="space-y-4">
                                <div className="p-4 bg-orange-50 dark:bg-orange-900/10 border border-orange-100 dark:border-orange-900/20 rounded-lg text-sm text-orange-800 dark:text-orange-300">
                                    <strong>Internal Consistency:</strong> Determines if column A determines column B.
                                </div>
                                <div className="flex justify-between items-center">
                                    <h4 className="text-sm font-medium">Column Mappings</h4>
                                    <Button variant="outline" size="sm" onClick={addConsistencyMapping} className="gap-2">
                                        <Plus className="h-3 w-3" /> Add Mapping
                                    </Button>
                                </div>
                                {consistencyMappings.map((m) => (
                                    <div key={m.id} className="grid grid-cols-[1fr_1fr_auto] gap-4 items-end p-4 border rounded-lg bg-muted/20">
                                        <div className="space-y-2">
                                            <Label>Determinant (Group By)</Label>
                                            <Select value={m.determinantColumn} onChange={e => updateConsistencyMapping(m.id, 'determinantColumn', e.target.value)}>
                                                <option value=""></option>
                                                {consistencyColumns.map(c => <option key={c} value={c}>{c}</option>)}
                                            </Select>
                                        </div>
                                        <div className="space-y-2">
                                            <Label>Dependent (Check)</Label>
                                            <Select value={m.dependentColumn} onChange={e => updateConsistencyMapping(m.id, 'dependentColumn', e.target.value)}>
                                                <option value=""></option>
                                                {consistencyColumns.map(c => <option key={c} value={c}>{c}</option>)}
                                            </Select>
                                        </div>
                                        <Button variant="ghost" size="icon" onClick={() => removeConsistencyMapping(m.id)}><Trash2 className="h-4 w-4 text-destructive"/></Button>
                                    </div>
                                ))}
                            </div>
                        )}

                        {/* Integrity Rules */}
                        {configType === 'integrity' && (
                            <div className="space-y-4">
                                <div className="p-4 bg-blue-50 dark:bg-blue-900/10 border border-blue-100 dark:border-blue-900/20 rounded-lg text-sm text-blue-800 dark:text-blue-300">
                                    <strong>Referential Integrity:</strong> Ensures values in Source exist in Target.
                                </div>
                                <div className="flex justify-between items-center">
                                    <h4 className="text-sm font-medium">Column Mappings</h4>
                                    <Button variant="outline" size="sm" onClick={addIntegrityMapping} className="gap-2">
                                        <Plus className="h-3 w-3" /> Add Mapping
                                    </Button>
                                </div>
                                {integrityMappings.map((m) => (
                                    <div key={m.id} className="grid grid-cols-[1fr_1fr_auto] gap-4 items-end p-4 border rounded-lg bg-muted/20">
                                        <div className="space-y-2">
                                            <Label>Source Column</Label>
                                            <Select value={m.sourceColumn} onChange={e => updateIntegrityMapping(m.id, 'sourceColumn', e.target.value)}>
                                                <option value=""></option>
                                                {integritySourceColumns.map(c => <option key={c} value={c}>{c}</option>)}
                                            </Select>
                                        </div>
                                        <div className="space-y-2">
                                            <Label>Target Column</Label>
                                            <Select value={m.targetColumn} onChange={e => updateIntegrityMapping(m.id, 'targetColumn', e.target.value)}>
                                                <option value=""></option>
                                                {integrityTargetColumns.map(c => <option key={c} value={c}>{c}</option>)}
                                            </Select>
                                        </div>
                                        <Button variant="ghost" size="icon" onClick={() => removeIntegrityMapping(m.id)}><Trash2 className="h-4 w-4 text-destructive"/></Button>
                                    </div>
                                ))}
                            </div>
                        )}

                        {/* De-id Rules */}
                        {configType === 'deidentify' && (
                            <div className="space-y-4">
                                <div className="p-4 bg-purple-50 dark:bg-purple-900/10 border border-purple-100 dark:border-purple-900/20 rounded-lg text-sm text-purple-800 dark:text-purple-300">
                                    <strong>De-identification:</strong> Apply masking or encryption to columns.
                                </div>
                                <div className="flex justify-between items-center">
                                    <h4 className="text-sm font-medium">Applied Strategies</h4>
                                    <Button variant="outline" size="sm" onClick={addColumnStrategy} className="gap-2">
                                        <Plus className="h-3 w-3" /> Add Strategy
                                    </Button>
                                </div>
                                {columnStrategies.map((s, idx) => (
                                    <div key={idx} className="grid grid-cols-[1fr_1fr_auto] gap-4 items-end p-4 border rounded-lg bg-muted/20">
                                        <div className="space-y-2">
                                            <Label>Column</Label>
                                            <Select value={s.columnName} onChange={e => updateColumnStrategy(idx, 'columnName', e.target.value)}>
                                                <option value=""></option>
                                                {deidColumns.map(c => <option key={c} value={c}>{c}</option>)}
                                            </Select>
                                        </div>
                                        <div className="space-y-2">
                                            <Label>Strategy</Label>
                                            <Select value={s.strategy} onChange={e => updateColumnStrategy(idx, 'strategy', e.target.value)}>
                                                <option value=""></option>
                                                {DEIDENTIFICATION_STRATEGIES.map(ds => (
                                                    <option key={ds.value} value={ds.value}>
                                                        {ds.label} - {ds.description}
                                                    </option>
                                                ))}
                                            </Select>
                                        </div>
                                        <Button variant="ghost" size="icon" onClick={() => removeColumnStrategy(idx)}><Trash2 className="h-4 w-4 text-destructive"/></Button>
                                    </div>
                                ))}
                            </div>
                        )}
                        
                        <div className="flex justify-end pt-6 border-t mt-6">
                            <Button size="lg" onClick={handleSubmit} disabled={createConfiguration.isPending}>
                                {createConfiguration.isPending ? "Creating..." : "Create Configuration"}
                            </Button>
                        </div>
                    </div>
                )}
            </CardContent>
        </Card>
      </div>
    </div>
  );
}

function GetObjectsItems(meta: any[] | undefined) {
    if (!meta) return null;
    return meta.map((m: any) => (
        <option key={m.objectName} value={m.objectName}>
            {m.objectName}
        </option>
    ));
}
