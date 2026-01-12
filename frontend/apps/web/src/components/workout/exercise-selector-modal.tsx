'use client';

import { useState, useMemo } from 'react';
import { Search, X, Dumbbell } from 'lucide-react';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { ExerciseBadgeCompact } from '@/components/exercise/exercise-badge';

interface Exercise {
  id: string;
  name: string;
  muscleGroup: string;
  equipment?: string;
  category?: string;
  workoutLocation?: number; // 0 = Gym, 1 = Home, 2 = Both
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

// Workout location types
const LOCATION_TYPES = [
  { value: 'all', label: 'Todos os locais' },
  { value: '0', label: '🏋️ Academia' },
  { value: '1', label: '🏠 Casa' },
  { value: '2', label: '🔄 Ambos' },
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
  const [locationFilter, setLocationFilter] = useState('all');

  // Filter exercises based on search, muscle group, equipment, and location
  const filteredExercises = useMemo(() => {
    let filtered = exercises;

    // Filter by same muscle group as current exercise
    if (currentExercise) {
      filtered = filtered.filter(
        (ex) => ex.muscleGroup.toLowerCase() === currentExercise.muscleGroup.toLowerCase()
      );
    }

    // Filter by workout location
    if (locationFilter !== 'all') {
      const locationValue = parseInt(locationFilter);
      filtered = filtered.filter((ex) => {
        // If exercise has no location specified, include it
        if (ex.workoutLocation === undefined) return true;
        // Include if matches location or if exercise is marked as "Both" (2)
        return ex.workoutLocation === locationValue || ex.workoutLocation === 2;
      });
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
  }, [exercises, currentExercise, equipmentFilter, locationFilter, searchTerm]);

  const handleSelectExercise = (exercise: Exercise) => {
    onSelectExercise(exercise);
    onClose();
    // Reset filters
    setSearchTerm('');
    setEquipmentFilter('all');
    setLocationFilter('all');
  };

  const handleClose = () => {
    onClose();
    setSearchTerm('');
    setEquipmentFilter('all');
    setLocationFilter('all');
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

          {/* Filters Row */}
          <div className="grid grid-cols-2 gap-4">
            {/* Location Filter */}
            <div>
              <label className="text-sm font-medium mb-2 block">Local:</label>
              <Select value={locationFilter} onValueChange={setLocationFilter}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {LOCATION_TYPES.map((type) => (
                    <SelectItem key={type.value} value={type.value}>
                      {type.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Equipment Filter */}
            <div>
              <label className="text-sm font-medium mb-2 block">Equipamento:</label>
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
                    <div className="flex items-center gap-2 mb-1">
                      <h4 className="font-medium">{exercise.name}</h4>
                      <ExerciseBadgeCompact
                        workoutLocation={exercise.workoutLocation}
                        equipment={exercise.equipment}
                      />
                    </div>
                    <div className="flex gap-2 mt-1 flex-wrap">
                      <Badge variant="secondary" className="text-xs">
                        {exercise.muscleGroup}
                      </Badge>
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
