import {
  Trophy,
  Target,
  Dumbbell,
  TrendingUp,
  Zap,
  Award,
  Users,
  CheckCircle2,
  Star,
  Flame,
  Heart,
  Activity,
  Calendar,
  Clock,
  Flag,
  Gift,
  Mountain,
  Footprints,
  Sun,
  Moon,
  Bike,
  Watch,
  Timer,
  Gauge,
  Medal,
  Crown,
  Sparkles,
  Rocket,
  Shield,
  Swords,
  Weight,
  ClipboardList,
  UserCheck,
  CalendarCheck,
  Share2,
  LucideIcon,
} from 'lucide-react';

export interface ChallengeIconOption {
  name: string;
  icon: LucideIcon;
  label: string;
  category: string;
}

export const challengeIcons: ChallengeIconOption[] = [
  // Achievement & Success
  { name: 'trophy', icon: Trophy, label: 'Troféu', category: 'Conquista' },
  { name: 'medal', icon: Medal, label: 'Medalha', category: 'Conquista' },
  { name: 'award', icon: Award, label: 'Prêmio', category: 'Conquista' },
  { name: 'crown', icon: Crown, label: 'Coroa', category: 'Conquista' },
  { name: 'star', icon: Star, label: 'Estrela', category: 'Conquista' },
  { name: 'sparkles', icon: Sparkles, label: 'Brilho', category: 'Conquista' },

  // Fitness & Exercise
  { name: 'dumbbell', icon: Dumbbell, label: 'Haltere', category: 'Fitness' },
  { name: 'weight', icon: Weight, label: 'Peso', category: 'Fitness' },
  { name: 'activity', icon: Activity, label: 'Atividade', category: 'Fitness' },
  { name: 'trending-up', icon: TrendingUp, label: 'Progresso', category: 'Fitness' },
  { name: 'gauge', icon: Gauge, label: 'Intensidade', category: 'Fitness' },
  { name: 'bike', icon: Bike, label: 'Bicicleta', category: 'Fitness' },
  { name: 'footprints', icon: Footprints, label: 'Passos', category: 'Fitness' },

  // Energy & Power
  { name: 'zap', icon: Zap, label: 'Energia', category: 'Força' },
  { name: 'flame', icon: Flame, label: 'Fogo', category: 'Força' },
  { name: 'rocket', icon: Rocket, label: 'Foguete', category: 'Força' },
  { name: 'mountain', icon: Mountain, label: 'Montanha', category: 'Força' },
  { name: 'shield', icon: Shield, label: 'Escudo', category: 'Força' },
  { name: 'swords', icon: Swords, label: 'Espadas', category: 'Força' },

  // Time & Schedule
  { name: 'calendar', icon: Calendar, label: 'Calendário', category: 'Tempo' },
  { name: 'calendar-check', icon: CalendarCheck, label: 'Agendado', category: 'Tempo' },
  { name: 'clock', icon: Clock, label: 'Relógio', category: 'Tempo' },
  { name: 'timer', icon: Timer, label: 'Cronômetro', category: 'Tempo' },
  { name: 'watch', icon: Watch, label: 'Relógio de Pulso', category: 'Tempo' },
  { name: 'sun', icon: Sun, label: 'Sol', category: 'Tempo' },
  { name: 'moon', icon: Moon, label: 'Lua', category: 'Tempo' },

  // Goals & Progress
  { name: 'target', icon: Target, label: 'Alvo', category: 'Meta' },
  { name: 'flag', icon: Flag, label: 'Bandeira', category: 'Meta' },
  { name: 'clipboard-list', icon: ClipboardList, label: 'Lista', category: 'Meta' },

  // Social & Community
  { name: 'users', icon: Users, label: 'Usuários', category: 'Social' },
  { name: 'share-2', icon: Share2, label: 'Compartilhar', category: 'Social' },
  { name: 'heart', icon: Heart, label: 'Coração', category: 'Social' },
  { name: 'gift', icon: Gift, label: 'Presente', category: 'Social' },

  // Completion & Check
  { name: 'user-check', icon: UserCheck, label: 'Verificado', category: 'Status' },
  { name: 'check-circle', icon: CheckCircle2, label: 'Completo', category: 'Status' },
];

export const getChallengeIcon = (iconName?: string | null): LucideIcon => {
  if (!iconName) return Trophy;

  const icon = challengeIcons.find(i => i.name === iconName);
  return icon?.icon || Trophy;
};

export const getChallengeIconCategories = (): string[] => {
  const categories = new Set(challengeIcons.map(i => i.category));
  return Array.from(categories);
};
