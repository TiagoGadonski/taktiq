const https = require('https');
const fs = require('fs');

// Configuração
const API_BASE = 'https://api.taktiq.app/api';
const ADMIN_EMAIL = 'admin@taktiq.app';
const ADMIN_PASSWORD = 'W3rt4juk@';

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

// Buscar todos os exercícios válidos
async function getExercises(token) {
  console.log('📥 Buscando exercícios válidos...');
  const response = await makeRequest(`${API_BASE}/exercises`, 'GET', null, {
    'Authorization': `Bearer ${token}`
  });
  return response;
}

// Buscar todos os planos de treino
async function getWorkoutPlans(token) {
  console.log('📥 Buscando planos de treino...');
  try {
    const response = await makeRequest(`${API_BASE}/workout-plans`, 'GET', null, {
      'Authorization': `Bearer ${token}`
    });
    return response;
  } catch (error) {
    console.log('⚠️  Nenhum plano de treino encontrado ou erro:', error.message);
    return [];
  }
}

// Buscar todos os treinos (workouts)
async function getWorkouts(token) {
  console.log('📥 Buscando treinos (workouts)...');
  try {
    const response = await makeRequest(`${API_BASE}/workouts`, 'GET', null, {
      'Authorization': `Bearer ${token}`
    });
    return response;
  } catch (error) {
    console.log('⚠️  Nenhum treino encontrado ou erro:', error.message);
    return [];
  }
}

// Verificar integridade
function checkIntegrity(items, validExerciseIds, itemType) {
  const brokenItems = [];

  for (const item of items) {
    const brokenExercises = [];

    // Verificar estrutura (pode variar dependendo da API)
    // Possíveis estruturas: exercises, workoutExercises, etc.
    const exercises = item.exercises || item.workoutExercises || [];

    for (const exercise of exercises) {
      const exerciseId = exercise.exerciseId || exercise.id;

      if (exerciseId && !validExerciseIds.has(exerciseId)) {
        brokenExercises.push({
          exerciseId: exerciseId,
          name: exercise.name || 'Sem nome'
        });
      }
    }

    if (brokenExercises.length > 0) {
      brokenItems.push({
        id: item.id,
        name: item.name || item.title || 'Sem nome',
        totalExercises: exercises.length,
        brokenExercises: brokenExercises,
        brokenCount: brokenExercises.length
      });
    }
  }

  return brokenItems;
}

// Função principal
async function main() {
  console.log('🔍 Verificando integridade dos treinos\n');

  try {
    // Login
    const token = await login();
    console.log('✅ Login realizado\n');

    // Carregar IDs dos exercícios deletados
    let deletedIds = [];
    if (fs.existsSync('empty-exercise-ids.json')) {
      deletedIds = JSON.parse(fs.readFileSync('empty-exercise-ids.json', 'utf8'));
      console.log(`📋 ${deletedIds.length} IDs de exercícios deletados carregados\n`);
    }

    // Buscar exercícios válidos
    const exercises = await getExercises(token);
    const validExerciseIds = new Set(exercises.map(ex => ex.id));
    console.log(`✅ ${exercises.length} exercícios válidos no sistema\n`);

    // Buscar planos de treino
    const workoutPlans = await getWorkoutPlans(token);
    console.log(`✅ ${workoutPlans.length} planos de treino encontrados\n`);

    // Buscar workouts
    const workouts = await getWorkouts(token);
    console.log(`✅ ${workouts.length} treinos encontrados\n`);

    // Verificar integridade
    const brokenPlans = checkIntegrity(workoutPlans, validExerciseIds, 'workout-plan');
    const brokenWorkouts = checkIntegrity(workouts, validExerciseIds, 'workout');

    // Relatório
    console.log('='.repeat(70));
    console.log('📊 ANÁLISE DE INTEGRIDADE');
    console.log('='.repeat(70));
    console.log(`Total de Planos de Treino: ${workoutPlans.length}`);
    console.log(`Planos com referências quebradas: ${brokenPlans.length}`);
    console.log(`\nTotal de Treinos (Workouts): ${workouts.length}`);
    console.log(`Treinos com referências quebradas: ${brokenWorkouts.length}`);
    console.log('='.repeat(70));

    // Detalhes dos planos quebrados
    if (brokenPlans.length > 0) {
      console.log('\n🚨 PLANOS DE TREINO COM PROBLEMAS:\n');
      brokenPlans.forEach((plan, index) => {
        console.log(`${index + 1}. "${plan.name}"`);
        console.log(`   ID: ${plan.id}`);
        console.log(`   Total de exercícios: ${plan.totalExercises}`);
        console.log(`   Exercícios quebrados: ${plan.brokenCount}`);
        console.log(`   IDs quebrados: ${plan.brokenExercises.map(e => e.exerciseId.substring(0, 8) + '...').join(', ')}`);
        console.log('');
      });

      // Salvar relatório
      fs.writeFileSync(
        'broken-workout-plans.json',
        JSON.stringify(brokenPlans, null, 2)
      );
      console.log('💾 Relatório detalhado salvo em: broken-workout-plans.json\n');
    }

    // Detalhes dos workouts quebrados
    if (brokenWorkouts.length > 0) {
      console.log('\n🚨 TREINOS (WORKOUTS) COM PROBLEMAS:\n');
      brokenWorkouts.forEach((workout, index) => {
        console.log(`${index + 1}. "${workout.name}"`);
        console.log(`   ID: ${workout.id}`);
        console.log(`   Total de exercícios: ${workout.totalExercises}`);
        console.log(`   Exercícios quebrados: ${workout.brokenCount}`);
        console.log('');
      });

      // Salvar relatório
      fs.writeFileSync(
        'broken-workouts.json',
        JSON.stringify(brokenWorkouts, null, 2)
      );
      console.log('💾 Relatório detalhado salvo em: broken-workouts.json\n');
    }

    // Resumo final
    console.log('\n' + '='.repeat(70));
    console.log('📝 RESUMO');
    console.log('='.repeat(70));

    const totalBroken = brokenPlans.length + brokenWorkouts.length;

    if (totalBroken === 0) {
      console.log('✅ Nenhum treino com referências quebradas! Sistema está íntegro.');
    } else {
      console.log(`⚠️  ${totalBroken} itens precisam de correção`);
      console.log('\n💡 PRÓXIMOS PASSOS:');
      console.log('   1. Revise os relatórios JSON gerados');
      console.log('   2. Execute o script de correção para remover exercícios quebrados');
      console.log('   3. Ou recrie os treinos com os novos exercícios disponíveis');
    }
    console.log('='.repeat(70));

  } catch (error) {
    console.error('\n❌ Erro:', error.message);
    process.exit(1);
  }
}

// Executar
main();
