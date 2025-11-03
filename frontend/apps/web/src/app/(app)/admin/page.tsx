'use client';

import { useEffect, useState } from 'react';
import { useAuth } from '@/hooks/use-auth';
import { useRouter } from 'next/navigation';
import {
  Shield,
  Users,
  UserPlus,
  Search,
  MoreVertical,
  Ban,
  CheckCircle,
  Edit,
  Trash2,
  Key,
  Trophy,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent } from '@/components/ui/card';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
  DropdownMenuSeparator,
} from '@/components/ui/dropdown-menu';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { useToast } from '@/hooks/use-toast';
import { getAssetUrl } from '@/lib/env';
import { apiClient } from '@/lib/api';

interface User {
  id: string;
  name: string;
  email: string;
  role: 'User' | 'PersonalTrainer' | 'Admin';
  isActive: boolean;
  createdAt: string;
  profilePictureUrl?: string;
  lastLoginAt?: string;
}

export default function AdminPage() {
  const { user } = useAuth();
  const router = useRouter();
  const { toast } = useToast();
  const [users, setUsers] = useState<User[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [createDialogOpen, setCreateDialogOpen] = useState(false);
  const [passwordDialogOpen, setPasswordDialogOpen] = useState(false);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [newRole, setNewRole] = useState<string>('');
  const [newPassword, setNewPassword] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(10);
  const [newUser, setNewUser] = useState({
    name: '',
    email: '',
    password: '',
    role: 'User',
  });

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

  // Fetch users
  const fetchUsers = async () => {
    try {
      const data = await apiClient.get<any>('/admin/users');
      setUsers(data.users || data);
    } catch (error) {
      toast({
        title: 'Erro',
        description: 'Não foi possível carregar os usuários.',
        variant: 'destructive',
      });
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    if (user?.role === 'Admin') {
      fetchUsers();
    }
  }, [user]);

  // Filter and sort users (most recent first)
  const filteredUsers = users
    .filter(
      (u) =>
        u.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        u.email.toLowerCase().includes(searchTerm.toLowerCase())
    )
    .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());

  // Pagination
  const totalPages = Math.ceil(filteredUsers.length / pageSize);
  const startIndex = (currentPage - 1) * pageSize;
  const endIndex = startIndex + pageSize;
  const paginatedUsers = filteredUsers.slice(startIndex, endIndex);

  // Reset to page 1 when search term changes
  useEffect(() => {
    setCurrentPage(1);
  }, [searchTerm]);

  const handleEditUser = (user: User) => {
    setSelectedUser(user);
    setNewRole(user.role);
    setEditDialogOpen(true);
  };

  const handleCreateUser = async () => {
    try {
      const createdUser = await apiClient.post<any>('/admin/users', newUser);
      toast({
        title: 'Sucesso',
        description: 'Usuário criado com sucesso.',
      });
      setCreateDialogOpen(false);
      setNewUser({ name: '', email: '', password: '', role: 'User' });
      fetchUsers(); // Refresh the list
    } catch (error: any) {
      toast({
        title: 'Erro',
        description: error.response?.data?.message || 'Não foi possível criar o usuário.',
        variant: 'destructive',
      });
    }
  };

  const handleUpdateUser = async () => {
    if (!selectedUser) return;

    try {
      await apiClient.put(`/admin/users/${selectedUser.id}`, { role: newRole });
      setUsers((prev) =>
        prev.map((u) =>
          u.id === selectedUser.id ? { ...u, role: newRole as any } : u
        )
      );
      toast({
        title: 'Sucesso',
        description: 'Usuário atualizado com sucesso.',
      });
      setEditDialogOpen(false);
    } catch (error) {
      toast({
        title: 'Erro',
        description: 'Não foi possível atualizar o usuário.',
        variant: 'destructive',
      });
    }
  };

  const handleToggleUserStatus = async (userId: string, isActive: boolean) => {
    try {
      const endpoint = isActive ? 'deactivate' : 'activate';
      await apiClient.post(`/admin/users/${userId}/${endpoint}`, {});
      setUsers((prev) =>
        prev.map((u) => (u.id === userId ? { ...u, isActive: !isActive } : u))
      );
      toast({
        title: 'Sucesso',
        description: `Usuário ${isActive ? 'desativado' : 'ativado'} com sucesso.`,
      });
    } catch (error) {
      toast({
        title: 'Erro',
        description: 'Não foi possível alterar o status do usuário.',
        variant: 'destructive',
      });
    }
  };

  const handleDeleteUser = async (userId: string) => {
    if (!confirm('Tem certeza que deseja excluir este usuário?')) return;

    try {
      await apiClient.delete(`/admin/users/${userId}`);
      setUsers((prev) => prev.filter((u) => u.id !== userId));
      toast({
        title: 'Sucesso',
        description: 'Usuário excluído com sucesso.',
      });
    } catch (error) {
      toast({
        title: 'Erro',
        description: 'Não foi possível excluir o usuário.',
        variant: 'destructive',
      });
    }
  };

  const handleChangePassword = (user: User) => {
    setSelectedUser(user);
    setNewPassword('');
    setPasswordDialogOpen(true);
  };

  const handleSavePassword = async () => {
    if (!selectedUser) return;

    try {
      await apiClient.post(`/admin/users/${selectedUser.id}/change-password`, {
        newPassword: newPassword,
      });
      toast({
        title: 'Sucesso',
        description: 'Senha alterada com sucesso.',
      });
      setPasswordDialogOpen(false);
      setNewPassword('');
    } catch (error: any) {
      toast({
        title: 'Erro',
        description: error.response?.data?.message || 'Não foi possível alterar a senha.',
        variant: 'destructive',
      });
    }
  };

  const handleSeedChallenges = async () => {
    try {
      const response = await apiClient.post<any>('/admin/seed-challenges', {});
      toast({
        title: 'Sucesso',
        description: response.message || 'Desafios do sistema criados com sucesso.',
      });
    } catch (error: any) {
      toast({
        title: 'Erro',
        description: error.response?.data?.message || 'Não foi possível criar os desafios.',
        variant: 'destructive',
      });
    }
  };

  const getRoleBadge = (role: string) => {
    const styles = {
      Admin: 'bg-primary/20 text-primary border-primary/30',
      PersonalTrainer: 'bg-blue-500/20 text-blue-500 border-blue-500/30',
      User: 'bg-gray-500/20 text-gray-500 border-gray-500/30',
    };
    return styles[role as keyof typeof styles] || styles.User;
  };

  if (user?.role !== 'Admin') {
    return null;
  }

  if (isLoading) {
    return (
      <div className="flex h-full items-center justify-center">
        <div className="text-center">
          <div className="h-12 w-12 animate-spin rounded-full border-4 border-primary border-t-transparent mx-auto mb-4" />
          <p className="text-muted-foreground">Carregando...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Header */}
      <div className="glass rounded-xl p-6 border hover-lift">
        <div className="flex items-center gap-3 mb-2">
          <Shield className="h-8 w-8 text-primary animate-glow-pulse" />
          <h1 className="text-3xl font-bold bg-gradient-to-r from-primary to-primary/70 bg-clip-text text-transparent">
            Painel de Administração
          </h1>
        </div>
        <p className="text-muted-foreground">
          Gerencie usuários, permissões e configurações do sistema
        </p>
      </div>

      {/* System Actions */}
      <Card className="glass border-primary/20">
        <CardContent className="pt-6">
          <div className="flex items-start justify-between gap-4">
            <div className="flex items-center gap-3">
              <div className="p-3 bg-primary/20 rounded-lg">
                <Trophy className="h-6 w-6 text-primary" />
              </div>
              <div>
                <h3 className="font-semibold">Desafios do Sistema</h3>
                <p className="text-sm text-muted-foreground">
                  Criar desafios padrão para todos os usuários
                </p>
              </div>
            </div>
            <Button
              onClick={handleSeedChallenges}
              className="bg-primary hover:bg-primary/90 hover-lift tap-scale"
            >
              <Trophy className="mr-2 h-4 w-4" />
              Criar Desafios
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Stats */}
      <div className="grid gap-4 md:grid-cols-3">
        <Card className="glass hover-lift tap-scale p-6 border-primary/20">
          <div className="flex items-center gap-4">
            <div className="p-3 bg-primary/20 rounded-lg">
              <Users className="h-6 w-6 text-primary" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Total de Usuários</p>
              <p className="text-2xl font-bold text-primary">{users.length}</p>
            </div>
          </div>
        </Card>
        <Card className="glass hover-lift tap-scale p-6 border-blue-500/20">
          <div className="flex items-center gap-4">
            <div className="p-3 bg-blue-500/20 rounded-lg">
              <UserPlus className="h-6 w-6 text-blue-500" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Personal Trainers</p>
              <p className="text-2xl font-bold text-blue-500">
                {users.filter((u) => u.role === 'PersonalTrainer').length}
              </p>
            </div>
          </div>
        </Card>
        <Card className="glass hover-lift tap-scale p-6 border-green-500/20">
          <div className="flex items-center gap-4">
            <div className="p-3 bg-green-500/20 rounded-lg">
              <CheckCircle className="h-6 w-6 text-green-500" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Usuários Ativos</p>
              <p className="text-2xl font-bold text-green-500">
                {users.filter((u) => u.isActive).length}
              </p>
            </div>
          </div>
        </Card>
      </div>

      {/* Search and Actions */}
      <div className="flex gap-4">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Buscar usuários por nome ou email..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="pl-10 glass border-primary/20 focus:border-primary/50"
          />
        </div>
        <Button
          onClick={() => setCreateDialogOpen(true)}
          className="bg-primary hover:bg-primary/90 hover-lift tap-scale"
        >
          <UserPlus className="mr-2 h-4 w-4" />
          Criar Usuário
        </Button>
      </div>

      {/* Users Table */}
      <Card className="glass border-primary/20">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="border-b border-border/50">
              <tr className="text-left">
                <th className="p-4 font-semibold">Usuário</th>
                <th className="p-4 font-semibold">Email</th>
                <th className="p-4 font-semibold">Tipo</th>
                <th className="p-4 font-semibold">Status</th>
                <th className="p-4 font-semibold">Criado em</th>
                <th className="p-4 font-semibold">Último Acesso</th>
                <th className="p-4 font-semibold text-right">Ações</th>
              </tr>
            </thead>
            <tbody>
              {paginatedUsers.length === 0 ? (
                <tr>
                  <td colSpan={7} className="p-8 text-center text-muted-foreground">
                    Nenhum usuário encontrado
                  </td>
                </tr>
              ) : (
                paginatedUsers.map((user, index) => (
                <tr
                  key={user.id}
                  className="border-b border-border/30 hover:bg-accent/50 transition-colors animate-slide-up"
                  style={{ animationDelay: `${index * 50}ms` }}
                >
                  <td className="p-4">
                    <div className="flex items-center gap-3">
                      <Avatar className="h-10 w-10 ring-2 ring-primary/20">
                        <AvatarImage src={getAssetUrl(user.profilePictureUrl)} />
                        <AvatarFallback className="bg-primary/20 text-primary font-bold">
                          {user.name.charAt(0).toUpperCase()}
                        </AvatarFallback>
                      </Avatar>
                      <span className="font-medium">{user.name}</span>
                    </div>
                  </td>
                  <td className="p-4 text-muted-foreground">{user.email}</td>
                  <td className="p-4">
                    <Badge className={`${getRoleBadge(user.role)} border`}>
                      {user.role === 'PersonalTrainer'
                        ? 'Personal Trainer'
                        : user.role === 'Admin'
                        ? 'Administrador'
                        : 'Usuário'}
                    </Badge>
                  </td>
                  <td className="p-4">
                    <Badge
                      className={`border ${
                        user.isActive
                          ? 'bg-green-500/20 text-green-500 border-green-500/30'
                          : 'bg-red-500/20 text-red-500 border-red-500/30'
                      }`}
                    >
                      {user.isActive ? 'Ativo' : 'Inativo'}
                    </Badge>
                  </td>
                  <td className="p-4 text-muted-foreground">
                    {new Date(user.createdAt).toLocaleDateString('pt-BR')}
                  </td>
                  <td className="p-4 text-muted-foreground">
                    {user.lastLoginAt
                      ? new Date(user.lastLoginAt).toLocaleDateString('pt-BR', {
                          day: '2-digit',
                          month: '2-digit',
                          year: 'numeric',
                          hour: '2-digit',
                          minute: '2-digit'
                        })
                      : 'Nunca'}
                  </td>
                  <td className="p-4 text-right">
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button
                          variant="ghost"
                          size="icon"
                          className="hover-lift tap-scale"
                        >
                          <MoreVertical className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end" className="glass">
                        <DropdownMenuItem
                          onClick={() => handleEditUser(user)}
                          className="cursor-pointer"
                        >
                          <Edit className="mr-2 h-4 w-4" />
                          Editar Tipo
                        </DropdownMenuItem>
                        <DropdownMenuItem
                          onClick={() => handleChangePassword(user)}
                          className="cursor-pointer"
                        >
                          <Key className="mr-2 h-4 w-4" />
                          Alterar Senha
                        </DropdownMenuItem>
                        <DropdownMenuItem
                          onClick={() =>
                            handleToggleUserStatus(user.id, user.isActive)
                          }
                          className="cursor-pointer"
                        >
                          {user.isActive ? (
                            <>
                              <Ban className="mr-2 h-4 w-4" />
                              Desativar
                            </>
                          ) : (
                            <>
                              <CheckCircle className="mr-2 h-4 w-4" />
                              Ativar
                            </>
                          )}
                        </DropdownMenuItem>
                        <DropdownMenuSeparator />
                        <DropdownMenuItem
                          onClick={() => handleDeleteUser(user.id)}
                          className="cursor-pointer text-destructive focus:text-destructive"
                        >
                          <Trash2 className="mr-2 h-4 w-4" />
                          Excluir
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
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
              Mostrando {startIndex + 1} a {Math.min(endIndex, filteredUsers.length)} de {filteredUsers.length} usuários
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
                {Array.from({ length: totalPages }, (_, i) => i + 1).map((page) => (
                  <Button
                    key={page}
                    variant={currentPage === page ? 'default' : 'outline'}
                    size="sm"
                    onClick={() => setCurrentPage(page)}
                    className={`hover-lift tap-scale ${
                      currentPage === page ? 'bg-primary text-primary-foreground' : ''
                    }`}
                  >
                    {page}
                  </Button>
                ))}
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

      {/* Create User Dialog */}
      <Dialog open={createDialogOpen} onOpenChange={setCreateDialogOpen}>
        <DialogContent className="glass">
          <DialogHeader>
            <DialogTitle>Criar Novo Usuário</DialogTitle>
            <DialogDescription>
              Crie uma nova conta de usuário. Uma senha temporária será gerada.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="name">Nome</Label>
              <Input
                id="name"
                placeholder="Nome completo"
                value={newUser.name}
                onChange={(e) => setNewUser({ ...newUser, name: e.target.value })}
                className="glass"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="email">Email</Label>
              <Input
                id="email"
                type="email"
                placeholder="email@exemplo.com"
                value={newUser.email}
                onChange={(e) => setNewUser({ ...newUser, email: e.target.value })}
                className="glass"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="password">Senha Temporária</Label>
              <Input
                id="password"
                type="password"
                placeholder="Senha inicial do usuário"
                value={newUser.password}
                onChange={(e) => setNewUser({ ...newUser, password: e.target.value })}
                className="glass"
              />
              <p className="text-xs text-muted-foreground">
                O usuário deverá alterar esta senha no primeiro login.
              </p>
            </div>
            <div className="space-y-2">
              <Label htmlFor="new-role">Tipo de Usuário</Label>
              <Select value={newUser.role} onValueChange={(role) => setNewUser({ ...newUser, role })}>
                <SelectTrigger id="new-role" className="glass">
                  <SelectValue placeholder="Selecione o tipo" />
                </SelectTrigger>
                <SelectContent className="glass">
                  <SelectItem value="User">Usuário</SelectItem>
                  <SelectItem value="PersonalTrainer">Personal Trainer</SelectItem>
                  <SelectItem value="Admin">Administrador</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => {
                setCreateDialogOpen(false);
                setNewUser({ name: '', email: '', password: '', role: 'User' });
              }}
              className="hover-lift tap-scale"
            >
              Cancelar
            </Button>
            <Button
              onClick={handleCreateUser}
              className="bg-primary hover:bg-primary/90 hover-lift tap-scale"
              disabled={!newUser.name || !newUser.email || !newUser.password}
            >
              Criar Usuário
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Edit User Dialog */}
      <Dialog open={editDialogOpen} onOpenChange={setEditDialogOpen}>
        <DialogContent className="glass">
          <DialogHeader>
            <DialogTitle>Editar Tipo de Usuário</DialogTitle>
            <DialogDescription>
              Altere o tipo de usuário de {selectedUser?.name}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="role">Tipo de Usuário</Label>
              <Select value={newRole} onValueChange={setNewRole}>
                <SelectTrigger id="role" className="glass">
                  <SelectValue placeholder="Selecione o tipo" />
                </SelectTrigger>
                <SelectContent className="glass">
                  <SelectItem value="User">Usuário</SelectItem>
                  <SelectItem value="PersonalTrainer">
                    Personal Trainer
                  </SelectItem>
                  <SelectItem value="Admin">Administrador</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setEditDialogOpen(false)}
              className="hover-lift tap-scale"
            >
              Cancelar
            </Button>
            <Button
              onClick={handleUpdateUser}
              className="bg-primary hover:bg-primary/90 hover-lift tap-scale"
            >
              Salvar
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Change Password Dialog */}
      <Dialog open={passwordDialogOpen} onOpenChange={setPasswordDialogOpen}>
        <DialogContent className="glass">
          <DialogHeader>
            <DialogTitle>Alterar Senha do Usuário</DialogTitle>
            <DialogDescription>
              Defina uma nova senha para {selectedUser?.name}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="new-password">Nova Senha</Label>
              <Input
                id="new-password"
                type="password"
                placeholder="Digite a nova senha"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                className="glass"
              />
              <p className="text-xs text-muted-foreground">
                A senha deve ter no mínimo 6 caracteres.
              </p>
            </div>
          </div>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => {
                setPasswordDialogOpen(false);
                setNewPassword('');
              }}
              className="hover-lift tap-scale"
            >
              Cancelar
            </Button>
            <Button
              onClick={handleSavePassword}
              className="bg-primary hover:bg-primary/90 hover-lift tap-scale"
              disabled={!newPassword || newPassword.length < 6}
            >
              Alterar Senha
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
