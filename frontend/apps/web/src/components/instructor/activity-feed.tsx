'use client';

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Activity, CheckCircle2, AlertCircle, Clock, ArrowRight } from 'lucide-react';
import { cn } from '@/lib/utils';
import { formatDistanceToNow } from 'date-fns';
import { ptBR } from 'date-fns/locale';

interface ActivityItem {
  id: string;
  type: 'workout_completed' | 'check_in' | 'alert';
  clientName: string;
  message: string;
  timestamp: Date;
  urgent?: boolean;
}

interface ActivityFeedProps {
  activities: ActivityItem[];
  onViewAll?: () => void;
}

export function ActivityFeed({ activities, onViewAll }: ActivityFeedProps) {
  const getIcon = (type: ActivityItem['type']) => {
    switch (type) {
      case 'workout_completed':
        return <CheckCircle2 className="h-4 w-4 text-green-600" />;
      case 'check_in':
        return <Activity className="h-4 w-4 text-blue-600" />;
      case 'alert':
        return <AlertCircle className="h-4 w-4 text-orange-600" />;
      default:
        return <Clock className="h-4 w-4 text-gray-600" />;
    }
  };

  const getTimeAgo = (timestamp: Date) => {
    return formatDistanceToNow(timestamp, {
      addSuffix: true,
      locale: ptBR
    });
  };

  if (!activities || activities.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Activity className="h-5 w-5" />
            Atividade Recente
          </CardTitle>
          <CardDescription>Acompanhe as últimas ações dos seus clientes</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="text-center py-8 text-muted-foreground">
            <Activity className="h-12 w-12 mx-auto mb-3 opacity-20" />
            <p>Nenhuma atividade recente</p>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <div>
            <CardTitle className="flex items-center gap-2">
              <Activity className="h-5 w-5" />
              Atividade Recente
            </CardTitle>
            <CardDescription>Acompanhe as últimas ações dos seus clientes</CardDescription>
          </div>
          {onViewAll && (
            <Button variant="ghost" size="sm" onClick={onViewAll}>
              Ver todas
              <ArrowRight className="h-4 w-4 ml-1" />
            </Button>
          )}
        </div>
      </CardHeader>
      <CardContent>
        <div className="space-y-3">
          {activities.map((activity) => (
            <div
              key={activity.id}
              className={cn(
                'flex items-start gap-3 p-3 rounded-lg transition-colors',
                activity.urgent
                  ? 'bg-orange-50 hover:bg-orange-100'
                  : 'bg-gray-50 hover:bg-gray-100'
              )}
            >
              {/* Icon */}
              <div className="mt-0.5">
                {getIcon(activity.type)}
              </div>

              {/* Content */}
              <div className="flex-1 min-w-0">
                <p className="text-sm">
                  <span className="font-medium">{activity.clientName}</span>{' '}
                  {activity.message}
                </p>
                <p className="text-xs text-muted-foreground mt-1">
                  {getTimeAgo(activity.timestamp)}
                </p>
              </div>
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}
