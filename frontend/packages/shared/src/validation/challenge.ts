import { z } from 'zod';

export const createChallengeSchema = z.object({
  name: z.string().min(2, 'Nome deve ter no mínimo 2 caracteres'),
  description: z.string().optional(),
  type: z.enum(['volume', 'sets', 'workouts', 'streak', 'exercise']),
  target: z.number().min(1, 'Meta deve ser maior que zero'),
  unit: z.string().min(1, 'Unidade é obrigatória'),
  startDate: z.string().datetime(),
  endDate: z.string().datetime(),
}).refine((data) => new Date(data.endDate) > new Date(data.startDate), {
  message: 'Data de término deve ser posterior à data de início',
  path: ['endDate'],
});

export const updateChallengeSchema = z.object({
  name: z.string().min(2, 'Nome deve ter no mínimo 2 caracteres').optional(),
  description: z.string().optional(),
  target: z.number().min(1).optional(),
  endDate: z.string().datetime().optional(),
});

export type CreateChallengeInput = z.infer<typeof createChallengeSchema>;
export type UpdateChallengeInput = z.infer<typeof updateChallengeSchema>;
