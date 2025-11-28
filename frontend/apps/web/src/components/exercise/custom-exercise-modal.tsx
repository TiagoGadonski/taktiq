'use client';

import { useState } from 'react';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { ImageUpload } from '@/components/media/image-upload';
import { VideoUpload } from '@/components/media/video-upload';
import { toast } from '@/components/ui/use-toast';
import { apiClient } from '@/lib/api';
import { Loader2, Dumbbell, Info } from 'lucide-react';
import { Alert, AlertDescription } from '@/components/ui/alert';

interface CustomExerciseModalProps {
  open: boolean;
  onClose: () => void;
  onExerciseCreated: (exercise: any) => void;
}

const muscleGroups = [
  'Chest',
  'Back',
  'Shoulders',
  'Biceps',
  'Triceps',
  'Legs',
  'Quadriceps',
  'Hamstrings',
  'Glutes',
  'Calves',
  'Abs',
  'Core',
  'Other'
];

const muscleGroupsTranslated: Record<string, string> = {
  'Chest': 'Peito',
  'Back': 'Costas',
  'Shoulders': 'Ombros',
  'Biceps': 'Bíceps',
  'Triceps': 'Tríceps',
  'Legs': 'Pernas',
  'Quadriceps': 'Quadríceps',
  'Hamstrings': 'Posteriores de Coxa',
  'Glutes': 'Glúteos',
  'Calves': 'Panturrilhas',
  'Abs': 'Abdômen',
  'Core': 'Core',
  'Other': 'Outro'
};

const categories = [
  { value: 'strength', label: 'Força' },
  { value: 'cardio', label: 'Cardio' },
  { value: 'flexibility', label: 'Flexibilidade' },
  { value: 'plyometrics', label: 'Pliometria' },
  { value: 'powerlifting', label: 'Powerlifting' },
  { value: 'strongman', label: 'Strongman' },
  { value: 'olympic_weightlifting', label: 'Levantamento Olímpico' },
];

