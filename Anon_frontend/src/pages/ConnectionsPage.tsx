import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { useConnections, useCreateConnection, useDeleteConnection, useTestConnection, useTestConnectionConfig } from "@/hooks/useApi";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Form, FormControl, FormDescription, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { Database, Plus, Trash2, Zap, Cloud, Server, HardDrive, CheckCircle2, XCircle, Loader2 } from "lucide-react";
import { formatDate } from "@/lib/utils";

// --- Zod Schemas ---

const commonSchema = z.object({
  name: z.string().min(2, "Name must be at least 2 characters"),
});

const s3ConfigSchema = z.object({
  bucket: z.string().min(1, "Bucket is required"),
  region: z.string().min(1, "Region is required"),
  accessKeyId: z.string().min(1, "Access Key ID is required"),
  secretAccessKey: z.string().min(1, "Secret Access Key is required"),
});

const databaseConfigSchema = z.object({
  host: z.string().min(1, "Host is required"),
  port: z.union([z.string(), z.number()]).transform((val) => Number(val)),
  database: z.string().min(1, "Database name is required"),
  user: z.string().min(1, "User is required"),
  password: z.string().optional(),
});

const snowflakeConfigSchema = z.object({
  account: z.string().min(1, "Account identifier is required"),
  warehouse: z.string().default("COMPUTE_WH"),
  database: z.string().min(1, "Database is required"),
  schema: z.string().default("PUBLIC"),
  user: z.string().min(1, "User is required"),
  password: z.string().min(1, "Password is required"),
  role: z.string().optional(),
});

const azureBlobConfigSchema = z.object({
  connectionString: z.string().min(1, "Connection string is required"),
  containerName: z.string().min(1, "Container name is required"),
});


