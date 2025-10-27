export interface Tokens {
  accessToken: string;
  refreshToken: string;
}

export interface User {
  id: string;
  name: string;
  email: string;
  role?: 'user' | 'admin';
  createdAt?: string;
  updatedAt?: string;
}

export interface Exercise {
  id: string;
  name: string;
  description?: string;
  muscleGroup?: string;
  imageUrl?: string;
  equipment?: string;
}

export interface WorkoutExercise {
  id?: string;
  exerciseId: string;
  exerciseName?: string;
  targetSets: number;
  targetReps: number;
  targetLoad: number;
  order?: number;
  notes?: string;
  exercise?: Exercise;
}

export interface WorkoutPlanExercise extends WorkoutExercise {
  restTimeSeconds?: number;
}

export interface Workout {
  id: string;
  name: string;
  description?: string;
  exercises?: WorkoutExercise[];
  createdAt?: string;
  updatedAt?: string;
}

export interface WorkoutPlan {
  id: string;
  name: string;
  description?: string;
  goal?: string;
  isActive?: boolean;
  workouts: Workout[];
  createdAt?: string;
  updatedAt?: string;
}

export interface WorkoutSet {
  id: string;
  sessionId: string;
  exerciseId: string;
  setNumber: number;
  reps: number;
  weight?: number;
  rpe?: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface WorkoutSession {
  id: string;
  workoutPlanId?: string;
  workoutId?: string;
  workout?: Workout;
  workoutPlan?: WorkoutPlan;
  sets?: WorkoutSet[];
  startedAt: string;
  completedAt?: string;
  duration?: number;
  notes?: string;
}

export interface PaginatedResponse<T> {
  data: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface SessionHistoryFilters {
  page: number;
  pageSize: number;
  startDate?: string;
  endDate?: string;
}

export interface CreateSessionInput {
  workoutPlanId?: string | null;
  workoutId?: string | null;
}

export interface CompleteSessionInput {
  sessionId: string;
  notes?: string;
}

export interface CreateSetInput {
  sessionId: string;
  exerciseId: string;
  setNumber: number;
  reps: number;
  weight?: number;
  rpe?: number;
}

export interface VolumeByWeek {
  week: string;
  volume: number;
  sets: number;
}

export interface VolumeByMuscle {
  muscleGroup: string;
  volume: number;
}

export interface PersonalRecord {
  exerciseId: string;
  exerciseName: string;
  weight?: number;
  maxLoad?: number;
  reps: number;
  achievedAt?: string;
  dateAchieved?: string;
}

export interface WeeklyWorkoutSummary {
  date: string;
  completed: boolean;
  setsCompleted: number;
  dayOfWeek: string;
}

export interface ProgressDashboard {
  totalWorkouts: number;
  totalSets: number;
  totalVolume: number;
  currentStreak: number;
  longestStreak: number;
  weeklyVolume: VolumeByWeek[];
  volumeByMuscle: VolumeByMuscle[];
  weeklyWorkouts: WeeklyWorkoutSummary[];
  recentPRs: PersonalRecord[];
  accountCreatedAt?: string;
}

export type ChallengeStatus = 'active' | 'completed' | 'upcoming';

export interface Challenge {
  id: string;
  name: string;
  description?: string;
  status: ChallengeStatus;
  progress?: number;
  goal?: number;
  reward?: string;
  startsAt?: string;
  endsAt?: string;
}

export interface ChallengeFilters {
  status?: ChallengeStatus;
}

export interface ApiClientRequestConfig {
  headers?: Record<string, string>;
  query?: Record<string, unknown>;
}
