import type { ApiClient } from './client';
import type {
  User,
  AuthTokens,
  LoginRequest,
  SignupRequest,
  Exercise,
  WorkoutPlan,
  Workout,
  WorkoutExercise,
  WorkoutSession,
  WorkoutSet,
  CreateSetRequest,
  ProgressDashboard,
  Challenge,
  CreateChallengeRequest,
  Badge,
  PaginatedResponse,
} from '../types';

// Backend response type
interface BackendAuthResponse {
  id: string;
  name: string;
  email: string;
  token: string;
}

export class AuthApi {
  constructor(private client: ApiClient) {}

  async login(data: LoginRequest): Promise<AuthTokens> {
    const response = await this.client.post<BackendAuthResponse>('/auth/login', data);
    return {
      accessToken: response.token,
      refreshToken: response.token, // Backend doesn't have refresh tokens yet, using same token
      expiresIn: 3600,
    };
  }

  async signup(data: SignupRequest): Promise<AuthTokens> {
    const response = await this.client.post<BackendAuthResponse>('/auth/signup', data);
    return {
      accessToken: response.token,
      refreshToken: response.token, // Backend doesn't have refresh tokens yet, using same token
      expiresIn: 3600,
    };
  }

  async refresh(refreshToken: string): Promise<AuthTokens> {
    // Backend doesn't have refresh endpoint yet, return same token
    return {
      accessToken: refreshToken,
      refreshToken: refreshToken,
      expiresIn: 3600,
    };
  }

  async logout(): Promise<void> {
    // Backend might not have logout endpoint, just clear tokens on frontend
    return Promise.resolve();
  }

  async getMe(): Promise<User> {
    return this.client.get<User>('/me');
  }

  async updateProfile(data: Partial<User>): Promise<User> {
    return this.client.patch<User>('/me', data);
  }

  async forgotPassword(data: { email: string }): Promise<{ message: string }> {
    return this.client.post<{ message: string }>('/auth/forgot-password', data);
  }

  async resetPassword(data: {
    token: string;
    newPassword: string;
    confirmPassword: string;
  }): Promise<{ message: string }> {
    return this.client.post<{ message: string }>('/auth/reset-password', data);
  }
}

export class ExerciseApi {
  constructor(private client: ApiClient) {}

  async getAll(params?: {
    category?: string;
    muscleGroup?: string;
    search?: string;
  }): Promise<Exercise[]> {
    return this.client.get<Exercise[]>('/exercises', { params });
  }

  async getById(id: string): Promise<Exercise> {
    return this.client.get<Exercise>(`/exercises/${id}`);
  }

  async create(data: Partial<Exercise>): Promise<Exercise> {
    return this.client.post<Exercise>('/exercises', data);
  }

  async update(id: string, data: Partial<Exercise>): Promise<Exercise> {
    return this.client.put<Exercise>(`/exercises/${id}`, data);
  }

  async delete(id: string): Promise<void> {
    return this.client.delete<void>(`/exercises/${id}`);
  }
}

export class WorkoutPlanApi {
  constructor(private client: ApiClient) {}

  async getAll(): Promise<WorkoutPlan[]> {
    return this.client.get<WorkoutPlan[]>('/workout-plans');
  }

  async getById(id: string): Promise<WorkoutPlan> {
    return this.client.get<WorkoutPlan>(`/workout-plans/${id}`);
  }

  async create(data: Partial<WorkoutPlan>): Promise<WorkoutPlan> {
    return this.client.post<WorkoutPlan>('/workout-plans', data);
  }

  async update(id: string, data: Partial<WorkoutPlan>): Promise<WorkoutPlan> {
    return this.client.put<WorkoutPlan>(`/workout-plans/${id}`, data);
  }

  async delete(id: string): Promise<void> {
    return this.client.delete<void>(`/workout-plans/${id}`);
  }

  async setActive(id: string): Promise<WorkoutPlan> {
    return this.client.patch<WorkoutPlan>(`/workout-plans/${id}/activate`, {});
  }

  async setInactive(id: string): Promise<void> {
    return this.client.patch<void>(`/workout-plans/${id}/deactivate`, {});
  }

  async updateVisibility(
    id: string,
    data: { visibilityLevel: number; allowCopying: boolean }
  ): Promise<{ message: string }> {
    return this.client.patch<{ message: string }>(`/workout-plans/${id}/visibility`, data);
  }

  async getPublicPlans(params?: {
    page?: number;
    pageSize?: number;
    search?: string;
    goal?: string;
  }): Promise<{
    plans: any[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
  }> {
    return this.client.get<any>('/workout-plans/public', { params });
  }

  async getPublicPlanById(id: string): Promise<WorkoutPlan> {
    return this.client.get<WorkoutPlan>(`/workout-plans/public/${id}`);
  }

  async renew(id: string, data: { additionalWeeks: number }): Promise<{ message: string }> {
    return this.client.post<{ message: string }>(`/workout-plans/${id}/renew`, data);
  }

  async duplicate(id: string, data: { duration: number }): Promise<WorkoutPlan> {
    return this.client.post<WorkoutPlan>(`/workout-plans/${id}/duplicate`, data);
  }

  async clone(id: string): Promise<WorkoutPlan> {
    return this.client.post<WorkoutPlan>(`/workout-plans/${id}/clone`, {});
  }
}

export class WorkoutApi {
  constructor(private client: ApiClient) {}

