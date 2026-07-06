'use client';

import { useState, useEffect } from 'react';
import { RefreshCw, CheckCircle2, XCircle, Clock, Loader2, Database, X } from 'lucide-react';
import { useSyncStatus, useTriggerSync, useCancelSync, itemKeys } from '@/lib/hooks/useApi';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress';
import { Badge } from '@/components/ui/badge';
import { useToast } from '@/hooks/use-toast';
import { useQueryClient } from '@tanstack/react-query';

function formatDate(dateString: string) {
  return new Date(dateString).toLocaleString();
}

function formatDuration(startedAt: string, completedAt?: string) {
  const start = new Date(startedAt).getTime();
  const end = completedAt ? new Date(completedAt).getTime() : Date.now();
  const durationMs = end - start;

  const seconds = Math.floor(durationMs / 1000);
  const minutes = Math.floor(seconds / 60);
  const remainingSeconds = seconds % 60;

  if (minutes > 0) {
    return `${minutes}m ${remainingSeconds}s`;
  }
  return `${seconds}s`;
}

function StatusIcon({ status }: { status: string }) {
  switch (status) {
    case 'Completed':
      return <CheckCircle2 className="w-5 h-5 text-green-500" />;
    case 'Failed':
      return <XCircle className="w-5 h-5 text-red-500" />;
    case 'InProgress':
      return <Loader2 className="w-5 h-5 text-blue-500 animate-spin" />;
    case 'Pending':
      return <Clock className="w-5 h-5 text-yellow-500" />;
    case 'Cancelled':
      return <X className="w-5 h-5 text-orange-500" />;
    default:
      return <Clock className="w-5 h-5 text-muted-foreground" />;
  }
}

function StatusBadge({ status }: { status: string }) {
  const variant = {
    Completed: 'default' as const,
    Failed: 'destructive' as const,
    InProgress: 'secondary' as const,
    Pending: 'outline' as const,
    Cancelled: 'outline' as const,
  }[status] || 'outline' as const;

  return <Badge variant={variant}>{status}</Badge>;
}

