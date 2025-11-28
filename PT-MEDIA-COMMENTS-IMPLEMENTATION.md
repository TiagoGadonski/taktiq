# Implementação de Exercícios Customizados e Comentários para Personal Trainers

## Resumo

Este documento descreve as funcionalidades implementadas para Personal Trainers:
1. **Sistema de Upload de Mídia com FFmpeg** - Já existente e funcional
2. **Sistema de Comentários em Planos de Treino** - Implementado no backend
3. **Opções de PT na geração de planos por IA** - Implementado no frontend

---

## 1. Sistema de Mídia (Já Implementado)

### Backend Completo
- ✅ **VideoProcessingService** (`src/GymHero.Infrastructure/Services/VideoProcessingService.cs`)
  - Compressão de vídeos com FFmpeg
  - Geração automática de thumbnails
  - Extração de metadata (duração, dimensões)
  - Suporte a diferentes qualidades (Low, Medium, High)

- ✅ **MediaEndpoints** (`src/GymHero.Api/Endpoints/MediaEndpoints.cs`)
  - `POST /api/media/upload` - Upload de imagens e vídeos
  - `GET /api/media/{mediaId}` - Detalhes da mídia
  - `GET /api/media/my` - Lista de mídias do usuário
  - `DELETE /api/media/{mediaId}` - Deletar mídia

- ✅ **Media Entity** (`src/GymHero.Domain/Entities/Media.cs`)
  - Armazena URL, thumbnail, metadata
  - Suporte a contexto de uso (UsageContext)
  - Referência a entidade relacionada (EntityId)

### Frontend Completo
- ✅ **ImageUpload Component** (`frontend/apps/web/src/components/media/image-upload.tsx`)
  - Preview de imagens
  - Upload com progresso
  - Validação de tipo e tamanho

- ✅ **VideoUpload Component** (`frontend/apps/web/src/components/media/video-upload.tsx`)
  - Preview de vídeos
  - Upload com barra de progresso
  - Validação de tipo e tamanho (até 100MB)

### Como Usar os Componentes de Upload

```typescript
import { ImageUpload } from '@/components/media/image-upload';
import { VideoUpload } from '@/components/media/video-upload';

// Upload de imagem para exercício
<ImageUpload
  onImageUploaded={(url) => setExerciseImageUrl(url)}
  currentImageUrl={exerciseImageUrl}
  usageContext="ExerciseImage"
  entityId={exerciseId}
/>

// Upload de vídeo para exercício
<VideoUpload
  onVideoUploaded={(url) => setExerciseVideoUrl(url)}
  currentVideoUrl={exerciseVideoUrl}
  usageContext="ExerciseVideo"
  entityId={exerciseId}
/>
```

---

## 2. Sistema de Comentários em Planos (Implementado no Backend)

### Backend Implementado

- ✅ **WorkoutPlanComment Entity** (`src/GymHero.Domain/Entities/WorkoutPlanComment.cs`)
  - Comentários em planos
  - Suporte a respostas aninhadas (ParentCommentId)
  - Soft delete

- ✅ **WorkoutPlanCommentEndpoints** (`src/GymHero.Api/Endpoints/WorkoutPlanCommentEndpoints.cs`)
  - `POST /api/workout-plans/{planId}/comments` - Criar comentário
  - `GET /api/workout-plans/{planId}/comments` - Listar comentários
  - `PUT /api/workout-plans/{planId}/comments/{commentId}` - Atualizar comentário
  - `DELETE /api/workout-plans/{planId}/comments/{commentId}` - Deletar comentário

- ✅ **DTOs** (`src/GymHero.Shared/DTOs/WorkoutPlanCommentDtos.cs`)
  - CreateWorkoutPlanCommentRequest
  - WorkoutPlanCommentResponse (com lista de respostas)
  - UpdateWorkoutPlanCommentRequest

- ✅ **Migration Criada** - Tabela WorkoutPlanComments será criada ao rodar a aplicação

### Frontend - A Implementar

