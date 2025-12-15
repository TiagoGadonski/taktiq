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
        if (res.statusCode >= 200 && res.statusCode < 300) {
          try {
            const json = body ? JSON.parse(body) : {};
            resolve(json);
          } catch (e) {
            resolve(body);
          }
        } else {
          reject(new Error(`HTTP ${res.statusCode}: ${body}`));
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

// Deletar exercício
async function deleteExercise(token, exerciseId) {
  await makeRequest(
    `${API_BASE}/exercises/${exerciseId}`,
    'DELETE',
    null,
    { 'Authorization': `Bearer ${token}` }
  );
}

// Função principal
async function main() {
  console.log('🗑️  Iniciando limpeza de exercícios vazios\n');

  try {
    // Carregar IDs
    if (!fs.existsSync('empty-exercise-ids.json')) {
      console.error('❌ Arquivo empty-exercise-ids.json não encontrado!');
      console.log('Execute primeiro: node find-empty-exercises.js');
      process.exit(1);
    }

    const ids = JSON.parse(fs.readFileSync('empty-exercise-ids.json', 'utf8'));
    console.log(`📋 ${ids.length} exercícios vazios para deletar\n`);

    // Login
    const token = await login();
    console.log('✅ Login realizado\n');

    // Deletar cada exercício
    let stats = { deleted: 0, errors: 0 };

    for (let i = 0; i < ids.length; i++) {
      const id = ids[i];
      try {
        await deleteExercise(token, id);
        stats.deleted++;
        console.log(`[${i + 1}/${ids.length}] ✅ Deletado: ${id}`);

        // Delay para não sobrecarregar a API
        await new Promise(resolve => setTimeout(resolve, 300));
      } catch (error) {
        stats.errors++;
        console.log(`[${i + 1}/${ids.length}] ❌ Erro ao deletar ${id}: ${error.message}`);
      }
    }

    // Resultados finais
    console.log('\n' + '='.repeat(60));
    console.log('📊 RESULTADOS DA LIMPEZA');
    console.log('='.repeat(60));
    console.log(`✅ Deletados: ${stats.deleted}`);
    console.log(`❌ Erros: ${stats.errors}`);
    console.log(`📦 Total: ${ids.length}`);
    console.log('='.repeat(60));

  } catch (error) {
    console.error('\n❌ Erro fatal:', error.message);
    process.exit(1);
  }
}

// Executar
main();
