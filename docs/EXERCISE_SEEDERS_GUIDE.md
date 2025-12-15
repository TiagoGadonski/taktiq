# Guia de Seeders de Exercícios 🏋️

Este documento explica como popular o banco de dados com exercícios usando diferentes métodos.

## Visão Geral

O GymHero oferece **3 métodos** para popular o banco de dados com exercícios:

1. **Seeder API Wger** (Português + Inglês) - Importa exercícios da API pública Wger
2. **Seeder Abrangente C#** - 150+ exercícios curados manualmente (casa, academia, equipamentos)
3. **Script Node.js** - 150+ exercícios via API REST

---

## 1️⃣ Seeder API Wger (Recomendado para Começar)

Importa exercícios diretamente da API Wger em **português e inglês**.

### Características:
- ✅ 400-600 exercícios
- ✅ Exercícios em português e inglês
- ✅ Imagens incluídas (quando disponíveis)
- ✅ Categorizado por grupo muscular
- ✅ Detecta automaticamente se é para casa/academia

### Como Executar:

**Opção A: Via Interface Admin (Frontend)**
1. Faça login como Admin
2. Acesse `/admin`
3. Clique em **"Seed Exercises from API"**

**Opção B: Via API (Postman/cURL)**
```bash
# Certifique-se que você tem um token de admin
POST https://localhost:7219/api/admin/seed-exercises
Authorization: Bearer {seu_token_admin}
```

**Opção C: Via Swagger**
1. Acesse `https://localhost:7219/swagger`
2. Autorize-se com token de admin
3. Execute o endpoint `POST /api/admin/seed-exercises`

### Funcionamento:
```
API Wger → Português (limit=200) → Imagens → Banco de Dados
        → Inglês (limit=200)   → WorkoutLocation automático
```

---

## 2️⃣ Seeder Abrangente C# (Recomendado para Qualidade)

Importa **150+ exercícios curados manualmente** em português, organizados por categoria.

### Características:
- ✅ 150+ exercícios de alta qualidade
- ✅ 100% em português brasileiro
- ✅ Descrições detalhadas
- ✅ Categorização precisa (Casa, Academia, Ambos)
- ✅ Inclui:
  - Calistenia (flexões, barras, dips)
  - Halteres (roscas, desenvolvimentos)
  - Faixas elásticas
  - Core/Abdômen (12 variações)
  - Cardio e HIIT
  - Mobilidade
  - Exercícios avançados (muscle-up, front lever)

### Como Executar:

**Opção A: Via Interface Admin (Frontend)**
1. Faça login como Admin
2. Acesse `/admin`
3. Clique em **"Seed Comprehensive Exercises"**

**Opção B: Via API**
```bash
POST https://localhost:7219/api/admin/seed-comprehensive-exercises
Authorization: Bearer {seu_token_admin}
```

**Resposta esperada:**
```json
{
  "message": "Exercícios abrangentes importados com sucesso",
  "exercisesAdded": 150,
  "exercisesSkipped": 0,
  "total": 150,
  "timestamp": "2025-01-15T10:30:00Z"
}
```

### Categorias Incluídas:

| Categoria | Quantidade | Equipamento |
|-----------|-----------|-------------|
| Peito | 7 | Peso Corporal |
| Ombros | 6 | Halteres, Peso Corporal |
| Tríceps | 4 | Halteres, Peso Corporal |
| Costas | 8 | Barra, Halteres, Peso Corporal |
| Bíceps | 5 | Halteres, Faixas |
| Quadríceps | 9 | Peso Corporal, Halteres |
| Glúteos/Isquios | 6 | Peso Corporal, Halteres |
| Panturrilhas | 3 | Peso Corporal, Halteres |
| Core/Abdômen | 12 | Peso Corporal |
| Cardio | 9 | Peso Corporal |
| Faixas Elásticas | 6 | Faixas |
| Mobilidade | 5 | Peso Corporal |
| Calistenia Avançada | 7 | Peso Corporal, Barra |

---

## 3️⃣ Script Node.js (Alternativa)

Script JavaScript para importar exercícios via API REST.

### Pré-requisitos:
```bash
npm install axios
# ou
yarn add axios
```

### Como Executar:

1. **Certifique-se que a API está rodando**
   ```bash
   cd src/GymHero.Api
   dotnet run
   ```

2. **Execute o script**
   ```bash
   node seed-comprehensive-exercises.js
   ```

### Personalizar URL:
Edite o arquivo `seed-comprehensive-exercises.js`:
```javascript
const API_BASE_URL = 'https://localhost:7219/api';  // Altere se necessário
```

---

## 📊 Comparação dos Métodos

| Método | Exercícios | Idioma | Imagens | Qualidade | Uso Recomendado |
|--------|-----------|--------|---------|-----------|----------------|
| **API Wger** | 400-600 | PT + EN | Sim | Boa | Quantidade |
| **Seeder C#** | 150+ | PT-BR | Não* | Excelente | Qualidade |
| **Script Node.js** | 150+ | PT-BR | Não* | Excelente | Desenvolvimento |

\* Pode adicionar URLs de imagens manualmente depois

---

## 🎯 Estratégia Recomendada

### Para Produção:
```
1. Execute o Seeder Abrangente C# (150 exercícios de qualidade)
2. Execute o Seeder API Wger (adicionar mais variedade)
3. Total: ~700 exercícios
```

### Para Desenvolvimento:
```
1. Execute o Seeder Abrangente C# (rápido e offline)
2. Total: 150 exercícios essenciais
```

---

## 🔧 Solução de Problemas

### Erro: "Unauthorized"
- Certifique-se de estar autenticado como **Admin**
- Verifique se o token JWT está válido

### Erro: "Database connection failed"
- Verifique se o PostgreSQL está rodando
- Confirme a connection string em `appsettings.Development.json`

### Exercícios duplicados
- Os seeders verificam nomes existentes e pulam duplicatas
- Mensagem: `exercisesSkipped: X`

### API Wger não responde
- Verifique sua conexão com a internet
- A API Wger pode ter rate limiting

---

## 📝 Customização

### Adicionar Novos Exercícios ao Seeder C#

Edite: `src/GymHero.Infrastructure/Services/ComprehensiveExerciseSeederService.cs`

```csharp
new Exercise
{
    Name = "Meu Exercício",
    Description = "Descrição detalhada",
    MuscleGroup = "Peito",
    Category = "Força",
    Equipment = "Halteres",
    Notes = "Dicas de execução",
    WorkoutLocation = WorkoutLocation.Both
}
```

### Modificar Detecção de WorkoutLocation

Edite: `src/GymHero.Infrastructure/Services/ExerciseSeederService.cs`

Método: `DetermineWorkoutLocation()`

---

## 🚀 Próximos Passos

Após popular os exercícios:

1. ✅ Verificar exercícios importados: `GET /api/exercises`
2. ✅ Testar filtros por `WorkoutLocation`
3. ✅ Adicionar imagens para exercícios sem imagem
4. ✅ Criar planos de treino usando os exercícios

---

## 📞 Suporte

Para problemas ou dúvidas:
- Verifique os logs da aplicação
- Consulte a documentação da API Wger: https://wger.de/api/v2/
- Abra uma issue no repositório

---

**Última atualização:** 2025-01-15
**Versão:** 1.0
