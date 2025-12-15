const https = require('https');

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

// Buscar todos os exercícios
async function getExercises(token) {
  console.log('📥 Buscando exercícios...');
  const response = await makeRequest(`${API_BASE}/exercises`, 'GET', null, {
    'Authorization': `Bearer ${token}`
  });
  return response;
}

// Função principal
async function main() {
  console.log('🔍 Verificando exercícios incompletos\n');

  try {
    // Login
    const token = await login();
    console.log('✅ Login realizado\n');

    // Buscar exercícios
    const exercises = await getExercises(token);
    console.log(`✅ ${exercises.length} exercícios encontrados\n`);

    // Categorizar exercícios
    const incomplete = {
      noDescription: [],
      noVideo: [],
      noImage: [],
      noLocation: [],
      noNotes: []
    };

    exercises.forEach(ex => {
      if (!ex.description || (typeof ex.description === 'string' && ex.description.trim() === '')) {
        incomplete.noDescription.push(ex);
      }
      if (!ex.videoUrl || (typeof ex.videoUrl === 'string' && ex.videoUrl.trim() === '')) {
        incomplete.noVideo.push(ex);
      }
      if (!ex.imageUrl || (typeof ex.imageUrl === 'string' && ex.imageUrl.trim() === '')) {
        incomplete.noImage.push(ex);
      }
      if (!ex.workoutLocation || (typeof ex.workoutLocation === 'string' && ex.workoutLocation.trim() === '')) {
        incomplete.noLocation.push(ex);
      }
      if (!ex.notes || (typeof ex.notes === 'string' && ex.notes.trim() === '')) {
        incomplete.noNotes.push(ex);
      }
    });

    // Relatório
    console.log('='.repeat(70));
    console.log('📊 ANÁLISE DE EXERCÍCIOS INCOMPLETOS');
    console.log('='.repeat(70));
    console.log(`Total de exercícios: ${exercises.length}`);
    console.log(`\nExercícios faltando:`);
    console.log(`   📝 Descrição: ${incomplete.noDescription.length}`);
    console.log(`   🎥 Vídeo: ${incomplete.noVideo.length}`);
    console.log(`   🖼️  Imagem: ${incomplete.noImage.length}`);
    console.log(`   📍 Local (Gym/Home): ${incomplete.noLocation.length}`);
    console.log(`   📋 Notas: ${incomplete.noNotes.length}`);
    console.log('='.repeat(70));

    // Detalhes dos sem descrição
    if (incomplete.noDescription.length > 0) {
      console.log('\n🚨 EXERCÍCIOS SEM DESCRIÇÃO:\n');
      incomplete.noDescription.slice(0, 20).forEach((ex, idx) => {
        console.log(`${idx + 1}. ${ex.name || 'SEM NOME'} (${ex.muscleGroup || 'N/A'}) - ${ex.workoutLocation || 'Sem local'}`);
      });
      if (incomplete.noDescription.length > 20) {
        console.log(`   ... e mais ${incomplete.noDescription.length - 20} exercícios`);
      }
    }

    // Detalhes dos sem vídeo
    if (incomplete.noVideo.length > 0) {
      console.log('\n🚨 EXERCÍCIOS SEM VÍDEO:\n');
      incomplete.noVideo.slice(0, 20).forEach((ex, idx) => {
        console.log(`${idx + 1}. ${ex.name || 'SEM NOME'} (${ex.muscleGroup || 'N/A'}) - ${ex.workoutLocation || 'Sem local'}`);
      });
      if (incomplete.noVideo.length > 20) {
        console.log(`   ... e mais ${incomplete.noVideo.length - 20} exercícios`);
      }
    }

    // Detalhes dos sem local
    if (incomplete.noLocation.length > 0) {
      console.log('\n🚨 EXERCÍCIOS SEM LOCAL (CRÍTICO PARA FILTRO):\n');
      incomplete.noLocation.forEach((ex, idx) => {
        console.log(`${idx + 1}. ${ex.name || 'SEM NOME'} (${ex.muscleGroup || 'N/A'}) - Equipment: ${ex.equipment || 'N/A'}`);
      });
    }

    console.log('\n');

  } catch (error) {
    console.error('\n❌ Erro:', error.message);
    process.exit(1);
  }
}

// Executar
main();