Para implementar comentários no frontend, adicione um componente na visualização de planos:

**Exemplo de uso:**

```typescript
// frontend/apps/web/src/components/plans/plan-comments.tsx
'use client';

import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { MessageSquare, Send } from 'lucide-react';

interface Comment {
  id: string;
  userId: string;
  userName: string;
  content: string;
  createdAt: string;
  replies: Comment[];
}

interface PlanCommentsProps {
  planId: string;
}

export function PlanComments({ planId }: PlanCommentsProps) {
  const [newComment, setNewComment] = useState('');
  const queryClient = useQueryClient();

  // Buscar comentários
  const { data: comments = [] } = useQuery<Comment[]>({
    queryKey: ['plan-comments', planId],
    queryFn: async () => {
      return apiClient.get(`/workout-plans/${planId}/comments`);
    },
  });

  // Criar comentário
  const createMutation = useMutation({
    mutationFn: async (content: string) => {
      return apiClient.post(`/workout-plans/${planId}/comments`, {
        workoutPlanId: planId,
        content,
        parentCommentId: null,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries(['plan-comments', planId]);
      setNewComment('');
    },
  });

  const handleSubmit = () => {
    if (newComment.trim()) {
      createMutation.mutate(newComment);
    }
  };

  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold flex items-center gap-2">
        <MessageSquare className="h-5 w-5" />
        Comentários ({comments.length})
      </h3>

      {/* Formulário novo comentário */}
      <div className="space-y-2">
        <Textarea
          value={newComment}
          onChange={(e) => setNewComment(e.target.value)}
          placeholder="Adicione um comentário..."
          rows={3}
        />
        <Button onClick={handleSubmit} disabled={createMutation.isPending}>
          <Send className="h-4 w-4 mr-2" />
          Comentar
        </Button>
      </div>

      {/* Lista de comentários */}
      <div className="space-y-3">
        {comments.map((comment) => (
          <div key={comment.id} className="border rounded-lg p-4">
            <div className="flex justify-between items-start mb-2">
              <span className="font-semibold">{comment.userName}</span>
              <span className="text-sm text-muted-foreground">
                {new Date(comment.createdAt).toLocaleDateString()}
              </span>
            </div>
            <p className="text-sm">{comment.content}</p>

            {/* Respostas aninhadas */}
            {comment.replies.length > 0 && (
              <div className="ml-6 mt-3 space-y-2 border-l-2 pl-3">
                {comment.replies.map((reply) => (
                  <div key={reply.id}>
                    <span className="font-semibold text-sm">{reply.userName}</span>
                    <p className="text-sm">{reply.content}</p>
                  </div>
                ))}
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
```

**Uso no plano:**

```typescript
// Na página de visualização do plano
import { PlanComments } from '@/components/plans/plan-comments';

// Dentro do componente
<PlanComments planId={planId} />
```

---

## 3. Opções de PT na Geração de Planos por IA (Implementado)

### Frontend - AI Workout Page

Modificações em `frontend/apps/web/src/app/(app)/ai-workout/page.tsx`:

✅ **Detecção de Personal Trainer**
```typescript
const isPersonalTrainer = user?.role === 'PersonalTrainer';
```

✅ **3 Tipos de Plano**
1. **Template Pessoal** - Modelo reutilizável
2. **Para Aluno Específico** - Atribuição direta
3. **Para Marketplace** - Venda pública

✅ **Validações Implementadas**
- Seleção obrigatória de aluno (tipo student)
- Preço válido (tipo marketplace)
- Data de expiração (tipo student)

✅ **UI Card de Configurações PT**
- Radio buttons para tipo de plano
- Select para escolher aluno
- Input para preço
- Input para semanas de expiração

---

## 4. Próximos Passos - Exercícios Customizados

### Para permitir que PTs criem exercícios customizados com mídia:

#### Opção 1: Modal de Criação de Exercício

Adicione um botão "Criar Exercício Personalizado" na página de criação de planos:

