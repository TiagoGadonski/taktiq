const https = require('https');

// Configuração
const API_BASE = 'https://api.taktiq.app/api';
const GEMINI_API_KEY = 'AIzaSyDCIiSPd5IiaNLO_0OZ-LVcDGlN1OWqoNk';
const ADMIN_EMAIL = 'admin@taktiq.app';
const ADMIN_PASSWORD = 'W3rt4juk@';

// Mapeamento de grupos musculares para português
const MUSCLE_GROUP_PT = {
  'Pectoralis major': 'Peitoral Maior',
  'Anterior deltoid': 'Deltoide Anterior',
  'Latissimus dorsi': 'Grande Dorsal',
  'Biceps brachii': 'Bíceps',
  'Triceps brachii': 'Tríceps',
  'Rectus abdominis': 'Abdômen Reto',
  'Obliquus externus abdominis': 'Oblíquo Externo',
  'Quadriceps femoris': 'Quadríceps',
  'Biceps femoris': 'Bíceps Femoral',
  'Gluteus maximus': 'Glúteo Máximo',
  'Gastrocnemius': 'Panturrilha',
  'Trapezius': 'Trapézio',
  'Geral': 'Geral'
};

// Mapeamento de equipamentos para português
const EQUIPMENT_PT = {
  'Barbell': 'Barra',
  'Dumbbell': 'Halteres',
  'Kettlebell': 'Kettlebell',
  'Bench': 'Banco',
  'Pull-up bar': 'Barra Fixa',
  'Gym mat': 'Tapete',
  'SZ-Bar': 'Barra W',
  'none (bodyweight exercise)': 'Peso Corporal',
  'N/A': 'Sem Equipamento'
};

// Helper para fazer requisições HTTPS
function makeRequest(url, method = 'GET', data = null, headers = {}) {
  return new Promise((resolve, reject) => {
    const urlObj = new URL(url);
    const options = {
      hostname: urlObj.hostname,
      port: urlObj.port || 443,
      path: urlObj.pathname + urlObj.search,
      method: method,
      headers: {
        'Content-Type': 'application/json',
        ...headers
      }
    };

    const req = https.request(options, (res) => {
      let body = '';
      res.on('data', (chunk) => body += chunk);
      res.on('end', () => {
        try {
          const json = JSON.parse(body);
          resolve(json);
        } catch (e) {
          resolve(body);
        }
      });
    });

    req.on('error', reject);
    if (data) req.write(JSON.stringify(data));
    req.end();
  });
}

// Login
async function login() {
  console.log('🔐 Fazendo login...');
  const response = await makeRequest(`${API_BASE}/auth/login`, 'POST', {
    email: ADMIN_EMAIL,
    password: ADMIN_PASSWORD
  });
  return response.token;
}

// Buscar todos os exercícios
async function getExercises(token) {
  console.log('📥 Buscando exercícios...');
  const response = await makeRequest(`${API_BASE}/exercises`, 'GET', null, {
    'Authorization': `Bearer ${token}`
  });
  return response;
}

