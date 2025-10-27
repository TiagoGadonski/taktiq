import type { ApiClient } from './client';
import type {
  Challenge,
  ChallengeFilters,
  ChallengeStatus,
  CompleteSessionInput,
  CreateSessionInput,
  CreateSetInput,
  PaginatedResponse,
  ProgressDashboard,
  SessionHistoryFilters,
  Tokens,
  User,
  VolumeByMuscle,
  VolumeByWeek,
  WorkoutPlan,
  WorkoutSession,
} from '../types';

const buildQueryString = (params?: Record<string, unknown>): string => {
  if (!params) {
    return '';
  }

  const query = new URLSearchParams();

  Object.entries(params).forEach(([key, value]) => {
    if (value === undefined || value === null || value === '') {
      return;
    }

    if (Array.isArray(value)) {
      value.forEach((item) => query.append(key, String(item)));
    } else {
      query.append(key, String(value));
    }
  });

  const queryString = query.toString();
  return queryString ? `?${queryString}` : '';
};

export interface ApiEndpoints {
  auth: {
    login: (data: { email: string; password: string }) => Promise<Tokens>;
    signup: (data: { name: string; email: string; password: string }) => Promise<Tokens>;
    logout: () => Promise<void>;
    getMe: () => Promise<User | null>;
  };
  workoutPlans: {
    getAll: () => Promise<WorkoutPlan[]>;
    setActive: (planId: string) => Promise<WorkoutPlan>;
    setInactive: (planId: string) => Promise<WorkoutPlan>;
    delete: (planId: string) => Promise<void>;
  };
  sessions: {
    getCurrent: () => Promise<WorkoutSession | null>;
    getHistory: (filters: SessionHistoryFilters) => Promise<PaginatedResponse<WorkoutSession>>;
    start: (input?: string | CreateSessionInput | null) => Promise<WorkoutSession>;
    complete: (input: CompleteSessionInput) => Promise<WorkoutSession>;
  };
  sets: {
    create: (input: CreateSetInput) => Promise<WorkoutSession>;
    delete: (setId: string) => Promise<void>;
  };
  progress: {
    getDashboard: () => Promise<ProgressDashboard>;
    getWeeklyVolume: () => Promise<VolumeByWeek[]>;
    getVolumeByMuscle: () => Promise<VolumeByMuscle[]>;
  };
  challenges: {
    getAll: (filters?: ChallengeFilters) => Promise<Challenge[]>;
    getByStatus: (status: ChallengeStatus) => Promise<Challenge[]>;
  };
}

const withQuery = (path: string, query?: Record<string, unknown>) => `${path}${buildQueryString(query)}`;

export const createApiEndpoints = (client: ApiClient): ApiEndpoints => ({
  auth: {
    login: (data) => client.post('/auth/login', data),
    signup: (data) => client.post('/auth/signup', data),
    logout: () => client.post('/auth/logout'),
    getMe: () => client.get('/auth/me'),
  },
  workoutPlans: {
    getAll: () => client.get('/workout-plans'),
    setActive: (planId) => client.post(`/workout-plans/${planId}/activate`),
    setInactive: (planId) => client.post(`/workout-plans/${planId}/deactivate`),
    delete: (planId) => client.delete(`/workout-plans/${planId}`),
  },
  sessions: {
    getCurrent: () => client.get('/sessions/current'),
    getHistory: (filters) => client.get(withQuery('/sessions/history', filters)),
    start: (input) => {
      if (!input) {
        return client.post('/sessions/start');
      }

      if (typeof input === 'string') {
        return client.post('/sessions/start', { workoutPlanId: input });
      }

      return client.post('/sessions/start', input);
    },
    complete: (input) => client.post('/sessions/complete', input),
  },
  sets: {
    create: (input) => client.post('/sets', input),
    delete: (setId) => client.delete(`/sets/${setId}`),
  },
  progress: {
    getDashboard: () => client.get('/progress/dashboard'),
    getWeeklyVolume: () => client.get('/progress/weekly-volume'),
    getVolumeByMuscle: () => client.get('/progress/volume-by-muscle'),
  },
  challenges: {
    getAll: (filters) => client.get(withQuery('/challenges', filters)),
    getByStatus: (status) => client.get(withQuery('/challenges', { status })),
  },
});
