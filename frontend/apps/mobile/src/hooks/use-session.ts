import { createUseSession } from '@gymhero/shared';
import { api } from '@/lib/api';

export const useSession = createUseSession(api);
