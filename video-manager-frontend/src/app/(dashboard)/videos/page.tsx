'use client';

import { useVideos } from '@/lib/hooks/useVideos';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { useLogout } from '@/lib/hooks/useAuth';
import Link from 'next/link';

export default function VideosPage() {
  const { data, isLoading, error } = useVideos();
  const logout = useLogout();

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <p>Loading videos...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <p className="text-destructive">Error loading videos</p>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-background">
      <header className="border-b">
        <div className="container mx-auto px-4 py-4 flex justify-between items-center">
          <h1 className="text-2xl font-bold">Video Manager</h1>
          <Button onClick={logout} variant="outline">
            Logout
          </Button>
        </div>
      </header>

      <main className="container mx-auto px-4 py-8">
        <div className="flex justify-between items-center mb-6">
          <h2 className="text-xl font-semibold">My Videos</h2>
          <Link href="/import">
            <Button>Import Videos</Button>
          </Link>
        </div>

        {data?.videos && data.videos.length > 0 ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
            {data.videos.map((video) => (
              <Card key={video.id}>
                <CardHeader>
                  <CardTitle className="truncate">{video.title}</CardTitle>
                  <CardDescription>
                    <Badge variant="secondary">{video.provider}</Badge>
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  {video.thumbnailPath && (
                    <img
                      src={video.thumbnailPath}
                      alt={video.title}
                      className="w-full h-40 object-cover rounded-md mb-2"
                    />
                  )}
                  {video.description && (
                    <p className="text-sm text-muted-foreground line-clamp-2">
                      {video.description}
                    </p>
                  )}
                </CardContent>
              </Card>
            ))}
          </div>
        ) : (
          <Card>
            <CardContent className="py-12 text-center">
              <p className="text-muted-foreground mb-4">No videos found</p>
              <Link href="/import">
                <Button>Import Your First Video</Button>
              </Link>
            </CardContent>
          </Card>
        )}
      </main>
    </div>
  );
}
