import { Route, Switch } from "wouter";
import { Layout } from "@/components/Layout";
import { ProtectedRoute } from "@/components/auth/ProtectedRoute";
import { DashboardPage } from "@/pages/DashboardPage";
import { ConnectionsPage } from "@/pages/ConnectionsPage";
import { ConfigurationsPage } from "@/pages/ConfigurationsPage";
import { ConfigurationFormPage } from "@/pages/ConfigurationFormPage";
import { JobsPage } from "@/pages/JobsPage";
import { UsersPage } from "@/pages/UsersPage";
import { ActivityPage } from "@/pages/ActivityPage";
import { DeidentifyWizardPage } from "@/pages/DeidentifyWizardPage";

import { Toaster } from "@/components/ui/toaster";

function App() {
  return (
    <Layout>
      <Switch>
        <Route path="/">
          <ProtectedRoute permission="read:dashboard">
            <DashboardPage />
          </ProtectedRoute>
        </Route>
        <Route path="/connections">
          <ProtectedRoute permission="manage:connections">
            <ConnectionsPage />
          </ProtectedRoute>
        </Route>
        <Route path="/configurations">
          <ProtectedRoute permission="manage:configurations">
            <ConfigurationsPage />
          </ProtectedRoute>
        </Route>
        <Route path="/configurations/new">
          <ProtectedRoute permission="manage:configurations">
            <ConfigurationFormPage />
          </ProtectedRoute>
        </Route>
        <Route path="/configurations/:id/edit">
          <ProtectedRoute permission="manage:configurations">
            <ConfigurationFormPage />
          </ProtectedRoute>
        </Route>
        <Route path="/deidentify">
          <ProtectedRoute permission="manage:jobs">
            <DeidentifyWizardPage />
          </ProtectedRoute>
        </Route>
        <Route path="/jobs">
          <ProtectedRoute permission="manage:jobs">
            <JobsPage />
          </ProtectedRoute>
        </Route>
        <Route path="/users">
          <ProtectedRoute permission="manage:users">
            <UsersPage />
          </ProtectedRoute>
        </Route>
        <Route path="/activity">
          <ProtectedRoute permission="read:activity">
            <ActivityPage />
          </ProtectedRoute>
        </Route>
        <Route>
          <div className="text-center py-12">
            <h2 className="text-2xl font-bold">404 — Not Found</h2>
          </div>
        </Route>
      </Switch>
      <Toaster />
    </Layout>
  );
}

export default App;
