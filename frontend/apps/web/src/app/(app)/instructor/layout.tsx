'use client';

import { InstructorSidebarNav } from '@/components/instructor/sidebar-nav';
import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/api';

interface InstructorStats {
  clientCount: number;
  pendingInvites: number;
}

export default function InstructorLayout({
  children,
}: {
  children: React.Node;
}) {
  // Fetch instructor stats for badge counts
  const { data: stats } = useQuery({
    queryKey: ['instructor-stats'],
    queryFn: async () => {
      try {
        const clients = await apiClient.get<any[]>('/personal/clients');
        // TODO: Fetch pending invites count from API
        return {
          clientCount: clients?.length || 0,
          pendingInvites: 0 // Placeholder
        } as InstructorStats;
      } catch (error) {
        console.error('Failed to fetch instructor stats:', error);
        return {
          clientCount: 0,
          pendingInvites: 0
        } as InstructorStats;
      }
    },
    staleTime: 5 * 60 * 1000, // 5 minutes
  });

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Sidebar Navigation */}
      <InstructorSidebarNav
        clientCount={stats?.clientCount}
        pendingInvites={stats?.pendingInvites}
      />

      {/* Main Content Area */}
      <div className="lg:pl-64">
        {/* Mobile top padding for header */}
        <div className="lg:hidden h-16" />

        {/* Content */}
        <main className="min-h-screen">
          {children}
        </main>
      </div>
    </div>
  );
}
