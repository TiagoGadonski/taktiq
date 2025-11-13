'use client';

import { useEffect, useState } from 'react';
import { useAuth } from '@/hooks/use-auth';
import { useRouter } from 'next/navigation';
import {
  Activity,
  ArrowLeft,
  Search,
  Filter,
  Calendar,
  User,
  AlertCircle,
  CheckCircle,
  XCircle,
  Clock,
  RefreshCw,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { useToast } from '@/hooks/use-toast';
import { apiClient } from '@/lib/api';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Label } from '@/components/ui/label';

interface ActivityLog {
  id: string;
  userId: string;
  userName: string;
  userEmail: string;
  action: string;
  endpoint: string;
  httpMethod: string;
  statusCode: number;
  responseTimeMs: number;
  ipAddress: string;
  userAgent: string;
  timestamp: string;
  errorMessage?: string;
}

interface LogsResponse {
  logs: ActivityLog[];
  pagination: {
    currentPage: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
  };
}

export default function AdminLogsPage() {
  const { user } = useAuth();
  const router = useRouter();
  const { toast } = useToast();
  const [logs, setLogs] = useState<ActivityLog[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(20);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  // Filters
  const [searchTerm, setSearchTerm] = useState('');
  const [actionFilter, setActionFilter] = useState<string>('all');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');

  // Redirect if not admin
  useEffect(() => {
    if (user && user.role !== 'Admin') {
      router.push('/dashboard');
      toast({
        title: 'Acesso negado',
        description: 'Você não tem permissão para acessar esta página.',
        variant: 'destructive',
      });
    }
  }, [user, router, toast]);

  // Fetch logs
  const fetchLogs = async () => {
    setIsLoading(true);
    try {
      const params: any = {
        page: currentPage,
        pageSize: pageSize,
      };

      if (actionFilter && actionFilter !== 'all') {
        params.action = actionFilter;
      }

      if (startDate) {
        params.startDate = new Date(startDate).toISOString();
      }

      if (endDate) {
        params.endDate = new Date(endDate).toISOString();
      }

      const queryString = new URLSearchParams(
        Object.entries(params).map(([key, value]) => [key, String(value)])
      ).toString();

      const data = await apiClient.get<LogsResponse>(`/admin/activity-logs?${queryString}`);
      setLogs(data.logs || []);
      setTotalPages(data.pagination.totalPages);
      setTotalCount(data.pagination.totalCount);
    } catch (error) {
      toast({
        title: 'Erro',
        description: 'Não foi possível carregar os logs.',
        variant: 'destructive',
      });
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    if (user?.role === 'Admin') {
      fetchLogs();
    }
  }, [user, currentPage, actionFilter, startDate, endDate]);

  // Filter logs by search term (client-side)
  const filteredLogs = logs.filter(
    (log) =>
      log.userName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      log.userEmail?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      log.action?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      log.endpoint?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const getStatusBadge = (statusCode: number) => {
    if (statusCode >= 200 && statusCode < 300) {
      return (
        <Badge className="bg-green-500/20 text-green-500 border-green-500/30">
          <CheckCircle className="h-3 w-3 mr-1" />
          {statusCode}
        </Badge>
      );
    } else if (statusCode >= 400 && statusCode < 500) {
      return (
        <Badge className="bg-orange-500/20 text-orange-500 border-orange-500/30">
          <AlertCircle className="h-3 w-3 mr-1" />
          {statusCode}
        </Badge>
      );
    } else if (statusCode >= 500) {
      return (
        <Badge className="bg-red-500/20 text-red-500 border-red-500/30">
          <XCircle className="h-3 w-3 mr-1" />
          {statusCode}
        </Badge>
      );
    }
    return <Badge variant="outline">{statusCode}</Badge>;
  };

  const getActionBadge = (action: string) => {
    const colors: Record<string, string> = {
      Login: 'bg-blue-500/20 text-blue-500 border-blue-500/30',
      Logout: 'bg-gray-500/20 text-gray-500 border-gray-500/30',
      CreatePlan: 'bg-green-500/20 text-green-500 border-green-500/30',
      UpdateProfile: 'bg-purple-500/20 text-purple-500 border-purple-500/30',
      StartWorkout: 'bg-orange-500/20 text-orange-500 border-orange-500/30',
      CompleteWorkout: 'bg-green-500/20 text-green-500 border-green-500/30',
    };
    return (
      <Badge className={`border ${colors[action] || 'bg-gray-500/20 text-gray-500 border-gray-500/30'}`}>
        {action}
      </Badge>
    );
  };

  const getResponseTimeColor = (ms: number) => {
    if (ms < 100) return 'text-green-500';
    if (ms < 500) return 'text-yellow-500';
    if (ms < 1000) return 'text-orange-500';
    return 'text-red-500';
  };

  if (user?.role !== 'Admin') {
    return null;
  }

  if (isLoading && logs.length === 0) {
    return (
      <div className="flex h-full items-center justify-center">
        <div className="text-center">
          <div className="h-12 w-12 animate-spin rounded-full border-4 border-primary border-t-transparent mx-auto mb-4" />
          <p className="text-muted-foreground">Carregando logs...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Header */}
      <div className="glass rounded-xl p-6 border hover-lift">
        <div className="flex items-center gap-3 mb-2">
          <Button
            variant="ghost"
            size="icon"
            onClick={() => router.push('/admin')}
            className="hover-lift tap-scale"
          >
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <Activity className="h-8 w-8 text-primary animate-glow-pulse" />
          <h1 className="text-3xl font-bold bg-gradient-to-r from-primary to-primary/70 bg-clip-text text-transparent">
            Logs de Atividade
          </h1>
        </div>
        <p className="text-muted-foreground ml-14">
          Monitore todas as atividades do sistema em tempo real
        </p>
      </div>

      {/* Stats */}
      <div className="grid gap-4 md:grid-cols-3">
        <Card className="glass hover-lift tap-scale p-6 border-primary/20">
          <div className="flex items-center gap-4">
            <div className="p-3 bg-primary/20 rounded-lg">
              <Activity className="h-6 w-6 text-primary" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Total de Logs</p>
              <p className="text-2xl font-bold text-primary">{totalCount}</p>
            </div>
          </div>
        </Card>
        <Card className="glass hover-lift tap-scale p-6 border-blue-500/20">
          <div className="flex items-center gap-4">
            <div className="p-3 bg-blue-500/20 rounded-lg">
              <CheckCircle className="h-6 w-6 text-blue-500" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Requisições OK</p>
              <p className="text-2xl font-bold text-blue-500">
                {logs.filter((l) => l.statusCode >= 200 && l.statusCode < 300).length}
              </p>
            </div>
          </div>
        </Card>
        <Card className="glass hover-lift tap-scale p-6 border-red-500/20">
          <div className="flex items-center gap-4">
            <div className="p-3 bg-red-500/20 rounded-lg">
              <XCircle className="h-6 w-6 text-red-500" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Erros</p>
              <p className="text-2xl font-bold text-red-500">
                {logs.filter((l) => l.statusCode >= 400).length}
              </p>
            </div>
          </div>
        </Card>
      </div>

      {/* Filters */}
      <Card className="glass border-primary/20">
        <CardContent className="pt-6">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            {/* Search */}
            <div className="space-y-2">
              <Label htmlFor="search">Buscar</Label>
              <div className="relative">
                <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                <Input
                  id="search"
                  placeholder="Usuário, ação, endpoint..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-10 glass"
                />
              </div>
            </div>

            {/* Action Filter */}
            <div className="space-y-2">
              <Label htmlFor="action">Ação</Label>
              <Select value={actionFilter} onValueChange={setActionFilter}>
                <SelectTrigger id="action" className="glass">
                  <SelectValue placeholder="Todas as ações" />
                </SelectTrigger>
                <SelectContent className="glass">
                  <SelectItem value="all">Todas</SelectItem>
                  <SelectItem value="Login">Login</SelectItem>
                  <SelectItem value="Logout">Logout</SelectItem>
                  <SelectItem value="CreatePlan">Criar Plano</SelectItem>
                  <SelectItem value="UpdateProfile">Atualizar Perfil</SelectItem>
                  <SelectItem value="StartWorkout">Iniciar Treino</SelectItem>
                  <SelectItem value="CompleteWorkout">Completar Treino</SelectItem>
                </SelectContent>
              </Select>
            </div>

            {/* Start Date */}
            <div className="space-y-2">
              <Label htmlFor="startDate">Data Inicial</Label>
              <Input
                id="startDate"
                type="date"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                className="glass"
              />
            </div>

            {/* End Date */}
            <div className="space-y-2">
              <Label htmlFor="endDate">Data Final</Label>
              <Input
                id="endDate"
                type="date"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                className="glass"
              />
            </div>
          </div>

          <div className="flex gap-2 mt-4">
            <Button
              variant="outline"
              size="sm"
              onClick={() => {
                setSearchTerm('');
                setActionFilter('all');
                setStartDate('');
                setEndDate('');
                setCurrentPage(1);
              }}
              className="hover-lift tap-scale"
            >
              <XCircle className="mr-2 h-4 w-4" />
              Limpar Filtros
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={fetchLogs}
              className="hover-lift tap-scale"
            >
              <RefreshCw className="mr-2 h-4 w-4" />
              Atualizar
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Logs Table */}
      <Card className="glass border-primary/20">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="border-b border-border/50">
              <tr className="text-left">
                <th className="p-4 font-semibold">Data/Hora</th>
                <th className="p-4 font-semibold">Usuário</th>
                <th className="p-4 font-semibold">Ação</th>
                <th className="p-4 font-semibold">Endpoint</th>
                <th className="p-4 font-semibold">Método</th>
                <th className="p-4 font-semibold">Status</th>
                <th className="p-4 font-semibold">Tempo</th>
                <th className="p-4 font-semibold">IP</th>
              </tr>
            </thead>
            <tbody>
              {filteredLogs.length === 0 ? (
                <tr>
                  <td colSpan={8} className="p-8 text-center text-muted-foreground">
                    Nenhum log encontrado
                  </td>
                </tr>
              ) : (
                filteredLogs.map((log, index) => (
                  <tr
                    key={log.id}
                    className="border-b border-border/30 hover:bg-accent/50 transition-colors animate-slide-up"
                    style={{ animationDelay: `${index * 30}ms` }}
                  >
                    <td className="p-4 text-sm">
                      <div className="flex flex-col">
                        <span className="font-medium">
                          {new Date(log.timestamp).toLocaleDateString('pt-BR')}
                        </span>
                        <span className="text-xs text-muted-foreground">
                          {new Date(log.timestamp).toLocaleTimeString('pt-BR')}
                        </span>
                      </div>
                    </td>
                    <td className="p-4">
                      <div className="flex flex-col">
                        <span className="font-medium text-sm">{log.userName || 'N/A'}</span>
                        <span className="text-xs text-muted-foreground">
                          {log.userEmail || 'N/A'}
                        </span>
                      </div>
                    </td>
                    <td className="p-4">{getActionBadge(log.action)}</td>
                    <td className="p-4">
                      <code className="text-xs bg-secondary px-2 py-1 rounded">
                        {log.endpoint}
                      </code>
                    </td>
                    <td className="p-4">
                      <Badge variant="outline" className="text-xs">
                        {log.httpMethod}
                      </Badge>
                    </td>
                    <td className="p-4">{getStatusBadge(log.statusCode)}</td>
                    <td className="p-4">
                      <div className="flex items-center gap-1">
                        <Clock className={`h-3 w-3 ${getResponseTimeColor(log.responseTimeMs)}`} />
                        <span className={`text-sm font-medium ${getResponseTimeColor(log.responseTimeMs)}`}>
                          {log.responseTimeMs}ms
                        </span>
                      </div>
                    </td>
                    <td className="p-4 text-xs text-muted-foreground font-mono">
                      {log.ipAddress}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination Controls */}
        {totalPages > 1 && (
          <div className="flex items-center justify-between px-6 py-4 border-t border-border/50">
            <div className="text-sm text-muted-foreground">
              Mostrando {(currentPage - 1) * pageSize + 1} a{' '}
              {Math.min(currentPage * pageSize, totalCount)} de {totalCount} logs
            </div>
            <div className="flex items-center gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
                disabled={currentPage === 1}
                className="hover-lift tap-scale"
              >
                Anterior
              </Button>
              <div className="flex items-center gap-1">
                {Array.from({ length: Math.min(totalPages, 5) }, (_, i) => {
                  let pageNum: number;
                  if (totalPages <= 5) {
                    pageNum = i + 1;
                  } else if (currentPage <= 3) {
                    pageNum = i + 1;
                  } else if (currentPage >= totalPages - 2) {
                    pageNum = totalPages - 4 + i;
                  } else {
                    pageNum = currentPage - 2 + i;
                  }

                  return (
                    <Button
                      key={pageNum}
                      variant={currentPage === pageNum ? 'default' : 'outline'}
                      size="sm"
                      onClick={() => setCurrentPage(pageNum)}
                      className={`hover-lift tap-scale ${
                        currentPage === pageNum ? 'bg-primary text-primary-foreground' : ''
                      }`}
                    >
                      {pageNum}
                    </Button>
                  );
                })}
              </div>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
                disabled={currentPage === totalPages}
                className="hover-lift tap-scale"
              >
                Próxima
              </Button>
            </div>
          </div>
        )}
      </Card>
    </div>
  );
}
