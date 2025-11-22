'use client';

import { useEffect, useState } from 'react';
import { useAuth } from '@/hooks/use-auth';
import { useRouter } from 'next/navigation';
import {
  Megaphone,
  Plus,
  Edit,
  Trash2,
  Eye,
  EyeOff,
  Bell,
  BellOff,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
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
import { useToast } from '@/hooks/use-toast';
import { apiClient } from '@/lib/api';
import { Switch } from '@/components/ui/switch';

interface Announcement {
  id: string;
  title: string;
  content: string;
  type: string;
  imageUrl?: string;
  publishedAt: string;
  expiresAt?: string;
  priority: number;
  showAsPopup: boolean;
  isActive: boolean;
  isCurrentlyActive: boolean;
  readCount: number;
  createdAt: string;
}

interface AnnouncementFormData {
  title: string;
  content: string;
  type: string;
  imageUrl: string;
  expiresAt: string;
  priority: number;
  showAsPopup: boolean;
  isActive: boolean;
}

const ANNOUNCEMENT_TYPES = [
  { value: 'NewFeature', label: 'Nova Funcionalidade' },
  { value: 'Maintenance', label: 'Manutenção' },
  { value: 'Tips', label: 'Dicas' },
  { value: 'General', label: 'Geral' },
];

export default function AdminAnnouncementsPage() {
  const { user } = useAuth();
  const router = useRouter();
  const { toast } = useToast();
  const [announcements, setAnnouncements] = useState<Announcement[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [selectedAnnouncement, setSelectedAnnouncement] = useState<Announcement | null>(null);
  const [formData, setFormData] = useState<AnnouncementFormData>({
    title: '',
    content: '',
    type: 'General',
    imageUrl: '',
    expiresAt: '',
    priority: 3,
    showAsPopup: true,
    isActive: true,
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

  // Fetch announcements
  const fetchAnnouncements = async () => {
    try {
      setIsLoading(true);
      const data = await apiClient.get<Announcement[]>('/admin/announcements');
      setAnnouncements(data);
    } catch (error: any) {
      toast({
        title: 'Erro ao carregar anúncios',
        description: error.message || 'Ocorreu um erro ao carregar os anúncios.',
        variant: 'destructive',
      });
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    if (user?.role === 'Admin') {
      fetchAnnouncements();
    }
  }, [user]);

  // Handle create/edit
  const handleSave = async () => {
    try {
      if (!formData.title.trim()) {
        toast({
          title: 'Título obrigatório',
          description: 'Por favor, insira um título para o anúncio.',
          variant: 'destructive',
        });
        return;
      }

      if (!formData.content.trim()) {
        toast({
          title: 'Conteúdo obrigatório',
          description: 'Por favor, insira o conteúdo do anúncio.',
          variant: 'destructive',
        });
        return;
      }

      const payload = {
        ...formData,
        expiresAt: formData.expiresAt ? new Date(formData.expiresAt).toISOString() : null,
        imageUrl: formData.imageUrl || null,
      };

      if (selectedAnnouncement) {
        // Update existing
        await apiClient.put(`/admin/announcements/${selectedAnnouncement.id}`, payload);
        toast({
          title: 'Anúncio atualizado',
          description: 'O anúncio foi atualizado com sucesso.',
        });
      } else {
        // Create new
        await apiClient.post('/admin/announcements', payload);
        toast({
          title: 'Anúncio criado',
          description: 'O anúncio foi criado com sucesso.',
        });
      }

      setDialogOpen(false);
      setSelectedAnnouncement(null);
      resetForm();
      fetchAnnouncements();
    } catch (error: any) {
      toast({
        title: 'Erro ao salvar anúncio',
        description: error.message || 'Ocorreu um erro ao salvar o anúncio.',
        variant: 'destructive',
      });
    }
  };

  // Handle delete
  const handleDelete = async () => {
    if (!selectedAnnouncement) return;

    try {
      await apiClient.delete(`/admin/announcements/${selectedAnnouncement.id}`);
      toast({
        title: 'Anúncio excluído',
        description: 'O anúncio foi excluído com sucesso.',
      });
      setDeleteDialogOpen(false);
      setSelectedAnnouncement(null);
      fetchAnnouncements();
    } catch (error: any) {
      toast({
        title: 'Erro ao excluir anúncio',
        description: error.message || 'Ocorreu um erro ao excluir o anúncio.',
        variant: 'destructive',
      });
    }
  };

  // Reset form
  const resetForm = () => {
    setFormData({
      title: '',
      content: '',
      type: 'General',
      imageUrl: '',
      expiresAt: '',
      priority: 3,
      showAsPopup: true,
      isActive: true,
    });
  };

  // Open create dialog
  const openCreateDialog = () => {
    resetForm();
    setSelectedAnnouncement(null);
    setDialogOpen(true);
  };

  // Open edit dialog
  const openEditDialog = (announcement: Announcement) => {
    setSelectedAnnouncement(announcement);
    setFormData({
      title: announcement.title,
      content: announcement.content,
      type: announcement.type,
      imageUrl: announcement.imageUrl || '',
      expiresAt: announcement.expiresAt
        ? new Date(announcement.expiresAt).toISOString().slice(0, 16)
        : '',
      priority: announcement.priority,
      showAsPopup: announcement.showAsPopup,
      isActive: announcement.isActive,
    });
    setDialogOpen(true);
  };

  // Open delete dialog
  const openDeleteDialog = (announcement: Announcement) => {
    setSelectedAnnouncement(announcement);
    setDeleteDialogOpen(true);
  };

  // Get type badge
  const getTypeBadge = (type: string) => {
    const typeConfig = ANNOUNCEMENT_TYPES.find((t) => t.value === type);
    return (
      <Badge variant="outline" className="ml-2">
        {typeConfig?.label || type}
      </Badge>
    );
  };

  // Get priority badge
  const getPriorityBadge = (priority: number) => {
    const colors: Record<number, string> = {
      1: 'bg-red-100 text-red-800',
      2: 'bg-orange-100 text-orange-800',
      3: 'bg-yellow-100 text-yellow-800',
      4: 'bg-blue-100 text-blue-800',
      5: 'bg-gray-100 text-gray-800',
    };
    return (
      <Badge className={colors[priority] || colors[3]}>
        Prioridade {priority}
      </Badge>
    );
  };

  if (user?.role !== 'Admin') {
    return null;
  }

  return (
    <div className="container mx-auto py-8 px-4">
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-3xl font-bold flex items-center gap-2">
            <Megaphone className="h-8 w-8" />
            Gerenciar Anúncios
          </h1>
          <p className="text-muted-foreground mt-2">
            Crie e gerencie anúncios e notificações para os usuários da plataforma
          </p>
        </div>
        <Button onClick={openCreateDialog}>
          <Plus className="h-4 w-4 mr-2" />
          Novo Anúncio
        </Button>
      </div>

      {isLoading ? (
        <div className="text-center py-12">
          <p className="text-muted-foreground">Carregando anúncios...</p>
        </div>
      ) : announcements.length === 0 ? (
        <Card>
          <CardContent className="py-12 text-center">
            <Megaphone className="h-12 w-12 mx-auto text-muted-foreground mb-4" />
            <p className="text-muted-foreground">
              Nenhum anúncio criado ainda. Crie o primeiro anúncio!
            </p>
          </CardContent>
        </Card>
      ) : (
        <div className="grid gap-4">
          {announcements.map((announcement) => (
            <Card key={announcement.id}>
              <CardHeader>
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-center gap-2">
                      <CardTitle>{announcement.title}</CardTitle>
                      {getTypeBadge(announcement.type)}
                    </div>
                    <div className="flex items-center gap-2 mt-2">
                      {getPriorityBadge(announcement.priority)}
                      {announcement.showAsPopup && (
                        <Badge variant="secondary">
                          <Bell className="h-3 w-3 mr-1" />
                          Popup
                        </Badge>
                      )}
                      {announcement.isActive ? (
                        <Badge variant="default" className="bg-green-600">
                          <Eye className="h-3 w-3 mr-1" />
                          Ativo
                        </Badge>
                      ) : (
                        <Badge variant="secondary">
                          <EyeOff className="h-3 w-3 mr-1" />
                          Inativo
                        </Badge>
                      )}
                      {!announcement.isCurrentlyActive && (
                        <Badge variant="destructive">Expirado</Badge>
                      )}
                    </div>
                  </div>
                  <div className="flex gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => openEditDialog(announcement)}
                    >
                      <Edit className="h-4 w-4" />
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => openDeleteDialog(announcement)}
                    >
                      <Trash2 className="h-4 w-4 text-destructive" />
                    </Button>
                  </div>
                </div>
              </CardHeader>
              <CardContent>
                <p className="text-sm text-muted-foreground mb-4 whitespace-pre-wrap">
                  {announcement.content}
                </p>
                {announcement.imageUrl && (
                  <div className="mb-4">
                    <img
                      src={announcement.imageUrl}
                      alt={announcement.title}
                      className="rounded-lg max-h-48 object-cover"
                    />
                  </div>
                )}
                <div className="flex items-center gap-4 text-sm text-muted-foreground">
                  <span>
                    Publicado em: {new Date(announcement.publishedAt).toLocaleString('pt-BR')}
                  </span>
                  {announcement.expiresAt && (
                    <span>
                      Expira em: {new Date(announcement.expiresAt).toLocaleString('pt-BR')}
                    </span>
                  )}
                  <span>Leituras: {announcement.readCount}</span>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {/* Create/Edit Dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>
              {selectedAnnouncement ? 'Editar Anúncio' : 'Novo Anúncio'}
            </DialogTitle>
            <DialogDescription>
              {selectedAnnouncement
                ? 'Atualize as informações do anúncio.'
                : 'Crie um novo anúncio para os usuários da plataforma.'}
            </DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="title">Título *</Label>
              <Input
                id="title"
                value={formData.title}
                onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                placeholder="Título do anúncio"
                maxLength={200}
              />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="content">Conteúdo *</Label>
              <Textarea
                id="content"
                value={formData.content}
                onChange={(e) => setFormData({ ...formData, content: e.target.value })}
                placeholder="Conteúdo do anúncio"
                rows={6}
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="grid gap-2">
                <Label htmlFor="type">Tipo</Label>
                <Select
                  value={formData.type}
                  onValueChange={(value) => setFormData({ ...formData, type: value })}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {ANNOUNCEMENT_TYPES.map((type) => (
                      <SelectItem key={type.value} value={type.value}>
                        {type.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="grid gap-2">
                <Label htmlFor="priority">Prioridade (1-5)</Label>
                <Select
                  value={formData.priority.toString()}
                  onValueChange={(value) =>
                    setFormData({ ...formData, priority: parseInt(value) })
                  }
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="1">1 - Urgente</SelectItem>
                    <SelectItem value="2">2 - Alta</SelectItem>
                    <SelectItem value="3">3 - Média</SelectItem>
                    <SelectItem value="4">4 - Baixa</SelectItem>
                    <SelectItem value="5">5 - Muito Baixa</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>

            <div className="grid gap-2">
              <Label htmlFor="imageUrl">URL da Imagem (opcional)</Label>
              <Input
                id="imageUrl"
                value={formData.imageUrl}
                onChange={(e) => setFormData({ ...formData, imageUrl: e.target.value })}
                placeholder="https://exemplo.com/imagem.jpg"
              />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="expiresAt">Data de Expiração (opcional)</Label>
              <Input
                id="expiresAt"
                type="datetime-local"
                value={formData.expiresAt}
                onChange={(e) => setFormData({ ...formData, expiresAt: e.target.value })}
              />
            </div>

            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-2">
                <Switch
                  id="showAsPopup"
                  checked={formData.showAsPopup}
                  onCheckedChange={(checked) =>
                    setFormData({ ...formData, showAsPopup: checked })
                  }
                />
                <Label htmlFor="showAsPopup">Exibir como popup</Label>
              </div>

              <div className="flex items-center space-x-2">
                <Switch
                  id="isActive"
                  checked={formData.isActive}
                  onCheckedChange={(checked) =>
                    setFormData({ ...formData, isActive: checked })
                  }
                />
                <Label htmlFor="isActive">Ativo</Label>
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogOpen(false)}>
              Cancelar
            </Button>
            <Button onClick={handleSave}>
              {selectedAnnouncement ? 'Atualizar' : 'Criar'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Excluir Anúncio</DialogTitle>
            <DialogDescription>
              Tem certeza que deseja excluir o anúncio &ldquo;{selectedAnnouncement?.title}&rdquo;? Esta
              ação não pode ser desfeita.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteDialogOpen(false)}>
              Cancelar
            </Button>
            <Button variant="destructive" onClick={handleDelete}>
              Excluir
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
