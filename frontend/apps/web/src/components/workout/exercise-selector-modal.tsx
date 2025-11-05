'use client';

import { useState, useMemo } from 'react';
import { Search, X, Dumbbell } from 'lucide-react';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';

interface Exercise {
  id: string;
  name: string;
  muscleGroup: string;
  equipment?: string;
  category?: string;
  imageUrl?: string;
}

interface ExerciseSelectorModalProps {
  open: boolean;
  onClose: () => void;
  onSelectExercise: (exercise: Exercise) => void;
  currentExercise?: Exercise;
  exercises: Exercise[];
}

// Common equipment types to filter by
const EQUIPMENT_TYPES = [
  { value: 'all', label: 'Todos os equipamentos' },
  { value: 'barbell', label: 'Barra' },
  { value: 'dumbbell', label: 'Halteres' },
  { value: 'machine', label: 'Máquina' },
  { value: 'cable', label: 'Polia/Cabo' },
  { value: 'bodyweight', label: 'Peso Corporal' },
  { value: 'bench', label: 'Banco' },
  { value: 'kettlebell', label: 'Kettlebell' },
  { value: 'bands', label: 'Elásticos' },
];

export function ExerciseSelectorModal({
  open,
  onClose,
  onSelectExercise,
  currentExercise,
  exercises,
}: ExerciseSelectorModalProps) {
  const [searchTerm, setSearchTerm] = useState('');
  const [equipmentFilter, setEquipmentFilter] = useState('all');

  // Filter exercises based on search, muscle group, and equipment
  const filteredExercises = useMemo(() => {
    let filtered = exercises;

    // Filter by same muscle group as current exercise
    if (currentExercise) {
      filtered = filtered.filter(
        (ex) => ex.muscleGroup.toLowerCase() === currentExercise.muscleGroup.toLowerCase()
      );
    }

    // Filter by equipment type
    if (equipmentFilter !== 'all') {
      filtered = filtered.filter((ex) => {
        if (!ex.equipment) return false;
        return ex.equipment.toLowerCase().includes(equipmentFilter.toLowerCase());
      });
    }

    // Filter by search term
    if (searchTerm) {
      filtered = filtered.filter((ex) =>
        ex.name.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    // Exclude current exercise from results
    if (currentExercise) {
      filtered = filtered.filter((ex) => ex.id !== currentExercise.id);
    }

    return filtered;
  }, [exercises, currentExercise, equipmentFilter, searchTerm]);

  const handleSelectExercise = (exercise: Exercise) => {
    onSelectExercise(exercise);
    onClose();
    // Reset filters
    setSearchTerm('');
    setEquipmentFilter('all');
  };

  const handleClose = () => {
    onClose();
    setSearchTerm('');
    setEquipmentFilter('all');
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="max-w-2xl max-h-[80vh] flex flex-col">
        <DialogHeader>
          <DialogTitle>Substituir Exercício</DialogTitle>
          <DialogDescription>
            {currentExercise && (
              <>
                Substituindo: <strong>{currentExercise.name}</strong> ({currentExercise.muscleGroup})
              </>
            )}
          </DialogDescription>
        </DialogHeader>

        {/* Filters */}
        <div className="space-y-4">
          {/* Search Input */}
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Pesquisar exercício..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10"
            />
            {searchTerm && (
              <button
                onClick={() => setSearchTerm('')}
                className="absolute right-3 top-1/2 -translate-y-1/2"
              >
                <X className="h-4 w-4 text-muted-foreground" />
              </button>
            )}
          </div>

          {/* Equipment Filter */}
          <div>
            <label className="text-sm font-medium mb-2 block">Filtrar por equipamento:</label>
            <Select value={equipmentFilter} onValueChange={setEquipmentFilter}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {EQUIPMENT_TYPES.map((type) => (
                  <SelectItem key={type.value} value={type.value}>
                    {type.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>

        {/* Exercise List */}
        <div className="flex-1 overflow-y-auto border rounded-md p-2 space-y-2">
          {filteredExercises.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              <Dumbbell className="h-12 w-12 mx-auto mb-2 opacity-50" />
              <p>Nenhum exercício encontrado</p>
              <p className="text-sm">Tente ajustar os filtros</p>
            </div>
          ) : (
            filteredExercises.map((exercise) => (
              <button
                key={exercise.id}
                onClick={() => handleSelectExercise(exercise)}
                className="w-full p-3 border rounded-lg hover:bg-accent transition-colors text-left"
              >
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <h4 className="font-medium">{exercise.name}</h4>
                    <div className="flex gap-2 mt-1 flex-wrap">
                      <Badge variant="secondary" className="text-xs">
                        {exercise.muscleGroup}
                      </Badge>
                      {exercise.equipment && (
                        <Badge variant="outline" className="text-xs">
                          {exercise.equipment}
                        </Badge>
                      )}
                    </div>
                  </div>
                  {exercise.imageUrl && (
                    <img
                      src={exercise.imageUrl}
                      alt={exercise.name}
                      className="w-16 h-16 object-cover rounded ml-3"
                    />
                  )}
                </div>
              </button>
            ))
          )}
        </div>

        {/* Footer */}
        <div className="flex justify-between items-center pt-4 border-t">
          <p className="text-sm text-muted-foreground">
            {filteredExercises.length} exercício{filteredExercises.length !== 1 ? 's' : ''} encontrado{filteredExercises.length !== 1 ? 's' : ''}
          </p>
          <Button variant="outline" onClick={handleClose}>
            Cancelar
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
