const https = require('https');
const http = require('http');

// Configuração
const API_BASE = 'https://api.taktiq.app/api';
const GEMINI_API_KEY = process.env.GEMINI_API_KEY || 'AIzaSyDCIiSPd5IiaNLO_0OZ-LVcDGlN1OWqoNk'; // Da sua config
const ADMIN_EMAIL = 'admin@taktiq.app';
const ADMIN_PASSWORD = 'W3rt4juk@';

// Traduções manuais
const TRANSLATIONS = {
  'Push-up': 'Flexão de Braço',
  'Push up': 'Flexão de Braço',
  'Pushup': 'Flexão de Braço',
  'Pull-up': 'Barra Fixa',
  'Pull up': 'Barra Fixa',
  'Squat': 'Agachamento',
  'Bench Press': 'Supino Reto',
  'Deadlift': 'Levantamento Terra',
  'Plank': 'Prancha',
  'Crunch': 'Abdominal',
  'Lunge': 'Afundo',
  'Dip': 'Mergulho',
  'Bicep Curl': 'Rosca Bíceps',
  'Shoulder Press': 'Desenvolvimento de Ombros',
  'Lat Pulldown': 'Puxada Alta',
  'Leg Press': 'Leg Press',
  'Calf Raise': 'Elevação de Panturrilha',
  'Mountain Climbers': 'Escalador',
  'Burpee': 'Burpee',
  'Jump Rope': 'Pular Corda',
  'Face Pull': 'Puxada para o Rosto'
};

// URLs de vídeos do YouTube
const VIDEO_MAP = {
  'Flexão de Braço': 'https://www.youtube.com/watch?v=IODxDxX7oi4',
  'Barra Fixa': 'https://www.youtube.com/watch?v=eGo4IYlbE5g',
  'Agachamento': 'https://www.youtube.com/watch?v=aclHkVaku9U',
  'Supino Reto': 'https://www.youtube.com/watch?v=rT7DgCr-3pg',
  'Levantamento Terra': 'https://www.youtube.com/watch?v=op9kVnSso6Q',
  'Prancha': 'https://www.youtube.com/watch?v=ASdvN_XEl_c',
  'Abdominal': 'https://www.youtube.com/watch?v=Xyd_fa5zoEU',
  'Afundo': 'https://www.youtube.com/watch?v=QOVaHwm-Q6U',
  'Mergulho': 'https://www.youtube.com/watch?v=2z8JmcrW-As',
  'Rosca Bíceps': 'https://www.youtube.com/watch?v=ykJmrZ5v0Oo',
  'Desenvolvimento de Ombros': 'https://www.youtube.com/watch?v=qEwKCR5JCog',
  'Burpee': 'https://www.youtube.com/watch?v=auBLPXO8Fww',
  'Escalador': 'https://www.youtube.com/watch?v=nmwgirgXLYM',
  'Pular Corda': 'https://www.youtube.com/watch?v=FJmRQ5iTXKE'
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

// Traduzir nome
function translateName(name) {
  // Verifica tradução manual
  if (TRANSLATIONS[name]) {
    return TRANSLATIONS[name];
  }

  // Verifica se precisa tradução (contém palavras em inglês)
  const englishKeywords = ['push', 'pull', 'squat', 'press', 'curl', 'row', 'raise', 'fly', 'dip', 'crunch', 'plank', 'lunge', 'deadlift', 'bench'];
  const needsTranslation = englishKeywords.some(keyword => name.toLowerCase().includes(keyword));

  if (!needsTranslation) {
    return null; // Já está em português
  }

  return null; // Retorna null se não tem tradução manual
}

// Verificar se descrição é genérica
function isGenericDescription(description) {
  if (!description) return true;

  const genericPhrases = [
    'Execute o movimento com técnica correta',
    'Mantenha o controle durante toda a amplitude',
    'Respire adequadamente'
  ];

  return genericPhrases.some(phrase => description.includes(phrase));
}

// Gerar descrição com IA (Gemini)
async function generateDescription(exercise) {
  const prompt = `Gere uma descrição detalhada em português brasileiro para o exercício '${exercise.name}'.
Grupo muscular: ${exercise.muscleGroup}
Equipamento: ${exercise.equipment}

A descrição deve incluir:
1. Como executar o exercício (passo a passo)
2. Músculos trabalhados
3. Dicas de forma/técnica
4. Benefícios do exercício

Seja específico e detalhado (máximo 300 palavras).`;

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

    return response?.candidates?.[0]?.content?.parts?.[0]?.text?.trim();
  } catch (error) {
    console.error(`   ❌ Erro ao gerar descrição: ${error.message}`);
    return null;
  }
}

