// User & Auth types
export interface User {
  id: string;
  email: string;
  name: string;
  role: string;
  avatar?: string;
  profilePictureUrl?: string;
  phoneNumber?: string;
  bio?: string;
  location?: string;
  specialization?: string;
  gymName?: string;
  createdAt: string;
  preferences?: UserPreferences;
  preferredWorkoutLocation?: number;
  personalTrainerId?: string;
}

export interface UserPreferences {
  theme: 'light' | 'dark';
  language: 'pt-BR' | 'en-US';
  notifications: boolean;
  units: 'metric' | 'imperial';
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface SignupRequest {
  email: string;
  password: string;
  name: string;
  preferredWorkoutLocation?: number;
  isPersonalTrainer?: boolean;
}

// Exercise types
export interface Exercise {
  id: string;
  name: string;
  description?: string;
  category: ExerciseCategory;
  muscleGroup: MuscleGroup;
  equipment?: string;
  videoUrl?: string;
  imageUrl?: string;
  createdBy?: string;
  isPublic: boolean;
  workoutLocation?: number;
}

export enum ExerciseCategory {
  Strength = 'strength',
  Cardio = 'cardio',
  Flexibility = 'flexibility',
  Plyometrics = 'plyometrics',
}

export enum MuscleGroup {
  Chest = 'chest',
  Back = 'back',
  Legs = 'legs',
  Shoulders = 'shoulders',
  Arms = 'arms',
  Core = 'core',
  FullBody = 'full_body',
}

// Workout Plan types
export interface WorkoutPlan {
  id: string;
  name: string;
  description?: string;
  goal?: string;
  duration?: number;
  isActive: boolean;
  workouts: Workout[];
  exercises: WorkoutPlanExercise[]; // Kept for backward compatibility
  createdAt?: string;
  updatedAt?: string;
  // Public plan properties
  allowCopying?: boolean;
  creatorName?: string;
  viewCount?: number;
  workoutCount?: number;
}

export interface WorkoutPlanExercise {
  id: string;
  exerciseId: string;
  exerciseName: string;
  exercise?: Exercise;
  order: number;
  targetSets: number;
  targetReps: number;
  targetLoad: number;
}

// Workout (day) types
export interface Workout {
  id: string;
  planId?: string;
  name: string;
  dayOfWeek?: number;
  order: number;
  exercises: WorkoutExercise[];
}

export interface WorkoutExercise {
  id: string;
  workoutId?: string;
  exerciseId: string;
  exercise?: Exercise;
  exerciseName?: string;
  order: number;
  targetSets: number;
  targetReps: number;
  targetLoad: number;
  targetRpe?: number;
  restSeconds?: number;
  notes?: string;
}

// Session types
export interface WorkoutSession {
  id: string;
  workoutPlanId?: string;
  workoutId?: string;
  workoutPlan?: WorkoutPlan;
  startedAt: string;
  completedAt?: string;
  notes?: string;
  sets: WorkoutSet[];
}

export enum SessionStatus {
  InProgress = 'in_progress',
  Completed = 'completed',
  Cancelled = 'cancelled',
}

export interface WorkoutSet {
  id: string;
  sessionId: string;
  exerciseId: string;
  exercise?: Exercise;
  setNumber: number;
  reps: number;
  weight?: number;
  rpe?: number;
  duration?: number; // seconds for cardio
  distance?: number; // meters for cardio
  notes?: string;
  completedAt: string;
  isPr?: boolean;
  isAddedDuringSession?: boolean;
}

export interface CreateSetRequest {
  sessionId: string;
  exerciseId: string;
  setNumber: number;
  reps?: number;
  weight?: number;
  rpe?: number;
  duration?: number;
  distance?: number;
  notes?: string;
}

// Progress & Stats types
export interface ProgressDashboard {
  totalWorkouts: number;
  totalSets: number;
  totalVolume: number; // kg or lbs
  currentStreak: number;
  longestStreak?: number;
  weeklyVolume?: VolumeByWeek[];
  volumeByMuscle?: VolumeByMuscle[];
  weeklyWorkouts?: WeeklyWorkout[];
  recentPRs: PersonalRecord[];
  upcomingBadges?: Badge[];
  accountCreatedAt: string;
}

export interface WeeklyWorkout {
  date: string;
  dayOfWeek: string;
  completed: boolean;
  setsCompleted: number;
}

export interface VolumeByWeek {
  week: string;
  volume: number;
  sets: number;
}

export interface VolumeByMuscle {
  muscleGroup: MuscleGroup;
  volume: number;
  sets: number;
}

export interface PersonalRecord {
  exerciseId: string;
  exerciseName: string;
  weight: number;
  reps: number;
  achievedAt: string;
  dateAchieved?: string; // Alternative field name from backend
  maxLoad?: number; // Alternative field name from backend
}

// Challenge types
export interface Challenge {
  id: string;
  userId: string;
  name: string;
  description?: string;
  type: ChallengeType;
  target: number;
  current: number;
  unit: string;
  startDate: string;
  endDate: string;
  status: ChallengeStatus;
  createdAt: string;
}

export enum ChallengeType {
  Volume = 'volume',
  Sets = 'sets',
  Workouts = 'workouts',
  Streak = 'streak',
  Exercise = 'exercise',
}

export enum ChallengeStatus {
  Active = 'active',
  Completed = 'completed',
  Failed = 'failed',
}

export interface CreateChallengeRequest {
  name: string;
  description?: string;
  type: ChallengeType;
  target: number;
  unit: string;
  startDate: string;
  endDate: string;
}

// Badge types
export interface Badge {
  id: string;
  name: string;
  description: string;
  icon: string;
  category: BadgeCategory;
  requirement: string;
  rarity: BadgeRarity;
  earnedAt?: string;
}

export enum BadgeCategory {
  Volume = 'volume',
  Consistency = 'consistency',
  PRs = 'prs',
  Challenges = 'challenges',
}

export enum BadgeRarity {
  Common = 'common',
  Rare = 'rare',
  Epic = 'epic',
  Legendary = 'legendary',
}

// API Response types
export interface ApiResponse<T> {
  data: T;
  message?: string;
}

export interface ApiError {
  message: string;
  errors?: Record<string, string[]>;
  statusCode: number;
}

export interface PaginatedResponse<T> {
  data: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