export default function SyncPage() {
  const [activeJobId, setActiveJobId] = useState<string | undefined>();
  const [trackedJobId, setTrackedJobId] = useState<string | undefined>();
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const { data: syncStatus, isLoading, error, refetch } = useSyncStatus(
    activeJobId,
    'immich',
    true
  );

  const triggerSync = useTriggerSync();
  const cancelSync = useCancelSync();

  // Adjusting state during render (React's documented alternative to an effect
  // for syncing state to a changing value): https://react.dev/reference/react/useState#storing-information-from-previous-renders
  if (syncStatus?.jobId !== trackedJobId) {
    setTrackedJobId(syncStatus?.jobId);
    if (syncStatus?.jobId && !activeJobId) {
      setActiveJobId(syncStatus.jobId);
    }
  }

  // Invalidate items when sync completes
  useEffect(() => {
    if (syncStatus?.status === 'Completed') {
      queryClient.invalidateQueries({ queryKey: itemKeys.all });
    }
  }, [syncStatus?.status, queryClient]);

  const handleTriggerSync = async () => {
    try {
      const response = await triggerSync.mutateAsync(undefined);
      setActiveJobId(response.jobId);
      toast.success(response.message || 'Sync started');
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Failed to start sync');
    }
  };

  const handleCancelSync = async () => {
    if (!activeJobId) return;

    try {
      const response = await cancelSync.mutateAsync(activeJobId);
      toast.success(response.message || 'Sync cancelled');
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Failed to cancel sync');
    }
  };

  const isInProgress = syncStatus?.status === 'Pending' || syncStatus?.status === 'InProgress';
  const progressPercentage = syncStatus?.totalItems
    ? Math.round((syncStatus.processedItems / syncStatus.totalItems) * 100)
    : 0;

  return (
    <div className="container mx-auto px-4 py-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Sync</h1>
          <p className="text-muted-foreground">Sync media items from external providers</p>
        </div>
        <div className="flex gap-2">
          {isInProgress && (
            <Button
              variant="outline"
              onClick={handleCancelSync}
              disabled={cancelSync.isPending}
            >
              {cancelSync.isPending ? (
                <>
                  <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                  Cancelling...
                </>
              ) : (
                <>
                  <X className="w-4 h-4 mr-2" />
                  Cancel
                </>
              )}
            </Button>
          )}
          <Button
            onClick={handleTriggerSync}
            disabled={isInProgress || triggerSync.isPending}
          >
            {triggerSync.isPending ? (
              <>
                <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                Starting...
              </>
            ) : isInProgress ? (
              <>
                <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                Syncing...
              </>
            ) : (
              <>
                <RefreshCw className="w-4 h-4 mr-2" />
                Sync Now
              </>
            )}
          </Button>
        </div>
      </div>

      <div className="grid gap-6">
        {/* Current/Latest Sync Status */}
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-3">
                <Database className="w-6 h-6 text-primary" />
                <div>
                  <CardTitle>Immich Provider</CardTitle>
                  <CardDescription>Sync status for Immich media library</CardDescription>
                </div>
              </div>
              {syncStatus && <StatusBadge status={syncStatus.status} />}
            </div>
          </CardHeader>
          <CardContent className="space-y-4">
            {isLoading ? (
              <div className="flex items-center justify-center py-8">
                <Loader2 className="w-6 h-6 animate-spin text-muted-foreground" />
                <span className="ml-2 text-muted-foreground">Loading sync status...</span>
              </div>
            ) : error ? (
              <div className="text-center py-8">
                <p className="text-muted-foreground mb-4">
                  No sync history found. Start your first sync to import items from Immich.
                </p>
                <Button onClick={handleTriggerSync} disabled={triggerSync.isPending}>
                  <RefreshCw className="w-4 h-4 mr-2" />
                  Start First Sync
                </Button>
              </div>
            ) : syncStatus ? (
              <>
                {/* Progress bar for active syncs */}
                {isInProgress && (
                  <div className="space-y-2">
                    <div className="flex justify-between text-sm">
                      <span>Progress</span>
                      <span>
                        {syncStatus.processedItems.toLocaleString()} / {syncStatus.totalItems.toLocaleString()} items
                      </span>
                    </div>
                    <Progress value={progressPercentage} />
                    <p className="text-sm text-muted-foreground text-center">
                      {progressPercentage}% complete
                    </p>
                  </div>
                )}

                {/* Sync details */}
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4 pt-4">
                  <div className="space-y-1">
                    <p className="text-sm text-muted-foreground">Status</p>
                    <div className="flex items-center gap-2">
                      <StatusIcon status={syncStatus.status} />
                      <span className="font-medium">{syncStatus.status}</span>
                    </div>
                  </div>
                  <div className="space-y-1">
                    <p className="text-sm text-muted-foreground">Started</p>
                    <p className="font-medium">{formatDate(syncStatus.startedAt)}</p>
                  </div>
                  <div className="space-y-1">
                    <p className="text-sm text-muted-foreground">Duration</p>
                    <p className="font-medium">
                      {formatDuration(syncStatus.startedAt, syncStatus.completedAt)}
                    </p>
                  </div>
                  <div className="space-y-1">
                    <p className="text-sm text-muted-foreground">Items Synced</p>
                    <p className="font-medium">{syncStatus.processedItems.toLocaleString()}</p>
                  </div>
                </div>

                {/* Error message for failed syncs */}
                {syncStatus.status === 'Failed' && syncStatus.errorMessage && (
                  <div className="mt-4 p-4 bg-destructive/10 border border-destructive/20 rounded-lg">
                    <p className="text-sm font-medium text-destructive">Error</p>
                    <p className="text-sm text-destructive/80 mt-1">{syncStatus.errorMessage}</p>
                  </div>
                )}

                {/* Success message */}
                {syncStatus.status === 'Completed' && (
                  <div className="mt-4 p-4 bg-green-500/10 border border-green-500/20 rounded-lg">
                    <p className="text-sm font-medium text-green-600">Sync completed successfully</p>
                    <p className="text-sm text-green-600/80 mt-1">
                      {syncStatus.processedItems.toLocaleString()} items have been synced from Immich.
                    </p>
                  </div>
                )}

                {/* Cancelled message */}
                {syncStatus.status === 'Cancelled' && (
                  <div className="mt-4 p-4 bg-orange-500/10 border border-orange-500/20 rounded-lg">
                    <p className="text-sm font-medium text-orange-600">Sync cancelled</p>
                    <p className="text-sm text-orange-600/80 mt-1">
                      The sync was cancelled. {syncStatus.processedItems.toLocaleString()} of {syncStatus.totalItems.toLocaleString()} items were processed before cancellation.
                    </p>
                  </div>
                )}
              </>
            ) : null}
          </CardContent>
        </Card>

        {/* Info card */}
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">About Syncing</CardTitle>
          </CardHeader>
          <CardContent className="text-sm text-muted-foreground space-y-2">
            <p>
              Syncing imports metadata from your Immich library into Video Manager. This allows you to:
            </p>
            <ul className="list-disc list-inside space-y-1 ml-2">
              <li>Filter and search items without querying Immich directly</li>
              <li>Tag items and organize them into collections</li>
              <li>Quickly find untagged items that need organization</li>
            </ul>
            <p className="pt-2">
              Thumbnails and previews are still loaded from Immich in real-time. Only metadata is stored locally.
            </p>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