// Atualizar exercício via API
async function updateExercise(token, exerciseId, updates) {
  const updateRequest = {
    name: updates.name,
    description: updates.description,
    muscleGroup: updates.muscleGroup,
    category: updates.category,
    equipment: updates.equipment,
    notes: updates.notes,
    videoUrl: updates.videoUrl,
    imageUrl: updates.imageUrl,
    workoutLocation: updates.workoutLocation
  };

  await makeRequest(
    `${API_BASE}/exercises/${exerciseId}`,
    'PUT',
    updateRequest,
    { 'Authorization': `Bearer ${token}` }
  );
}

// Processar um exercício
async function processExercise(token, exercise, index, total) {
  console.log(`\n[${index + 1}/${total}] Processando: ${exercise.name}`);

  let wasUpdated = false;
  const updates = { ...exercise };

  // 1. Traduzir nome se necessário
  const translatedName = translateName(exercise.name);
  if (translatedName) {
    console.log(`   ✅ Traduzindo: "${exercise.name}" → "${translatedName}"`);
    updates.name = translatedName;
    wasUpdated = true;
  }

  // 2. Adicionar vídeo se não tiver
  if (!exercise.videoUrl) {
    const videoUrl = VIDEO_MAP[updates.name] || VIDEO_MAP[exercise.name];
    if (videoUrl) {
      console.log(`   ✅ Adicionando vídeo`);
      updates.videoUrl = videoUrl;
      wasUpdated = true;
    }
  }

  // 3. Melhorar descrição se for genérica
  if (isGenericDescription(exercise.description)) {
    console.log(`   🤖 Gerando descrição com IA...`);
    const newDescription = await generateDescription(exercise);
    if (newDescription) {
      console.log(`   ✅ Descrição atualizada`);
      updates.description = newDescription;
      updates.notes = newDescription;
      wasUpdated = true;
    }
  }

  // 4. Salvar se houve mudanças
  if (wasUpdated) {
    try {
      await updateExercise(token, exercise.id, updates);
      console.log(`   💾 Exercício atualizado com sucesso`);
      return 'updated';
    } catch (error) {
      console.log(`   ❌ Erro ao atualizar: ${error.message}`);
      return 'error';
    }
  } else {
    console.log(`   ⏭️  Nenhuma mudança necessária`);
    return 'skipped';
  }
}

// Função principal
async function main() {
  console.log('🚀 Iniciando aprimoramento de exercícios\n');

  try {
    // Login
    const token = await login();
    console.log('✅ Login realizado\n');

    // Buscar exercícios
    const exercises = await getExercises(token);
    console.log(`✅ ${exercises.length} exercícios encontrados\n`);

    // Processar cada exercício
    let stats = { updated: 0, skipped: 0, errors: 0 };

    for (let i = 0; i < exercises.length; i++) {
      const result = await processExercise(token, exercises[i], i, exercises.length);
      stats[result]++;

      // Delay para não sobrecarregar a API
      await new Promise(resolve => setTimeout(resolve, 1000));
    }

    // Resultados finais
    console.log('\n\n' + '='.repeat(50));
    console.log('📊 RESULTADOS FINAIS');
    console.log('='.repeat(50));
    console.log(`✅ Atualizados: ${stats.updated}`);
    console.log(`⏭️  Pulados: ${stats.skipped}`);
    console.log(`❌ Erros: ${stats.errors}`);
    console.log(`📦 Total: ${exercises.length}`);
    console.log('='.repeat(50));

  } catch (error) {
    console.error('\n❌ Erro fatal:', error.message);
    process.exit(1);
  }
}

// Executar
main();