// Gerar nome e descrição com IA
async function generateExerciseData(muscleGroup, equipment) {
  const muscleGroupPT = MUSCLE_GROUP_PT[muscleGroup] || muscleGroup;
  const equipmentPT = EQUIPMENT_PT[equipment] || equipment;

  const prompt = `Você é um especialista em educação física. Com base nas informações abaixo, sugira um nome de exercício apropriado em português brasileiro.

Grupo Muscular: ${muscleGroupPT} (original: ${muscleGroup})
Equipamento: ${equipmentPT} (original: ${equipment})

Responda APENAS com um JSON no seguinte formato (sem markdown, sem explicações adicionais):
{
  "name": "Nome do Exercício em Português",
  "description": "Descrição detalhada de como executar o exercício, músculos trabalhados, dicas de técnica e benefícios (máximo 300 palavras)"
}

IMPORTANTE:
- O nome deve ser claro e específico
- A descrição deve ser prática e educativa
- Use apenas português brasileiro
- Retorne APENAS o JSON, nada mais`;

  try {
    const response = await makeRequest(
      `https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key=${GEMINI_API_KEY}`,
      'POST',
      {
        contents: [
          {
            parts: [
              { text: prompt }
            ]
          }
        ]
      }
    );

    const text = response?.candidates?.[0]?.content?.parts?.[0]?.text?.trim();

    if (!text) {
      return null;
    }

    // Tentar extrair JSON da resposta
    let jsonText = text;

    // Remover markdown se existir
    jsonText = jsonText.replace(/```json\n?/g, '').replace(/```\n?/g, '');

    try {
      const parsed = JSON.parse(jsonText);
      return parsed;
    } catch (e) {
      console.error('   ⚠️  Erro ao parsear JSON da IA:', e.message);
      return null;
    }
  } catch (error) {
    console.error(`   ❌ Erro ao chamar IA: ${error.message}`);
    return null;
  }
}

// Atualizar exercício
async function updateExercise(token, exerciseId, updates) {
  await makeRequest(
    `${API_BASE}/exercises/${exerciseId}`,
    'PUT',
    updates,
    { 'Authorization': `Bearer ${token}` }
  );
}

// Processar um exercício vazio
async function processEmptyExercise(token, exercise, index, total) {
  console.log(`\n[${index + 1}/${total}] Processando exercício vazio`);
  console.log(`   Grupo Muscular: ${exercise.muscleGroup}`);
  console.log(`   Equipamento: ${exercise.equipment || 'N/A'}`);

  try {
    // Gerar nome e descrição com IA
    console.log('   🤖 Gerando nome e descrição com IA...');
    const generated = await generateExerciseData(exercise.muscleGroup, exercise.equipment || 'N/A');

    if (!generated || !generated.name) {
      console.log('   ❌ Não foi possível gerar dados');
      return 'error';
    }

    console.log(`   ✅ Nome sugerido: "${generated.name}"`);

    // Atualizar exercício
    const updates = {
      ...exercise,
      name: generated.name,
      description: generated.description,
      notes: generated.description
    };

    await updateExercise(token, exercise.id, updates);
    console.log('   💾 Exercício atualizado com sucesso');

    return 'updated';
  } catch (error) {
    console.log(`   ❌ Erro: ${error.message}`);
    return 'error';
  }
}

// Função principal
async function main() {
  console.log('🔧 Iniciando recuperação de exercícios vazios\n');

  try {
    // Login
    const token = await login();
    console.log('✅ Login realizado\n');

    // Buscar exercícios
    const exercises = await getExercises(token);
    console.log(`✅ ${exercises.length} exercícios encontrados\n`);

    // Filtrar exercícios vazios
    const emptyExercises = exercises.filter(ex => !ex.name || ex.name.trim() === '');
    console.log(`🔍 Encontrados ${emptyExercises.length} exercícios vazios\n`);

    if (emptyExercises.length === 0) {
      console.log('✅ Nenhum exercício vazio para recuperar!');
      return;
    }

    // Processar cada exercício
    let stats = { updated: 0, errors: 0 };

    for (let i = 0; i < emptyExercises.length; i++) {
      const result = await processEmptyExercise(token, emptyExercises[i], i, emptyExercises.length);
      stats[result]++;

      // Delay para não sobrecarregar a API
      await new Promise(resolve => setTimeout(resolve, 2000));
    }

    // Resultados finais
    console.log('\n\n' + '='.repeat(60));
    console.log('📊 RESULTADOS DA RECUPERAÇÃO');
    console.log('='.repeat(60));
    console.log(`✅ Recuperados: ${stats.updated}`);
    console.log(`❌ Erros: ${stats.errors}`);
    console.log(`📦 Total: ${emptyExercises.length}`);
    console.log('='.repeat(60));

  } catch (error) {
    console.error('\n❌ Erro fatal:', error.message);
    process.exit(1);
  }
}

// Executar
main();
