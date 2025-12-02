'use client';

import { useEffect, useState } from 'react';
import { useAuth } from '@/hooks/use-auth';
import { useRouter } from 'next/navigation';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Dumbbell,
  Search,
  Filter,
  X,
  Edit,
  Save,
  ImageOff,
  VideoOff,
  Languages,
  CheckCircle2,
  AlertTriangle,
  Plus,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { useToast } from '@/hooks/use-toast';
import { api } from '@/lib/api';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import type { Exercise, ExerciseCategory, MuscleGroup } from '@gymhero/shared';

type FilterType = 'all' | 'no-translation' | 'no-image' | 'no-video' | 'complete';

export default function AdminExercisesPage() {
  const { user } = useAuth();
  const router = useRouter();
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const [searchTerm, setSearchTerm] = useState('');
  const [filterType, setFilterType] = useState<FilterType>('all');
  const [editingExercise, setEditingExercise] = useState<Exercise | null>(null);
  const [editDialogOpen, setEditDialogOpen] = useState(false);

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

  const { data: exercises = [], isLoading } = useQuery({
    queryKey: ['exercises', 'admin'],
    queryFn: () => api.exercises.getAll(),
  });

  const updateMutation = useMutation({
    mutationFn: (data: { id: string; exercise: Partial<Exercise> }) =>
      api.exercises.update(data.id, data.exercise),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['exercises'] });
      toast({
        title: 'Exercício atualizado!',
        description: 'As alterações foram salvas com sucesso.',
      });
      setEditDialogOpen(false);
      setEditingExercise(null);
    },
    onError: (error: any) => {
      toast({
        variant: 'destructive',
        title: 'Erro ao atualizar',
        description: error.message || 'Não foi possível atualizar o exercício.',
      });
    },
  });

  const handleEdit = (exercise: Exercise) => {
    setEditingExercise({ ...exercise });
    setEditDialogOpen(true);
  };

  const handleSave = () => {
    if (!editingExercise) return;
    updateMutation.mutate({
      id: editingExercise.id,
      exercise: editingExercise,
    });
  };

  // Filter exercises
  const filteredExercises = exercises.filter((exercise) => {
    // Search filter
    const matchesSearch = searchTerm
      ? exercise.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        exercise.description?.toLowerCase().includes(searchTerm.toLowerCase())
      : true;

    // Type filter
    let matchesFilter = true;
    switch (filterType) {
      case 'no-translation':
        matchesFilter = !exercise.description || exercise.description.trim() === '';
        break;
      case 'no-image':
        matchesFilter = !exercise.imageUrl || exercise.imageUrl.trim() === '';
        break;
      case 'no-video':
        matchesFilter = !exercise.videoUrl || exercise.videoUrl.trim() === '';
        break;
      case 'complete':
        matchesFilter = !!(exercise.description && exercise.imageUrl && exercise.videoUrl);
        break;
      case 'all':
      default:
        matchesFilter = true;
    }

    return matchesSearch && matchesFilter;
  });

  const stats = {
    total: exercises.length,
    noTranslation: exercises.filter(e => !e.description || e.description.trim() === '').length,
    noImage: exercises.filter(e => !e.imageUrl || e.imageUrl.trim() === '').length,
    noVideo: exercises.filter(e => !e.videoUrl || e.videoUrl.trim() === '').length,
    complete: exercises.filter(e => e.description && e.imageUrl && e.videoUrl).length,
  };

  if (user?.role !== 'Admin') {
    return null;
  }

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Header */}
      <div className="glass rounded-xl p-6 border hover-lift">
        <div className="flex items-center gap-3 mb-2">
          <Dumbbell className="h-8 w-8 text-primary animate-glow-pulse" />
          <h1 className="text-3xl font-bold bg-gradient-to-r from-primary to-primary/70 bg-clip-text text-transparent">
            Gerenciar Exercícios
          </h1>
        </div>
        <p className="text-muted-foreground">
          Edite exercícios e filtre aqueles que precisam de atenção
        </p>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-5">
        <Card className="glass border-primary/20 hover-lift tap-scale">
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Total
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.total}</div>
          </CardContent>
        </Card>

        <Card
          className={`glass border-orange-500/20 hover-lift tap-scale cursor-pointer ${filterType === 'no-translation' ? 'ring-2 ring-orange-500' : ''}`}
          onClick={() => setFilterType(filterType === 'no-translation' ? 'all' : 'no-translation')}
        >
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-orange-500 flex items-center gap-2">
              <Languages className="h-4 w-4" />
              Sem Tradução
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-orange-500">{stats.noTranslation}</div>
          </CardContent>
        </Card>

        <Card
          className={`glass border-red-500/20 hover-lift tap-scale cursor-pointer ${filterType === 'no-image' ? 'ring-2 ring-red-500' : ''}`}
          onClick={() => setFilterType(filterType === 'no-image' ? 'all' : 'no-image')}
        >
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-red-500 flex items-center gap-2">
              <ImageOff className="h-4 w-4" />
              Sem Imagem
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-red-500">{stats.noImage}</div>
          </CardContent>
        </Card>

        <Card
          className={`glass border-yellow-500/20 hover-lift tap-scale cursor-pointer ${filterType === 'no-video' ? 'ring-2 ring-yellow-500' : ''}`}
          onClick={() => setFilterType(filterType === 'no-video' ? 'all' : 'no-video')}
        >
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-yellow-500 flex items-center gap-2">
              <VideoOff className="h-4 w-4" />
              Sem Vídeo
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-yellow-500">{stats.noVideo}</div>
          </CardContent>
        </Card>

        <Card
          className={`glass border-green-500/20 hover-lift tap-scale cursor-pointer ${filterType === 'complete' ? 'ring-2 ring-green-500' : ''}`}
          onClick={() => setFilterType(filterType === 'complete' ? 'all' : 'complete')}
        >
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-green-500 flex items-center gap-2">
              <CheckCircle2 className="h-4 w-4" />
              Completos
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-green-500">{stats.complete}</div>
          </CardContent>
        </Card>
      </div>

      {/* Search and Filters */}
      <Card className="glass border-primary/20">
        <CardContent className="p-4">
          <div className="flex flex-col sm:flex-row gap-3">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                placeholder="Buscar por nome ou descrição..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10 glass"
              />
            </div>
            {filterType !== 'all' && (
              <Button
                variant="outline"
                onClick={() => setFilterType('all')}
                className="hover-lift tap-scale"
              >
                <X className="h-4 w-4 mr-2" />
                Limpar Filtro
              </Button>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Results */}
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <p className="text-sm text-muted-foreground">
            {filteredExercises.length} exercício{filteredExercises.length !== 1 ? 's' : ''} encontrado{filteredExercises.length !== 1 ? 's' : ''}
          </p>
        </div>

        {isLoading ? (
          <div className="grid gap-4">
            {[1, 2, 3, 4, 5].map((i) => (
              <Card key={i} className="glass border-primary/20 animate-pulse">
                <CardContent className="p-6">
                  <div className="space-y-3">
                    <div className="h-6 bg-muted rounded w-1/3" />
                    <div className="h-4 bg-muted rounded w-2/3" />
                    <div className="flex gap-2">
                      <div className="h-6 bg-muted rounded w-20" />
                      <div className="h-6 bg-muted rounded w-20" />
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        ) : filteredExercises.length > 0 ? (
          <div className="grid gap-4">
            {filteredExercises.map((exercise, index) => {
              const hasTranslation = exercise.description && exercise.description.trim() !== '';
              const hasImage = exercise.imageUrl && exercise.imageUrl.trim() !== '';
              const hasVideo = exercise.videoUrl && exercise.videoUrl.trim() !== '';
              const isComplete = hasTranslation && hasImage && hasVideo;

              return (
                <Card
                  key={exercise.id}
                  className="glass border-primary/20 hover-lift tap-scale animate-scale-in"
                  style={{ animationDelay: `${index * 30}ms` }}
                >
                  <CardContent className="p-6">
                    <div className="flex items-start justify-between gap-4">
                      <div className="flex-1 space-y-3">
                        <div>
                          <h3 className="font-bold text-lg mb-1">{exercise.name}</h3>
                          {exercise.description ? (
                            <p className="text-sm text-muted-foreground line-clamp-2">
                              {exercise.description}
                            </p>
                          ) : (
                            <p className="text-sm text-orange-500 italic">
                              Sem descrição/tradução
                            </p>
                          )}
                        </div>

                        <div className="flex flex-wrap gap-2">
                          <Badge variant="outline" className="bg-primary/10">
                            {exercise.muscleGroup}
                          </Badge>
                          <Badge variant="outline" className="bg-blue-500/10 text-blue-500">
                            {exercise.category}
                          </Badge>
                          {exercise.equipment && (
                            <Badge variant="outline">{exercise.equipment}</Badge>
                          )}
                        </div>

                        <div className="flex gap-3 text-sm">
                          <div className={`flex items-center gap-1 ${hasTranslation ? 'text-green-500' : 'text-orange-500'}`}>
                            <Languages className="h-4 w-4" />
                            {hasTranslation ? 'Traduzido' : 'Sem tradução'}
                          </div>
                          <div className={`flex items-center gap-1 ${hasImage ? 'text-green-500' : 'text-red-500'}`}>
                            {hasImage ? <CheckCircle2 className="h-4 w-4" /> : <ImageOff className="h-4 w-4" />}
                            {hasImage ? 'Com imagem' : 'Sem imagem'}
                          </div>
                          <div className={`flex items-center gap-1 ${hasVideo ? 'text-green-500' : 'text-yellow-500'}`}>
                            {hasVideo ? <CheckCircle2 className="h-4 w-4" /> : <VideoOff className="h-4 w-4" />}
                            {hasVideo ? 'Com vídeo' : 'Sem vídeo'}
                          </div>
                        </div>
                      </div>

                      <div className="flex flex-col gap-2">
                        {isComplete ? (
                          <Badge className="bg-green-500/20 text-green-500 border-green-500/30">
                            <CheckCircle2 className="h-3 w-3 mr-1" />
                            Completo
                          </Badge>
                        ) : (
                          <Badge variant="outline" className="bg-orange-500/20 text-orange-500 border-orange-500/30">
                            <AlertTriangle className="h-3 w-3 mr-1" />
                            Incompleto
                          </Badge>
                        )}
                        <Button
                          size="sm"
                          onClick={() => handleEdit(exercise)}
                          className="hover-lift tap-scale"
                        >
                          <Edit className="h-4 w-4 mr-2" />
                          Editar
                        </Button>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              );
            })}
          </div>
        ) : (
          <Card className="glass border-primary/20">
            <CardContent className="py-12 text-center">
              <Dumbbell className="h-16 w-16 text-muted-foreground mx-auto mb-4 opacity-50" />
              <h3 className="text-lg font-semibold mb-2">Nenhum exercício encontrado</h3>
              <p className="text-muted-foreground">
                {searchTerm
                  ? 'Tente ajustar sua busca ou filtros'
                  : 'Não há exercícios com os filtros selecionados'}
              </p>
            </CardContent>
          </Card>
        )}
      </div>

      {/* Edit Dialog */}
      <Dialog open={editDialogOpen} onOpenChange={setEditDialogOpen}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Editar Exercício</DialogTitle>
          </DialogHeader>
          {editingExercise && (
            <div className="space-y-4">
              <div>
                <Label>Nome</Label>
                <Input
                  value={editingExercise.name}
                  onChange={(e) =>
                    setEditingExercise({ ...editingExercise, name: e.target.value })
                  }
                  placeholder="Nome do exercício"
                />
              </div>

              <div>
                <Label>Descrição / Tradução</Label>
                <Textarea
                  value={editingExercise.description || ''}
                  onChange={(e) =>
                    setEditingExercise({ ...editingExercise, description: e.target.value })
                  }
                  placeholder="Descrição detalhada do exercício..."
                  rows={4}
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label>Grupo Muscular</Label>
                  <Select
                    value={editingExercise.muscleGroup}
                    onValueChange={(value) =>
                      setEditingExercise({ ...editingExercise, muscleGroup: value as MuscleGroup })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="chest">Peito</SelectItem>
                      <SelectItem value="back">Costas</SelectItem>
                      <SelectItem value="shoulders">Ombros</SelectItem>
                      <SelectItem value="arms">Braços</SelectItem>
                      <SelectItem value="legs">Pernas</SelectItem>
                      <SelectItem value="core">Core</SelectItem>
                      <SelectItem value="full_body">Corpo Todo</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                <div>
                  <Label>Categoria</Label>
                  <Select
                    value={editingExercise.category}
                    onValueChange={(value) =>
                      setEditingExercise({ ...editingExercise, category: value as ExerciseCategory })
                    }
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="strength">Força</SelectItem>
                      <SelectItem value="cardio">Cardio</SelectItem>
                      <SelectItem value="flexibility">Flexibilidade</SelectItem>
                      <SelectItem value="balance">Equilíbrio</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>

              <div>
                <Label>Equipamento</Label>
                <Input
                  value={editingExercise.equipment || ''}
                  onChange={(e) =>
                    setEditingExercise({ ...editingExercise, equipment: e.target.value })
                  }
                  placeholder="Ex: Barra, Halteres, Máquina Smith..."
                />
              </div>

              <div>
                <Label>URL da Imagem</Label>
                <Input
                  value={editingExercise.imageUrl || ''}
                  onChange={(e) =>
                    setEditingExercise({ ...editingExercise, imageUrl: e.target.value })
                  }
                  placeholder="https://exemplo.com/imagem.jpg"
                />
              </div>

              <div>
                <Label>URL do Vídeo</Label>
                <Input
                  value={editingExercise.videoUrl || ''}
                  onChange={(e) =>
                    setEditingExercise({ ...editingExercise, videoUrl: e.target.value })
                  }
                  placeholder="https://youtube.com/watch?v=..."
                />
              </div>

              <div className="flex gap-3 pt-4">
                <Button
                  onClick={handleSave}
                  disabled={updateMutation.isPending}
                  className="flex-1 hover-lift tap-scale"
                >
                  <Save className="h-4 w-4 mr-2" />
                  {updateMutation.isPending ? 'Salvando...' : 'Salvar Alterações'}
                </Button>
                <Button
                  variant="outline"
                  onClick={() => setEditDialogOpen(false)}
                  className="hover-lift tap-scale"
                >
                  Cancelar
                </Button>
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}
