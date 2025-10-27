import { z } from 'zod';

export const createExerciseSchema = z.object({
  name: z.string().min(2, 'Nome deve ter no mínimo 2 caracteres'),
  description: z.string().optional(),
  category: z.enum(['strength', 'cardio', 'flexibility', 'plyometrics']),
  muscleGroup: z.enum(['chest', 'back', 'legs', 'shoulders', 'arms', 'core', 'full_body']),
  equipment: z.string().optional(),
  videoUrl: z.string().url('URL inválida').optional().or(z.literal('')),
  imageUrl: z.string().url('URL inválida').optional().or(z.literal('')),
  isPublic: z.boolean().default(false),
});

export const createWorkoutPlanSchema = z.object({
  name: z.string().min(2, 'Nome deve ter no mínimo 2 caracteres'),
  description: z.string().optional(),
  duration: z.number().min(1).max(52).optional(),
  goal: z.string().optional(),
});

export const createWorkoutSchema = z.object({
  planId: z.string().uuid(),
  name: z.string().min(2, 'Nome deve ter no mínimo 2 caracteres'),
  dayOfWeek: z.number().min(0).max(6).optional(),
  order: z.number().min(0),
});

export const createWorkoutExerciseSchema = z.object({
  workoutId: z.string().uuid(),
  exerciseId: z.string().uuid(),
  order: z.number().min(0),
  targetSets: z.number().min(1).max(20),
  targetReps: z.string().optional(),
  targetRpe: z.number().min(1).max(10).optional(),
  restSeconds: z.number().min(0).max(600).optional(),
  notes: z.string().optional(),
});

export const createSetSchema = z.object({
  sessionId: z.string().uuid(),
  exerciseId: z.string().uuid(),
  setNumber: z.number().min(1),
  reps: z.number().min(0).max(100).optional(),
  weight: z.number().min(0).optional(),
  rpe: z.number().min(1).max(10).optional(),
  duration: z.number().min(0).optional(),
  distance: z.number().min(0).optional(),
  notes: z.string().optional(),
});

export const updateSetSchema = z.object({
  reps: z.number().min(0).max(100).optional(),
  weight: z.number().min(0).optional(),
  rpe: z.number().min(1).max(10).optional(),
  duration: z.number().min(0).optional(),
  distance: z.number().min(0).optional(),
  notes: z.string().optional(),
});

export const startSessionSchema = z.object({
  workoutId: z.string().uuid().optional(),
  notes: z.string().optional(),
});

export const completeSessionSchema = z.object({
  notes: z.string().optional(),
});

export type CreateExerciseInput = z.infer<typeof createExerciseSchema>;
export type CreateWorkoutPlanInput = z.infer<typeof createWorkoutPlanSchema>;
export type CreateWorkoutInput = z.infer<typeof createWorkoutSchema>;
export type CreateWorkoutExerciseInput = z.infer<typeof createWorkoutExerciseSchema>;
export type CreateSetInput = z.infer<typeof createSetSchema>;
export type UpdateSetInput = z.infer<typeof updateSetSchema>;
export type StartSessionInput = z.infer<typeof startSessionSchema>;
export type CompleteSessionInput = z.infer<typeof completeSessionSchema>;