export function CustomExerciseModal({ open, onClose, onExerciseCreated }: CustomExerciseModalProps) {
  const [isCreating, setIsCreating] = useState(false);
  const [name, setName] = useState('');
  const [muscleGroup, setMuscleGroup] = useState('Chest');
  const [category, setCategory] = useState('strength');
  const [equipment, setEquipment] = useState('');
  const [notes, setNotes] = useState('');
  const [imageUrl, setImageUrl] = useState('');
  const [videoUrl, setVideoUrl] = useState('');

  const handleCreate = async () => {
    if (!name.trim()) {
      toast({
        variant: 'destructive',
        title: 'Nome obrigatório',
        description: 'O exercício precisa ter um nome.',
      });
      return;
    }

    setIsCreating(true);
    try {
      const exercise = await apiClient.post('/exercises', {
        name: name.trim(),
        muscleGroup,
        category,
        equipment: equipment.trim() || 'bodyweight',
        notes: notes.trim() || null,
        imageUrl: imageUrl || null,
        videoUrl: videoUrl || null,
      });

      toast({
        title: 'Exercício criado!',
        description: `${name} foi adicionado à sua biblioteca.`,
      });

      // Return the created exercise with additional info
      onExerciseCreated({
        ...(exercise as any),
        namePt: name,
        primaryMuscles: [muscleGroup],
        primaryMusclesPt: [muscleGroupsTranslated[muscleGroup] || muscleGroup],
        equipmentPt: equipment || 'Peso corporal',
        gifUrl: imageUrl,
        instructions: notes ? notes.split('\n').filter(line => line.trim()) : [],
      });

      // Reset form
      setName('');
      setMuscleGroup('Chest');
      setCategory('strength');
      setEquipment('');
      setNotes('');
      setImageUrl('');
      setVideoUrl('');

      onClose();
    } catch (error: any) {
      toast({
        variant: 'destructive',
        title: 'Erro ao criar exercício',
        description: error.response?.data?.message || 'Não foi possível criar o exercício. Tente novamente.',
      });
    } finally {
      setIsCreating(false);
    }
  };

  const handleCancel = () => {
    if (name || notes || imageUrl || videoUrl) {
      if (!confirm('Deseja descartar as alterações?')) {
        return;
      }
    }

    setName('');
    setMuscleGroup('Chest');
    setCategory('strength');
    setEquipment('');
    setNotes('');
    setImageUrl('');
    setVideoUrl('');
    onClose();
  };

  return (
    <Dialog open={open} onOpenChange={handleCancel}>
      <DialogContent className="max-w-3xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2 text-2xl">
            <Dumbbell className="h-6 w-6 text-primary" />
            Criar Exercício Personalizado
          </DialogTitle>
          <DialogDescription>
            Crie um exercício customizado com vídeos, imagens e instruções detalhadas.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-6 py-4">
          {/* Info Alert */}
          <Alert>
            <Info className="h-4 w-4" />
            <AlertDescription>
              Este exercício será adicionado à sua biblioteca pessoal e poderá ser reutilizado em outros planos.
            </AlertDescription>
          </Alert>

          {/* Basic Info */}
          <div className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="exercise-name" className="text-sm font-medium">
                Nome do Exercício *
              </Label>
              <Input
                id="exercise-name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="Ex: Agachamento com barra alta"
                disabled={isCreating}
              />
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="space-y-2">
                <Label htmlFor="muscle-group" className="text-sm font-medium">
                  Grupo Muscular *
                </Label>
                <Select value={muscleGroup} onValueChange={setMuscleGroup} disabled={isCreating}>
                  <SelectTrigger id="muscle-group">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {muscleGroups.map((muscle) => (
                      <SelectItem key={muscle} value={muscle}>
                        {muscleGroupsTranslated[muscle] || muscle}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label htmlFor="category" className="text-sm font-medium">
                  Categoria *
                </Label>
                <Select value={category} onValueChange={setCategory} disabled={isCreating}>
                  <SelectTrigger id="category">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {categories.map((cat) => (
                      <SelectItem key={cat.value} value={cat.value}>
                        {cat.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label htmlFor="equipment" className="text-sm font-medium">
                  Equipamento
                </Label>
                <Input
                  id="equipment"
                  value={equipment}
                  onChange={(e) => setEquipment(e.target.value)}
                  placeholder="Ex: Barra, Halteres"
                  disabled={isCreating}
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="notes" className="text-sm font-medium">
                Instruções de Execução
              </Label>
              <Textarea
                id="notes"
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                placeholder="Descreva como executar o exercício:&#10;1. Posicione-se com os pés afastados na largura dos ombros&#10;2. Desça controladamente mantendo as costas retas&#10;3. Retorne à posição inicial..."
                rows={6}
                disabled={isCreating}
              />
              <p className="text-xs text-muted-foreground">
                Cada linha será uma instrução separada
              </p>
            </div>
          </div>

          {/* Media Uploads */}
          <div className="space-y-4">
            <div className="space-y-2">
              <Label className="text-sm font-medium">Imagem/GIF Demonstrativo</Label>
              <ImageUpload
                onImageUploaded={setImageUrl}
                currentImageUrl={imageUrl}
                usageContext="ExerciseImage"
              />
              <p className="text-xs text-muted-foreground">
                Adicione uma imagem ou GIF mostrando a execução do exercício
              </p>
            </div>

            <div className="space-y-2">
              <Label className="text-sm font-medium">Vídeo Tutorial</Label>
              <VideoUpload
                onVideoUploaded={setVideoUrl}
                currentVideoUrl={videoUrl}
                usageContext="ExerciseVideo"
              />
              <p className="text-xs text-muted-foreground">
                Adicione um vídeo detalhado mostrando a técnica correta (até 100MB)
              </p>
            </div>
          </div>

          {/* Actions */}
          <div className="flex gap-3 pt-4 border-t">
            <Button
              onClick={handleCreate}
              disabled={isCreating || !name.trim()}
              className="flex-1"
            >
              {isCreating ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  Criando...
                </>
              ) : (
                <>
                  <Dumbbell className="h-4 w-4 mr-2" />
                  Criar Exercício
                </>
              )}
            </Button>
            <Button
              variant="outline"
              onClick={handleCancel}
              disabled={isCreating}
              className="flex-1"
            >
              Cancelar
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
