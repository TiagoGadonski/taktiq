'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';

// Redirect to the main discover page
export default function PlansDiscoverRedirect() {
  const router = useRouter();

  useEffect(() => {
    router.replace('/discover');
  }, [router]);

  return (
    <div className="flex h-screen items-center justify-center">
      <div className="text-center">
        <div className="h-12 w-12 animate-spin rounded-full border-4 border-primary border-t-transparent mx-auto mb-4" />
        <p className="text-muted-foreground">Redirecionando...</p>
      </div>
    </div>
  );
}