```typescript
// frontend/apps/web/src/components/exercise/custom-exercise-modal.tsx
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { ImageUpload } from '@/components/media/image-upload';
import { VideoUpload } from '@/components/media/video-upload';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Button } from '@/components/ui/button';

interface CustomExerciseModalProps {
  open: boolean;
  onClose: () => void;
  onExerciseCreated: (exercise: any) => void;
}

export function CustomExerciseModal({ open, onClose, onExerciseCreated }: CustomExerciseModalProps) {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [imageUrl, setImageUrl] = useState('');
  const [videoUrl, setVideoUrl] = useState('');
  const [instructions, setInstructions] = useState('');

  const handleCreate = async () => {
    // Criar exercício via API
    const exercise = await apiClient.post('/exercises', {
      name,
      notes: description,
      imageUrl,
      videoUrl,
      muscleGroup: 'Custom', // ou permitir seleção
      category: 'strength',
      equipment: 'custom',
    });

    onExerciseCreated(exercise);
    onClose();
  };

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Criar Exercício Personalizado</DialogTitle>
        </DialogHeader>

        <div className="space-y-4">
          {/* Nome */}
          <div>
            <Label>Nome do Exercício</Label>
            <Input
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Ex: Agachamento com barra"
            />
          </div>

          {/* Descrição */}
          <div>
            <Label>Descrição</Label>
            <Textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Descreva o exercício..."
              rows={3}
            />
          </div>

          {/* Instruções */}
          <div>
            <Label>Instruções de Execução</Label>
            <Textarea
              value={instructions}
              onChange={(e) => setInstructions(e.target.value)}
              placeholder="1. Posicione-se...\n2. Execute o movimento...\n3. ..."
              rows={5}
            />
          </div>

          {/* Upload de Imagem */}
          <div>
            <Label>Imagem Demonstrativa</Label>
            <ImageUpload
              onImageUploaded={setImageUrl}
              currentImageUrl={imageUrl}
              usageContext="ExerciseImage"
            />
          </div>

          {/* Upload de Vídeo */}
          <div>
            <Label>Vídeo Demonstrativo</Label>
            <VideoUpload
              onVideoUploaded={setVideoUrl}
              currentVideoUrl={videoUrl}
              usageContext="ExerciseVideo"
            />
          </div>

          {/* Botões */}
          <div className="flex gap-2">
            <Button onClick={handleCreate} disabled={!name.trim()}>
              Criar Exercício
            </Button>
            <Button variant="outline" onClick={onClose}>
              Cancelar
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
```

#### Opção 2: Inline na Lista de Exercícios

Adicione um formulário expansível diretamente na lista de resultados de busca:

```typescript
// Na página plans/new/page.tsx, adicione:

<Button
  type="button"
  variant="outline"
  onClick={() => setShowCustomForm(!showCustomForm)}
>
  <Plus className="h-4 w-4 mr-2" />
  Criar Exercício Personalizado
</Button>

{showCustomForm && (
  <Card>
    <CardContent className="pt-6">
      {/* Formulário similar ao modal acima */}
    </CardContent>
  </Card>
)}
```

---

## 5. Testando as Funcionalidades

### Backend

```bash
# 1. Compilar backend
cd src/GymHero.Api
dotnet build

# 2. Rodar aplicação (migration será aplicada automaticamente)
dotnet run

# 3. Testar endpoints de comentários
# POST /api/workout-plans/{planId}/comments
# GET /api/workout-plans/{planId}/comments
```

### Frontend

```bash
# 1. Compilar frontend
cd frontend/apps/web
npm run build

# 2. Rodar em desenvolvimento
npm run dev

# 3. Testar funcionalidades:
# - Login como Personal Trainer
# - Ir para /ai-workout
# - Criar plano semanal
# - Verificar opções de PT (Template, Aluno, Marketplace)
```

---

## 6. Arquivos Modificados/Criados