export function ConnectionsPage() {
  const { data: connections, isLoading } = useConnections();
  const createMutation = useCreateConnection();
  const deleteMutation = useDeleteConnection();
  const testMutation = useTestConnection();
  const testConfigMutation = useTestConnectionConfig();
  const [showCreate, setShowCreate] = useState(false);
  const [activeTab, setActiveTab] = useState("s3");
  const [testResult, setTestResult] = useState<{ success: boolean; message: string; details?: string } | null>(null);

  const closeDialog = () => {
    setShowCreate(false);
    setTestResult(null);
  };

  const handleTestConnection = (type: string, data: any) => {
    setTestResult(null);
    const { name, ...config } = data;
    testConfigMutation.mutate(
      { type, config },
      {
        onSuccess: (result) => setTestResult(result),
        onError: (error) => setTestResult({ success: false, message: "Test failed", details: error.message }),
      }
    );
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Connections</h1>
          <p className="text-muted-foreground mt-1">Manage your data source connections</p>
        </div>
        <Button onClick={() => setShowCreate(true)}>
          <Plus className="h-4 w-4 mr-2" /> Add Connection
        </Button>
      </div>

      {isLoading ? (
        <p className="text-muted-foreground">Loading connections...</p>
      ) : !connections?.length ? (
        <Card>
          <CardContent className="py-12 text-center">
            <Database className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
            <h3 className="text-lg font-semibold">No connections yet</h3>
            <p className="text-muted-foreground mt-1">Add your first data source connection to get started.</p>
            <Button className="mt-4" onClick={() => setShowCreate(true)}>
              <Plus className="h-4 w-4 mr-2" /> Add Connection
            </Button>
          </CardContent>
        </Card>
      ) : (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {connections.map((conn) => (
            <Card key={conn.id} className="relative group">
              <CardHeader className="flex flex-row items-start justify-between pb-2">
                <div className="space-y-1">
                  <CardTitle className="text-base flex items-center gap-2">
                    {conn.type === 's3' && <Cloud className="h-4 w-4" />}
                    {conn.type === 'database' && <Database className="h-4 w-4" />}
                    {conn.type === 'snowflake' && <Server className="h-4 w-4" />}
                    {conn.type === 'azure_blob' && <HardDrive className="h-4 w-4" />}
                    {conn.name}
                  </CardTitle>
                  <CardDescription className="capitalize">{conn.type.replace('_', ' ')}</CardDescription>
                </div>
                <div className="flex gap-2">
                   <Badge variant={conn.status === "active" ? "outline" : "secondary"}>
                    {conn.status}
                  </Badge>
                </div>
              </CardHeader>
              <CardContent>
                <div className="flex items-center justify-between mt-4">
                  <p className="text-xs text-muted-foreground">
                    Added {formatDate(conn.createdAt)}
                  </p>
                  <div className="flex gap-2">
                     <Button
                      size="sm"
                      variant="outline"
                      onClick={() => testMutation.mutate(conn.id)}
                      disabled={testMutation.isPending}
                      title="Test Connection"
                    >
                      <Zap className="h-3 w-3" />
                    </Button>
                    <Button
                      size="sm"
                      variant="destructive"
                      onClick={() => {
                        if (confirm("Are you sure you want to delete this connection?")) {
                          deleteMutation.mutate(conn.id);
                        }
                      }}
                      disabled={deleteMutation.isPending}
                      title="Delete Connection"
                    >
                      <Trash2 className="h-3 w-3" />
                    </Button>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {/* Create Connection Dialog */}
      <Dialog open={showCreate} onOpenChange={setShowCreate}>
          {showCreate && (
             <div className="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-6 bg-black/50 overflow-y-auto">
               <div className="bg-background border rounded-lg shadow-xl w-full max-w-2xl flex flex-col relative z-50" role="dialog" aria-modal="true">
                  <div className="p-6 border-b flex justify-between items-center">
                    <DialogHeader>
                      <DialogTitle>Add New Connection</DialogTitle>
                    </DialogHeader>
                    <button onClick={closeDialog} className="text-muted-foreground hover:text-foreground">
                      <span className="sr-only">Close</span>
                      <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="lucide lucide-x h-4 w-4"><path d="M18 6 6 18"/><path d="m6 6 12 12"/></svg>
                    </button>
                  </div>
                  <div className="p-6 overflow-y-auto max-h-[80vh]">
                    <Tabs value={activeTab} onValueChange={(value) => { setActiveTab(value); setTestResult(null); }} className="w-full">
                      <TabsList className="grid w-full grid-cols-4 mb-6">
                        <TabsTrigger value="s3">Amazon S3</TabsTrigger>
                        <TabsTrigger value="database">Postgres</TabsTrigger>
                        <TabsTrigger value="snowflake">Snowflake</TabsTrigger>
                        <TabsTrigger value="azure_blob">Azure Blob</TabsTrigger>
                      </TabsList>

                      <TabsContent value="s3">
                        <ConnectionForm
                          type="s3"
                          schema={commonSchema.merge(s3ConfigSchema)}
                          onSubmit={(data) => {
                             const { name, ...config } = data;
                             createMutation.mutate({ name, type: 's3', config }, { onSuccess: closeDialog });
                          }}
                          isSubmitting={createMutation.isPending}
                          onCancel={closeDialog}
                          onTest={(data) => handleTestConnection('s3', data)}
                          isTestingConnection={testConfigMutation.isPending}
                          testResult={testResult}
                        />
                      </TabsContent>

                      <TabsContent value="database">
                        <ConnectionForm
                          type="database"
                          schema={commonSchema.merge(databaseConfigSchema)}
                          onSubmit={(data) => {
                             const { name, ...config } = data;
                             createMutation.mutate({ name, type: 'database', config }, { onSuccess: closeDialog });
                          }}
                          isSubmitting={createMutation.isPending}
                          onCancel={closeDialog}
                          onTest={(data) => handleTestConnection('database', data)}
                          isTestingConnection={testConfigMutation.isPending}
                          testResult={testResult}
                        />
                      </TabsContent>

                      <TabsContent value="snowflake">
                        <ConnectionForm
                          type="snowflake"
                          schema={commonSchema.merge(snowflakeConfigSchema)}
                          onSubmit={(data) => {
                             const { name, ...config } = data;
                             createMutation.mutate({ name, type: 'snowflake', config }, { onSuccess: closeDialog });
                          }}
                          isSubmitting={createMutation.isPending}
                          onCancel={closeDialog}
                          onTest={(data) => handleTestConnection('snowflake', data)}
                          isTestingConnection={testConfigMutation.isPending}
                          testResult={testResult}
                        />
                      </TabsContent>

                      <TabsContent value="azure_blob">
                        <ConnectionForm
                          type="azure_blob"
                          schema={commonSchema.merge(azureBlobConfigSchema)}
                          onSubmit={(data) => {
                             const { name, ...config } = data;
                             createMutation.mutate({ name, type: 'azure_blob', config }, { onSuccess: closeDialog });
                          }}
                          isSubmitting={createMutation.isPending}
                          onCancel={closeDialog}
                          onTest={(data) => handleTestConnection('azure_blob', data)}
                          isTestingConnection={testConfigMutation.isPending}
                          testResult={testResult}
                        />
                      </TabsContent>
                    </Tabs>
                  </div>
               </div>
             </div>
          )}
      </Dialog>
    </div>
  );
}

interface ConnectionFormProps {
  type: string;
  schema: z.ZodType<any, any, any>;
  onSubmit: (data: any) => void;
  isSubmitting: boolean;
  onCancel: () => void;
  onTest: (data: any) => void;
  isTestingConnection: boolean;
  testResult: { success: boolean; message: string; details?: string } | null;
}

function ConnectionForm({ type, schema, onSubmit, isSubmitting, onCancel, onTest, isTestingConnection, testResult }: ConnectionFormProps) {
  const form = useForm({
    resolver: zodResolver(schema),
    defaultValues: {
      name: "",
      port: type === 'database' ? '5432' : undefined,
    },
  });

  const handleTest = () => {
    const values = form.getValues();
    onTest(values);
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="name"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Connection Name</FormLabel>
              <FormControl>
                <Input placeholder="My Production DB" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        {/* Dynamic Fields based on Type */}
        {type === 's3' && (
          <>
            <div className="grid grid-cols-2 gap-4">
              <FormField control={form.control} name="bucket" render={({ field }) => (
                <FormItem>
                  <FormLabel>Bucket Name</FormLabel>
                  <FormControl><Input {...field} /></FormControl><FormMessage />
                </FormItem>
              )} />
              <FormField control={form.control} name="region" render={({ field }) => (
                <FormItem>
                  <FormLabel>Region</FormLabel>
                   <FormControl><Input {...field} placeholder="us-east-1" /></FormControl><FormMessage />
                </FormItem>
              )} />
            </div>
            <FormField control={form.control} name="accessKeyId" render={({ field }) => (
                <FormItem>
                  <FormLabel>Access Key ID</FormLabel>
                  <FormControl><Input type="password" {...field} /></FormControl><FormMessage />
                </FormItem>
            )} />
            <FormField control={form.control} name="secretAccessKey" render={({ field }) => (
                <FormItem>
                  <FormLabel>Secret Access Key</FormLabel>
                  <FormControl><Input type="password" {...field} /></FormControl><FormMessage />
                </FormItem>
            )} />
          </>
        )}

        {type === 'database' && (
           <>
            <div className="grid grid-cols-3 gap-4">
               <FormField control={form.control} name="host" render={({ field }) => (
                <FormItem className="col-span-2">
                  <FormLabel>Host</FormLabel>
                  <FormControl><Input {...field} placeholder="localhost" /></FormControl><FormMessage />
                </FormItem>
              )} />
               <FormField control={form.control} name="port" render={({ field }) => (
                <FormItem>
                  <FormLabel>Port</FormLabel>
                  <FormControl><Input {...field} type="number" /></FormControl><FormMessage />
                </FormItem>
              )} />
            </div>
            <FormField control={form.control} name="database" render={({ field }) => (
                <FormItem>
                  <FormLabel>Database Name</FormLabel>
                  <FormControl><Input {...field} /></FormControl><FormMessage />
                </FormItem>
            )} />
            <div className="grid grid-cols-2 gap-4">
                <FormField control={form.control} name="user" render={({ field }) => (
                    <FormItem>
                    <FormLabel>User</FormLabel>
                    <FormControl><Input {...field} /></FormControl><FormMessage />
                    </FormItem>
                )} />
                <FormField control={form.control} name="password" render={({ field }) => (
                    <FormItem>
                    <FormLabel>Password</FormLabel>
                    <FormControl><Input type="password" {...field} /></FormControl><FormMessage />
                    </FormItem>
                )} />
            </div>
           </>
        )}

        {type === 'snowflake' && (
           <>
            <FormField control={form.control} name="account" render={({ field }) => (
                <FormItem>
                  <FormLabel>Account Identifier</FormLabel>
                  <FormControl><Input {...field} placeholder="xy12345.us-east-1" /></FormControl>
                  <FormDescription>Your Snowflake account locator.</FormDescription>
                  <FormMessage />
                </FormItem>
            )} />
             <div className="grid grid-cols-2 gap-4">
                <FormField control={form.control} name="warehouse" render={({ field }) => (
                    <FormItem>
                    <FormLabel>Warehouse</FormLabel>
                    <FormControl><Input {...field} /></FormControl><FormMessage />
                    </FormItem>
                )} />
                 <FormField control={form.control} name="role" render={({ field }) => (
                    <FormItem>
                    <FormLabel>Role (Optional)</FormLabel>
                    <FormControl><Input {...field} /></FormControl><FormMessage />
                    </FormItem>
                )} />
            </div>
             <div className="grid grid-cols-2 gap-4">
                <FormField control={form.control} name="database" render={({ field }) => (
                    <FormItem>
                    <FormLabel>Database</FormLabel>
                    <FormControl><Input {...field} /></FormControl><FormMessage />
                    </FormItem>
                )} />
                <FormField control={form.control} name="schema" render={({ field }) => (
                    <FormItem>
                    <FormLabel>Schema</FormLabel>
                    <FormControl><Input {...field} /></FormControl><FormMessage />
                    </FormItem>
                )} />
            </div>
             <div className="grid grid-cols-2 gap-4">
                <FormField control={form.control} name="user" render={({ field }) => (
                    <FormItem>
                    <FormLabel>User</FormLabel>
                    <FormControl><Input {...field} /></FormControl><FormMessage />
                    </FormItem>
                )} />
                <FormField control={form.control} name="password" render={({ field }) => (
                    <FormItem>
                    <FormLabel>Password</FormLabel>
                    <FormControl><Input type="password" {...field} /></FormControl><FormMessage />
                    </FormItem>
                )} />
            </div>
           </>
        )}

        {type === 'azure_blob' && (
             <>
               <FormField control={form.control} name="connectionString" render={({ field }) => (
                <FormItem>
                  <FormLabel>Connection String</FormLabel>
                  <FormControl><Input type="password" {...field} /></FormControl><FormMessage />
                </FormItem>
               )} />
                <FormField control={form.control} name="containerName" render={({ field }) => (
                <FormItem>
                  <FormLabel>Container Name</FormLabel>
                  <FormControl><Input {...field} /></FormControl><FormMessage />
                </FormItem>
               )} />
             </>
        )}

        {/* Test Connection Result */}
        {testResult && (
          <div className={`p-3 rounded-md border ${testResult.success ? 'bg-green-50 border-green-200 dark:bg-green-950 dark:border-green-800' : 'bg-red-50 border-red-200 dark:bg-red-950 dark:border-red-800'}`}>
            <div className="flex items-center gap-2">
              {testResult.success ? (
                <CheckCircle2 className="h-4 w-4 text-green-600 dark:text-green-400" />
              ) : (
                <XCircle className="h-4 w-4 text-red-600 dark:text-red-400" />
              )}
              <span className={`font-medium text-sm ${testResult.success ? 'text-green-700 dark:text-green-300' : 'text-red-700 dark:text-red-300'}`}>
                {testResult.message}
              </span>
            </div>
            {testResult.details && (
              <p className={`mt-1 text-xs ${testResult.success ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'}`}>
                {testResult.details}
              </p>
            )}
          </div>
        )}

        <div className="flex justify-between gap-2 pt-4">
          <Button 
            type="button" 
            variant="outline" 
            onClick={handleTest}
            disabled={isTestingConnection}
          >
            {isTestingConnection ? (
              <>
                <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                Testing...
              </>
            ) : (
              <>
                <Zap className="h-4 w-4 mr-2" />
                Test Connection
              </>
            )}
          </Button>
          <div className="flex gap-2">
            <Button type="button" variant="outline" onClick={onCancel}>Cancel</Button>
            <Button type="submit" disabled={isSubmitting}>
               {isSubmitting ? "Creating..." : "Create Connection"}
            </Button>
          </div>
        </div>
      </form>
    </Form>
  );
}
