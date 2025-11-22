'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';

export default function MarketplacePage() {
  const router = useRouter();

  useEffect(() => {
    // Redirect to discover page
    router.replace('/discover');
  }, [router]);

  return (
    <div className="flex items-center justify-center min-h-screen">
      <div className="text-center">
        <h2 className="text-xl font-semibold mb-2">Redirecionando...</h2>
        <p className="text-muted-foreground">
          O marketplace foi integrado à página Descobrir Planos
        </p>
      </div>
    </div>
  );
}
