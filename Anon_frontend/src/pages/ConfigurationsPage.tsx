import { useState } from "react";
import { Link, useLocation } from "wouter";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Plus, Network, GitMerge, Table as TableIcon, Loader2, Shield, Pencil } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { useConfigurations, useDeleteConfiguration, useConnections } from "@/hooks/useApi";
import { useToast } from "@/hooks/use-toast";

interface ColumnStrategy {
  columnName: string;
  strategy: string;
}

export function ConfigurationsPage() {
  const { data: configurations, isLoading } = useConfigurations();
  const { data: connections } = useConnections();
  const deleteConfiguration = useDeleteConfiguration();
  const { toast } = useToast();
  const [, navigate] = useLocation();
  
  const [activeTab, setActiveTab] = useState("deidentify");
  
  // Filter configurations by type
  const internalConsistencyRules = (configurations || []).filter(c => c.type === "internal_consistency");
  const referentialIntegrityRules = (configurations || []).filter(c => c.type === "referential_integrity");
  const deidentificationPrefs = (configurations || []).filter(c => c.type === "deidentification");

  const handleDeleteConfiguration = (id: string) => {
    deleteConfiguration.mutate(id, {
      onSuccess: () => {
        toast({ title: "Deleted", description: "Configuration removed successfully." });
      },
      onError: (error) => {
        toast({
          variant: "destructive",
          title: "Error",
          description: error instanceof Error ? error.message : "Failed to delete configuration.",
        });
      },
    });
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Configurations</h1>
          <p className="text-muted-foreground mt-1">Define relationships and integrity rules between data objects.</p>
        </div>
        <Link href="/configurations/new">
          <Button className="gap-2">
            <Plus className="h-4 w-4" /> New Configuration
          </Button>
        </Link>
      </div>

      <Tabs value={activeTab} onValueChange={setActiveTab} className="w-full">
        <TabsList className="grid w-full max-w-lg grid-cols-3">
          <TabsTrigger value="deidentify">De-identification</TabsTrigger>
          <TabsTrigger value="consistency">Internal Consistency</TabsTrigger>
          <TabsTrigger value="integrity">Referential Integrity</TabsTrigger>
        </TabsList>
        
        <TabsContent value="deidentify" className="mt-6 space-y-6">
          <div className="flex items-center justify-between">
             <div>
                <h3 className="text-lg font-medium">De-identification Preferences</h3>
                <p className="text-sm text-muted-foreground">Define column-level strategies for data anonymization.</p>
             </div>
          </div>

          {isLoading ? (
            <div className="flex items-center justify-center p-12">
              <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
            </div>
          ) : deidentificationPrefs.length === 0 ? (
            <Card className="border-dashed">
              <CardContent className="flex flex-col items-center justify-center p-12 text-center">
                <div className="h-12 w-12 rounded-full bg-muted flex items-center justify-center mb-4">
                  <Shield className="h-6 w-6 text-muted-foreground" />
                </div>
                <h3 className="font-semibold mb-2">No de-identification preferences yet</h3>
                <p className="text-sm text-muted-foreground mb-4">Define how each column should be anonymized or transformed.</p>
                <Link href="/configurations/new">
                  <Button variant="outline" className="gap-2">
                    <Plus className="h-4 w-4" /> Create Your First Preference
                  </Button>
                </Link>
              </CardContent>
            </Card>
          ) : (
            <div className="grid gap-4">
              {deidentificationPrefs.map((pref) => {
                const rules = pref.rules as unknown as { columnStrategies?: ColumnStrategy[] } | null;
                const strategies = rules?.columnStrategies || [];
                const connectionName = connections?.find(c => c.id === pref.connectionId)?.name || `Connection ${pref.connectionId || ''}`;
                
                return (
                  <Card key={pref.id} className="overflow-hidden border-l-4 border-l-purple-500">
                    <CardContent className="p-0">
                      <div className="flex items-center justify-between p-6">
                        <div className="flex items-start gap-4">
                          <div className="p-2 bg-purple-100 dark:bg-purple-900/20 rounded-lg text-purple-600 mt-1">
                            <Shield className="h-5 w-5" />
                          </div>
                          <div className="space-y-1">
                            <div className="flex items-center gap-2">
                              <h4 className="font-semibold">{pref.name}</h4>
                              <Badge variant="outline" className="text-xs font-normal bg-muted">deidentification</Badge>
                            </div>
                            <div className="flex items-center gap-2 text-sm text-muted-foreground">
                              <span className="flex items-center gap-1">
                                <Network className="h-3 w-3" /> {connectionName}
                              </span>
                              <span>•</span>
                              <span className="flex items-center gap-1 font-mono text-xs bg-muted px-1.5 py-0.5 rounded">
                                <TableIcon className="h-3 w-3" /> {pref.objectName}
                              </span>
                            </div>
                            <p className="text-sm pt-1">
                              {strategies.map(s => `${s.columnName}: ${s.strategy}`).join(", ")}
                            </p>
                          </div>
                        </div>
                        <div className="flex items-center gap-2">
                          <Button 
                            variant="ghost" 
                            size="sm"
                            onClick={() => navigate(`/configurations/${pref.id}/edit`)}
                          >
                            <Pencil className="h-4 w-4 mr-1" /> Edit
                          </Button>
                          <Button 
                            variant="ghost" 
                            size="sm"
                            onClick={() => handleDeleteConfiguration(pref.id)}
                            className="text-destructive hover:text-destructive"
                          >
                            Delete
                          </Button>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                );
              })}
            </div>
          )}
        </TabsContent>
        
        <TabsContent value="consistency" className="mt-6 space-y-6">
          <div className="flex items-center justify-between">
             <div>
                <h3 className="text-lg font-medium">Object-Column Relations</h3>
                <p className="text-sm text-muted-foreground">Rules to maintain data consistency within a single object.</p>
             </div>
          </div>

          {isLoading ? (
            <div className="flex items-center justify-center p-12">
              <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
            </div>
          ) : internalConsistencyRules.length === 0 ? (
            <Card className="border-dashed">
              <CardContent className="flex flex-col items-center justify-center p-12 text-center">
                <div className="h-12 w-12 rounded-full bg-muted flex items-center justify-center mb-4">
                  <GitMerge className="h-6 w-6 text-muted-foreground" />
                </div>
                <h3 className="font-semibold mb-2">No internal consistency rules yet</h3>
                <p className="text-sm text-muted-foreground mb-4">Define functional dependencies to maintain data consistency within objects.</p>
                <Link href="/configurations/new">
                  <Button variant="outline" className="gap-2">
                    <Plus className="h-4 w-4" /> Create Your First Rule
                  </Button>
                </Link>
              </CardContent>
            </Card>
          ) : (
            <div className="grid gap-4">
              {internalConsistencyRules.map((rule) => {
                 const connectionName = connections?.find(c => c.id === rule.connectionId)?.name || `Connection ${rule.connectionId || ''}`;
                 return (
                  <Card key={rule.id} className="overflow-hidden border-l-4 border-l-orange-500">
                    <CardContent className="p-0">
                      <div className="flex items-center justify-between p-6">
                        <div className="flex items-start gap-4">
                          <div className="p-2 bg-orange-100 dark:bg-orange-900/20 rounded-lg text-orange-600 mt-1">
                            <GitMerge className="h-5 w-5" />
                          </div>
                          <div className="space-y-1">
                            <div className="flex items-center gap-2">
                              <h4 className="font-semibold">{rule.name}</h4>
                              <Badge variant="outline" className="text-xs font-normal bg-muted">internal_consistency</Badge>
                            </div>
                            <div className="flex items-center gap-2 text-sm text-muted-foreground">
                               <span className="flex items-center gap-1">
                                <Network className="h-3 w-3" /> {connectionName}
                              </span>
                              <span>•</span>
                              <span className="flex items-center gap-1 font-mono text-xs bg-muted px-1.5 py-0.5 rounded">
                                <TableIcon className="h-3 w-3" /> {rule.objectName}
                              </span>
                            </div>
                             {rule.description && (
                              <p className="text-sm pt-1 text-muted-foreground">{rule.description}</p>
                            )}
                          </div>
                        </div>
                        <div className="flex items-center gap-2">
                          <Button 
                            variant="ghost" 
                            size="sm"
                            onClick={() => navigate(`/configurations/${rule.id}/edit`)}
                          >
                            <Pencil className="h-4 w-4 mr-1" /> Edit
                          </Button>
                          <Button 
                            variant="ghost" 
                            size="sm"
                            onClick={() => handleDeleteConfiguration(rule.id)}
                            className="text-destructive hover:text-destructive"
                          >
                            Delete
                          </Button>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                );
              })}
            </div>
          )}
        </TabsContent>

        <TabsContent value="integrity" className="mt-6 space-y-6">
          <div className="flex items-center justify-between">
             <div>
                <h3 className="text-lg font-medium">Referential Integrity</h3>
                <p className="text-sm text-muted-foreground">Rules to maintain relationships between different objects.</p>
             </div>
          </div>

          {isLoading ? (
             <div className="flex items-center justify-center p-12">
              <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
            </div>
          ) : referentialIntegrityRules.length === 0 ? (
             <Card className="border-dashed">
              <CardContent className="flex flex-col items-center justify-center p-12 text-center">
                <div className="h-12 w-12 rounded-full bg-muted flex items-center justify-center mb-4">
                  <Network className="h-6 w-6 text-muted-foreground" />
                </div>
                <h3 className="font-semibold mb-2">No referential integrity rules yet</h3>
                <p className="text-sm text-muted-foreground mb-4">Define foreign keys and relationships to maintain data integrity across objects.</p>
                <Link href="/configurations/new">
                  <Button variant="outline" className="gap-2">
                    <Plus className="h-4 w-4" /> Create Your First Rule
                  </Button>
                </Link>
              </CardContent>
            </Card>
          ) : (
            <div className="grid gap-4">
               {referentialIntegrityRules.map((rule) => {
                 const sourceConnName = connections?.find(c => c.id === rule.sourceConnectionId)?.name || `Connection ${rule.sourceConnectionId || ''}`;
                 const targetConnName = connections?.find(c => c.id === rule.targetConnectionId)?.name || `Connection ${rule.targetConnectionId || ''}`;
                 
                 return (
                  <Card key={rule.id} className="overflow-hidden border-l-4 border-l-blue-500">
                    <CardContent className="p-0">
                      <div className="flex items-center justify-between p-6">
                        <div className="flex items-start gap-4">
                          <div className="p-2 bg-blue-100 dark:bg-blue-900/20 rounded-lg text-blue-600 mt-1">
                            <Network className="h-5 w-5" />
                          </div>
                          <div className="space-y-1">
                            <div className="flex items-center gap-2">
                              <h4 className="font-semibold">{rule.name}</h4>
                              <Badge variant="outline" className="text-xs font-normal bg-muted">referential_integrity</Badge>
                            </div>
                            <div className="flex flex-col gap-1 text-sm text-muted-foreground">
                              <div className="flex items-center gap-2">
                                <span className="font-semibold text-xs uppercase tracking-wider w-12">Source</span>
                                <span className="flex items-center gap-1">
                                  {sourceConnName}
                                </span>
                                <span>•</span>
                                <span className="flex items-center gap-1 font-mono text-xs bg-muted px-1.5 py-0.5 rounded">
                                  {rule.sourceObjectName}
                                </span>
                              </div>
                              <div className="flex items-center gap-2">
                                <span className="font-semibold text-xs uppercase tracking-wider w-12">Target</span>
                                <span className="flex items-center gap-1">
                                  {targetConnName}
                                </span>
                                <span>•</span>
                                <span className="flex items-center gap-1 font-mono text-xs bg-muted px-1.5 py-0.5 rounded">
                                  {rule.targetObjectName}
                                </span>
                              </div>
                            </div>
                          </div>
                        </div>
                        <div className="flex items-center gap-2">
                          <Button 
                            variant="ghost" 
                            size="sm"
                            onClick={() => navigate(`/configurations/${rule.id}/edit`)}
                          >
                            <Pencil className="h-4 w-4 mr-1" /> Edit
                          </Button>
                          <Button 
                            variant="ghost" 
                            size="sm"
                            onClick={() => handleDeleteConfiguration(rule.id)}
                            className="text-destructive hover:text-destructive"
                          >
                            Delete
                          </Button>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                );
              })}
            </div>
          )}
        </TabsContent>
      </Tabs>
    </div>
  );
}
