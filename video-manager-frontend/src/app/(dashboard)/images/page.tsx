'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';

export default function ImagesPage() {
  const router = useRouter();

  useEffect(() => {
    router.replace('/items');
  }, [router]);

  return (
    <div className="min-h-screen flex items-center justify-center">
      <p>Redirecting to items...</p>
    </div>
  );
}