### Backend
- ✅ `src/GymHero.Domain/Entities/WorkoutPlanComment.cs` - NOVO
- ✅ `src/GymHero.Shared/DTOs/WorkoutPlanCommentDtos.cs` - NOVO
- ✅ `src/GymHero.Api/Endpoints/WorkoutPlanCommentEndpoints.cs` - NOVO
- ✅ `src/GymHero.Infrastructure/Data/ApplicationDbContext.cs` - MODIFICADO
- ✅ `src/GymHero.Application/Common/Interfaces/IApplicationDbContext.cs` - MODIFICADO
- ✅ `src/GymHero.Api/Program.cs` - MODIFICADO
- ✅ `src/GymHero.Infrastructure/Migrations/xxxxx_AddWorkoutPlanComments.cs` - NOVO

### Frontend
- ✅ `frontend/apps/web/src/app/(app)/ai-workout/page.tsx` - MODIFICADO
- ⏳ `frontend/apps/web/src/components/plans/plan-comments.tsx` - A CRIAR
- ⏳ `frontend/apps/web/src/components/exercise/custom-exercise-modal.tsx` - A CRIAR

---

## 7. Checklist de Implementação

### Backend ✅
- [x] Entidade WorkoutPlanComment
- [x] DTOs para comentários
- [x] Endpoints de comentários
- [x] Migration criada
- [x] Endpoints registrados no Program.cs
- [x] Build sem erros

### Frontend - AI Workout ✅
- [x] Detecção de Personal Trainer
- [x] Estados para PT (planType, selectedStudentId, etc)
- [x] UI para seleção de tipo de plano
- [x] UI para seleção de aluno
- [x] UI para configuração de marketplace
- [x] Validações no handleSavePlan
- [x] Validações no handleStartPlan
- [x] Build sem erros

### Frontend - Comentários ⏳
- [ ] Componente PlanComments
- [ ] Integração na visualização de planos
- [ ] Testes de criação de comentários
- [ ] Testes de respostas aninhadas

### Frontend - Exercícios Customizados ⏳
- [ ] Modal/Form de criação de exercício
- [ ] Integração com ImageUpload
- [ ] Integração com VideoUpload
- [ ] Endpoint POST /exercises no backend (verificar se existe)
- [ ] Adicionar exercício criado à lista

---

## 8. Configuração do FFmpeg (Docker)

O FFmpeg já está configurado no Dockerfile:

```dockerfile
# Instalar FFmpeg para processamento de vídeo
RUN apt-get update && apt-get install -y ffmpeg && rm -rf /var/lib/apt/lists/*
```

Se precisar rodar localmente sem Docker:

**Windows:**
```bash
# Instalar via Chocolatey
choco install ffmpeg

# Ou baixar de: https://ffmpeg.org/download.html
```

**Linux:**
```bash
sudo apt-get update
sudo apt-get install ffmpeg
```

**macOS:**
```bash
brew install ffmpeg
```

---

## 9. Sugestões de Melhorias Futuras

1. **Biblioteca de Exercícios do PT**
   - Cada PT ter sua biblioteca privada de exercícios
   - Reutilizar exercícios entre planos
   - Categorização personalizada

2. **Templates de Planos**
   - Salvar planos como templates
   - Aplicar templates a múltiplos alunos
   - Marketplace de templates

3. **Comentários Avançados**
   - Menções (@usuario)
   - Reações (curtir, etc)
   - Notificações de novos comentários

4. **Análise de Vídeos**
   - IA para análise de forma
   - Feedback automático
   - Comparação com vídeo ideal

5. **Galeria de Mídia do PT**
   - Organização por pastas
   - Tags e busca
   - Compartilhamento entre exercícios

---

## Conclusão

✅ **Sistema de Mídia**: Completo e funcional
✅ **Comentários**: Backend completo, frontend pronto para implementar
✅ **Opções de PT em AI Workout**: Implementado e testado

O sistema está pronto para uso! PTs podem:
- Gerar planos por IA para alunos específicos ou marketplace
- Fazer upload de imagens e vídeos com compressão automática
- (Após implementar frontend) Comentar em planos
- (Após implementar modal) Criar exercícios customizados com mídia
