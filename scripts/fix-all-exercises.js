const https = require('https');

// Configuração
const API_BASE = 'https://api.taktiq.app/api';
const ADMIN_EMAIL = 'admin@taktiq.app';
const ADMIN_PASSWORD = 'W3rt4juk@';

// Mapeamento de equipamento para local
const EQUIPMENT_TO_LOCATION = {
  // Academia
  'barbell': 'Gym',
  'dumbbell': 'Both', // Pode ter em casa ou na academia
  'machine': 'Gym',
  'cable': 'Gym',
  'kettlebells': 'Both',
  'kettlebell': 'Both',
  'ez barbell': 'Gym',
  'other': 'Gym',

  // Casa/Ambos
  'body only': 'Home',
  'bodyweight': 'Home',
  'none (bodyweight exercise)': 'Home',
  'resistance band': 'Home',
  'gym mat': 'Home',
  'pull-up bar': 'Both',
  'parallel bars': 'Gym',
  'bench': 'Gym',
  'trx': 'Gym',
  'foam roller': 'Both',
  'wall': 'Home',
  'chair': 'Home',
  'table': 'Home'
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

// Atualizar exercício
async function updateExercise(token, exerciseId, updates) {
  await makeRequest(
    `${API_BASE}/exercises/${exerciseId}`,
    'PUT',
    updates,
    { 'Authorization': `Bearer ${token}` }
  );
}

// Determinar local baseado no equipamento
function determineLocation(equipment) {
  if (!equipment) return 'Both';

  const equipmentLower = equipment.toLowerCase();

  // Busca exata
  if (EQUIPMENT_TO_LOCATION[equipmentLower]) {
    return EQUIPMENT_TO_LOCATION[equipmentLower];
  }

  // Busca parcial
  if (equipmentLower.includes('barbell') || equipmentLower.includes('barra')) return 'Gym';
  if (equipmentLower.includes('machine') || equipmentLower.includes('máquina')) return 'Gym';
  if (equipmentLower.includes('cable') || equipmentLower.includes('polia')) return 'Gym';
  if (equipmentLower.includes('body') || equipmentLower.includes('peso corporal')) return 'Home';
  if (equipmentLower.includes('dumbbell') || equipmentLower.includes('halter')) return 'Both';
  if (equipmentLower.includes('kettlebell')) return 'Both';
  if (equipmentLower.includes('band') || equipmentLower.includes('elástico')) return 'Home';

  return 'Both'; // Default
}

// Gerar descrição básica
function generateBasicDescription(exercise) {
  const name = exercise.name || 'Exercício';
  const muscleGroup = exercise.muscleGroup || 'músculos';
  const equipment = exercise.equipment || 'equipamento';

  return `${name} é um exercício que trabalha ${muscleGroup}. ` +
         `Equipamento necessário: ${equipment}. ` +
         `Execute o movimento com técnica correta e controle, mantendo a respiração adequada durante toda a amplitude.`;
}

// Processar exercício
async function processExercise(token, exercise, index, total) {
  console.log(`[${index + 1}/${total}] Processando: ${exercise.name || 'SEM NOME'}`);

  let updated = false;
  const updates = { ...exercise };

  // 1. Adicionar workoutLocation se faltando
  if (!exercise.workoutLocation || exercise.workoutLocation === '') {
    const location = determineLocation(exercise.equipment);
    updates.workoutLocation = location;
    console.log(`   ✅ Local definido: ${location}`);
    updated = true;
  }

  // 2. Adicionar descrição se faltando
  if (!exercise.description || exercise.description === '') {
    updates.description = generateBasicDescription(exercise);
    console.log(`   ✅ Descrição gerada`);
    updated = true;
  }

  // 3. Adicionar notes se faltando (copia da descrição)
  if (!exercise.notes || exercise.notes === '') {
    updates.notes = updates.description || generateBasicDescription(exercise);
    updated = true;
  }

  // 4. Adicionar placeholder de vídeo/imagem (sem sobrescrever os existentes)
  if (!exercise.videoUrl || exercise.videoUrl === '') {
    // Deixar vazio por enquanto - melhor do que colocar URL inválida
    updates.videoUrl = '';
  }

  if (!exercise.imageUrl || exercise.imageUrl === '') {
    // Deixar vazio por enquanto - melhor do que colocar URL inválida
    updates.imageUrl = '';
  }

  // 5. Atualizar se houve mudanças
  if (updated) {
    try {
      await updateExercise(token, exercise.id, updates);
      console.log(`   💾 Atualizado com sucesso`);
      return 'updated';
    } catch (error) {
      console.log(`   ❌ Erro: ${error.message}`);
      return 'error';
    }
  } else {
    console.log(`   ⏭️  Já completo`);
    return 'skipped';
  }
}

// Função principal
async function main() {
  console.log('🔧 Corrigindo exercícios incompletos\n');

  try {
    // Login
    const token = await login();
    console.log('✅ Login realizado\n');

    // Buscar exercícios
    const exercises = await getExercises(token);
    console.log(`✅ ${exercises.length} exercícios encontrados\n`);

    // Processar cada exercício
    let stats = { updated: 0, skipped: 0, errors: 0 };
    let locationStats = { Gym: 0, Home: 0, Both: 0 };

    for (let i = 0; i < exercises.length; i++) {
      const result = await processExercise(token, exercises[i], i, exercises.length);
      stats[result]++;

      // Contar locais
      const location = exercises[i].workoutLocation || determineLocation(exercises[i].equipment);
      if (locationStats[location] !== undefined) {
        locationStats[location]++;
      }

      // Delay para não sobrecarregar a API
      await new Promise(resolve => setTimeout(resolve, 300));
    }

    // Resultados finais
    console.log('\n' + '='.repeat(70));
    console.log('📊 RESULTADOS');
    console.log('='.repeat(70));
    console.log(`✅ Atualizados: ${stats.updated}`);
    console.log(`⏭️  Já completos: ${stats.skipped}`);
    console.log(`❌ Erros: ${stats.errors}`);
    console.log(`📦 Total: ${exercises.length}`);
    console.log('='.repeat(70));

    console.log('\n📍 DISTRIBUIÇÃO POR LOCAL:');
    console.log(`   🏋️  Academia (Gym): ${locationStats.Gym}`);
    console.log(`   🏠 Casa (Home): ${locationStats.Home}`);
    console.log(`   🔀 Ambos (Both): ${locationStats.Both}`);
    console.log('='.repeat(70));

    console.log('\n💡 PRÓXIMOS PASSOS:');
    console.log('   ✅ Filtro academia/casa agora funcionará corretamente');
    console.log('   ⚠️  Vídeos e imagens ainda precisam ser adicionados manualmente');
    console.log('   ℹ️  Descrições básicas foram geradas - podem ser melhoradas depois');

  } catch (error) {
    console.error('\n❌ Erro fatal:', error.message);
    process.exit(1);
  }
}

// Executar
main();
