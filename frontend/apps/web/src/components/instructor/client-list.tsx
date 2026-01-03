'use client';

import { useState } from 'react';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { MoreVertical, Eye, FileText, Dumbbell, Trash2 } from 'lucide-react';
import { cn } from '@/lib/utils';
import { ClientSlideOver } from './client-slide-over';
import { useRouter } from 'next/navigation';

interface Client {
  id: string;
  name: string;
  email: string;
  profilePicture?: string;
  planCount: number;
  workoutCount: number;
  frequency: number;
  lastWorkout?: string;
  createdAt: string;
  status: 'active' | 'inactive' | 'invited';
  ptNotes?: string;
  latestAssessment?: any;
  activePlans?: any[];
}

interface ClientListProps {
  clients: Client[];
  onDeleteClient?: (clientId: string) => void;
}

export function ClientList({ clients, onDeleteClient }: ClientListProps) {
  const router = useRouter();
  const [selectedClient, setSelectedClient] = useState<Client | null>(null);
  const [isSlideOverOpen, setIsSlideOverOpen] = useState(false);

  const getInitials = (name: string) => {
    return name
      ?.split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  };

  const getStatusBadge = (client: Client) => {
    if (client.status === 'invited') {
      return (
        <Badge variant="outline" className="bg-blue-50 text-blue-700 border-blue-200">
          Convidado
        </Badge>
      );
    }

    if (client.planCount === 0) {
      return (
        <Badge variant="outline" className="bg-red-50 text-red-700 border-red-200">
          Sem Plano
        </Badge>
      );
    }

    if (client.planCount === 1) {
      return (
        <Badge variant="outline" className="bg-yellow-50 text-yellow-700 border-yellow-200">
          {client.planCount} plano
        </Badge>
      );
    }

    return (
      <Badge variant="outline" className="bg-green-50 text-green-700 border-green-200">
        {client.planCount} planos
      </Badge>
    );
  };

  const handleRowClick = (client: Client) => {
    setSelectedClient(client);
    setIsSlideOverOpen(true);
  };

  const handleDelete = (e: React.MouseEvent, clientId: string) => {
    e.stopPropagation();
    if (onDeleteClient) {
      onDeleteClient(clientId);
    }
  };

  if (clients.length === 0) {
    return (
      <div className="text-center py-12">
        <div className="text-muted-foreground">
          <p className="text-lg font-medium">Nenhum cliente encontrado</p>
          <p className="text-sm mt-1">
            Convide seu primeiro aluno para começar!
          </p>
        </div>
      </div>
    );
  }

  return (
    <>
      <div className="border rounded-lg overflow-hidden">
        <table className="w-full">
          <thead className="bg-gray-50 border-b">
            <tr>
              <th className="text-left py-3 px-4 text-sm font-medium text-muted-foreground">
                Nome
              </th>
              <th className="text-left py-3 px-4 text-sm font-medium text-muted-foreground">
                Status
              </th>
              <th className="text-left py-3 px-4 text-sm font-medium text-muted-foreground">
                Último Treino
              </th>
              <th className="text-right py-3 px-4 text-sm font-medium text-muted-foreground">
                Ações
              </th>
            </tr>
          </thead>
          <tbody className="divide-y">
            {clients.map((client) => (
              <tr
                key={client.id}
                className="hover:bg-gray-50 cursor-pointer transition-colors"
                onClick={() => handleRowClick(client)}
              >
                <td className="py-3 px-4">
                  <div className="flex items-center gap-3">
                    <Avatar className="h-10 w-10">
                      <AvatarImage src={client.profilePicture} alt={client.name} />
                      <AvatarFallback>{getInitials(client.name)}</AvatarFallback>
                    </Avatar>
                    <div>
                      <div className="font-medium">{client.name}</div>
                      <div className="text-sm text-muted-foreground">
                        {client.email}
                      </div>
                    </div>
                  </div>
                </td>
                <td className="py-3 px-4">
                  {getStatusBadge(client)}
                </td>
                <td className="py-3 px-4">
                  <div className="text-sm">
                    {client.lastWorkout || (
                      <span className="text-muted-foreground">
                        Nenhum treino
                      </span>
                    )}
                  </div>
                </td>
                <td className="py-3 px-4 text-right">
                  <DropdownMenu>
                    <DropdownMenuTrigger asChild onClick={(e) => e.stopPropagation()}>
                      <Button variant="ghost" size="sm">
                        <MoreVertical className="h-4 w-4" />
                      </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                      <DropdownMenuItem
                        onClick={(e) => {
                          e.stopPropagation();
                          router.push(`/instructor/clients/${client.id}`);
                        }}
                      >
                        <Eye className="h-4 w-4 mr-2" />
                        Ver Perfil
                      </DropdownMenuItem>
                      <DropdownMenuItem
                        onClick={(e) => {
                          e.stopPropagation();
                          router.push(`/instructor/clients/${client.id}/assessments/new`);
                        }}
                      >
                        <FileText className="h-4 w-4 mr-2" />
                        Nova Avaliação
                      </DropdownMenuItem>
                      <DropdownMenuItem
                        onClick={(e) => {
                          e.stopPropagation();
                          router.push(`/plans/new?clientId=${client.id}`);
                        }}
                      >
                        <Dumbbell className="h-4 w-4 mr-2" />
                        Criar Plano
                      </DropdownMenuItem>
                      {onDeleteClient && (
                        <>
                          <DropdownMenuItem
                            className="text-red-600"
                            onClick={(e) => handleDelete(e, client.id)}
                          >
                            <Trash2 className="h-4 w-4 mr-2" />
                            Remover Cliente
                          </DropdownMenuItem>
                        </>
                      )}
                    </DropdownMenuContent>
                  </DropdownMenu>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Client Slide-Over Modal */}
      {selectedClient && (
        <ClientSlideOver
          client={selectedClient}
          open={isSlideOverOpen}
          onOpenChange={setIsSlideOverOpen}
        />
      )}
    </>
  );
}
