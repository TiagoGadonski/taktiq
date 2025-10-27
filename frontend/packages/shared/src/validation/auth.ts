import { z } from 'zod';

export const loginSchema = z.object({
  email: z.string().email('Informe um email válido'),
  password: z.string().min(6, 'A senha deve conter pelo menos 6 caracteres'),
});

export const signupSchema = z.object({
  name: z.string().min(2, 'Informe seu nome completo'),
  email: z.string().email('Informe um email válido'),
  password: z.string().min(6, 'A senha deve conter pelo menos 6 caracteres'),
});

export type LoginInput = z.infer<typeof loginSchema>;
export type SignupInput = z.infer<typeof signupSchema>;