  async getById(id: string): Promise<Workout> {
    return this.client.get<Workout>(`/workouts/${id}`);
  }

  async create(data: Partial<Workout>): Promise<Workout> {
    return this.client.post<Workout>('/workouts', data);
  }

  async update(id: string, data: Partial<Workout>): Promise<Workout> {
    return this.client.put<Workout>(`/workouts/${id}`, data);
  }

  async delete(id: string): Promise<void> {
    return this.client.delete<void>(`/workouts/${id}`);
  }

  async addExercise(data: Partial<WorkoutExercise>): Promise<WorkoutExercise> {
    return this.client.post<WorkoutExercise>('/workout-exercises', data);
  }

  async removeExercise(id: string): Promise<void> {
    return this.client.delete<void>(`/workout-exercises/${id}`);
  }
}

export class SessionApi {
  constructor(private client: ApiClient) {}

  async start(workoutPlanId?: string, workoutId?: string): Promise<WorkoutSession> {
    return this.client.post<WorkoutSession>('/sessions/start', { workoutPlanId, workoutId });
  }

  async complete(sessionId: string, notes?: string): Promise<WorkoutSession> {
    return this.client.patch<WorkoutSession>(`/sessions/${sessionId}/complete`, { notes });
  }

  async cancel(sessionId: string): Promise<void> {
    return this.client.patch<void>(`/sessions/${sessionId}/cancel`, {});
  }

  async getCurrent(): Promise<WorkoutSession | null> {
    return this.client.get<WorkoutSession | null>('/sessions/current');
  }

  async getById(id: string): Promise<WorkoutSession> {
    return this.client.get<WorkoutSession>(`/sessions/${id}`);
  }

  async getHistory(params?: {
    page?: number;
    pageSize?: number;
    startDate?: string;
    endDate?: string;
  }): Promise<PaginatedResponse<WorkoutSession>> {
    return this.client.get<PaginatedResponse<WorkoutSession>>('/sessions', { params });
  }
}

export class SetApi {
  constructor(private client: ApiClient) {}

  async create(data: CreateSetRequest): Promise<WorkoutSet> {
    // Transform frontend field names to match backend DTO
    const backendPayload: any = {
      exerciseId: data.exerciseId,
      setNumber: data.setNumber,
      reps: data.reps ?? 0,
      load: data.weight ?? 0,
    };
    // Only include rpe if it has a value
    if (data.rpe !== undefined && data.rpe !== null) {
      backendPayload.rpe = data.rpe;
    }
    const url = `/sessions/${data.sessionId}/sets`;
    return this.client.post<WorkoutSet>(url, backendPayload);
  }

  async update(id: string, data: Partial<WorkoutSet>): Promise<WorkoutSet> {
    return this.client.patch<WorkoutSet>(`/sets/${id}`, data);
  }

  async delete(id: string): Promise<void> {
    return this.client.delete<void>(`/sets/${id}`);
  }
}

export class ProgressApi {
  constructor(private client: ApiClient) {}

  async getDashboard(): Promise<ProgressDashboard> {
    return this.client.get<ProgressDashboard>('/progress/dashboard');
  }

  async getVolumeByPeriod(params: {
    startDate: string;
    endDate: string;
    groupBy?: 'day' | 'week' | 'month';
  }): Promise<any> {
    return this.client.get<any>('/progress/volume', { params });
  }
}

export class ChallengeApi {
  constructor(private client: ApiClient) {}

  async getAll(params?: { status?: 'active' | 'completed' | 'failed' }): Promise<Challenge[]> {
    return this.client.get<Challenge[]>('/challenges', { params });
  }

  async getById(id: string): Promise<Challenge> {
    return this.client.get<Challenge>(`/challenges/${id}`);
  }

  async create(data: CreateChallengeRequest): Promise<Challenge> {
    return this.client.post<Challenge>('/challenges', data);
  }

  async update(id: string, data: Partial<Challenge>): Promise<Challenge> {
    return this.client.put<Challenge>(`/challenges/${id}`, data);
  }

  async delete(id: string): Promise<void> {
    return this.client.delete<void>(`/challenges/${id}`);
  }
}

export class BadgeApi {
  constructor(private client: ApiClient) {}

  async getAll(): Promise<Badge[]> {
    return this.client.get<Badge[]>('/badges');
  }

  async getEarned(): Promise<Badge[]> {
    return this.client.get<Badge[]>('/badges/earned');
  }
}

export const createApiEndpoints = (client: ApiClient) => ({
  auth: new AuthApi(client),
  exercises: new ExerciseApi(client),
  workoutPlans: new WorkoutPlanApi(client),
  workouts: new WorkoutApi(client),
  sessions: new SessionApi(client),
  sets: new SetApi(client),
  progress: new ProgressApi(client),
  challenges: new ChallengeApi(client),
  badges: new BadgeApi(client),
});
