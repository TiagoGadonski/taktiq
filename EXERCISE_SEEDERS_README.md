# Guia Rápido: Populando Exercícios 🏋️

## 🚀 Início Rápido (2 minutos)

### Método 1: Seeder Abrangente (Recomendado)
```bash
# Via Swagger: https://localhost:7219/swagger
POST /api/admin/seed-comprehensive-exercises
Authorization: Bearer {seu_token_admin}

# Resultado: 150+ exercícios em português
```

### Método 2: API Wger (Mais exercícios)
```bash
# Via Swagger: https://localhost:7219/swagger
POST /api/admin/seed-exercises
Authorization: Bearer {seu_token_admin}

# Resultado: 400-600 exercícios (PT + EN)
```

### Método 3: Script Node.js
```bash
node seed-comprehensive-exercises.js
```

---

## 📦 O Que Você Ganha

### Seeder Abrangente C# (150+ exercícios)
- ✅ **Casa**: Flexões, Barras, Calistenia
- ✅ **Academia**: Halteres, Equipamentos
- ✅ **Ambos**: Faixas elásticas, Jump rope
- ✅ **Core**: 12 variações de abdômen
- ✅ **Cardio**: Burpees, HIIT, Polichinelos
- ✅ **Mobilidade**: Yoga, Alongamentos
- ✅ **Avançado**: Muscle-up, Front Lever

### API Wger (400-600 exercícios)
- ✅ Exercícios em português e inglês
- ✅ Imagens incluídas
- ✅ Categorização automática
- ✅ Detecção de Casa/Academia

---

## 🎯 Estratégia Recomendada

### Para Produção:
```
1. Seeder Abrangente C# (base sólida)
2. API Wger (adicionar variedade)
3. Total: ~700 exercícios
```

### Para Desenvolvimento:
```
1. Seeder Abrangente C# (rápido)
2. Total: 150 exercícios
```

---

## 📚 Documentação Completa

Ver: [`docs/EXERCISE_SEEDERS_GUIDE.md`](docs/EXERCISE_SEEDERS_GUIDE.md)

---

## ✅ Verificar Resultados

```bash
GET /api/exercises
```

Filtrar por localização:
```bash
GET /api/exercises?workoutLocation=Home    # Apenas casa
GET /api/exercises?workoutLocation=Gym     # Apenas academia
GET /api/exercises?workoutLocation=Both    # Ambos
```

---

**Última atualização:** 2025-01-15
