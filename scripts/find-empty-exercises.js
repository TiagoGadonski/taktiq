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
  console.log('🔍 Procurando exercícios vazios\n');

  try {
    // Login
    const token = await login();
    console.log('✅ Login realizado\n');

    // Buscar exercícios
    const exercises = await getExercises(token);
    console.log(`✅ ${exercises.length} exercícios encontrados\n`);

    // Filtrar exercícios vazios
    const emptyExercises = exercises.filter(ex => !ex.name || ex.name.trim() === '');

    console.log('='.repeat(60));
    console.log('📊 ANÁLISE DE EXERCÍCIOS VAZIOS');
    console.log('='.repeat(60));
    console.log(`Total de exercícios: ${exercises.length}`);
    console.log(`Exercícios vazios: ${emptyExercises.length}`);
    console.log('='.repeat(60));

    if (emptyExercises.length > 0) {
      console.log('\n📋 Lista de IDs dos exercícios vazios:\n');
      emptyExercises.forEach((ex, index) => {
        console.log(`${index + 1}. ID: ${ex.id} | MuscleGroup: ${ex.muscleGroup || 'N/A'} | Equipment: ${ex.equipment || 'N/A'}`);
      });

      // Salvar IDs em arquivo
      const fs = require('fs');
      const ids = emptyExercises.map(ex => ex.id);
      fs.writeFileSync(
        'empty-exercise-ids.json',
        JSON.stringify(ids, null, 2)
      );
      console.log('\n💾 IDs salvos em: empty-exercise-ids.json');
    } else {
      console.log('\n✅ Nenhum exercício vazio encontrado!');
    }

  } catch (error) {
    console.error('\n❌ Erro:', error.message);
    process.exit(1);
  }
}

// Executar
main();
