const https = require('https');

const API_BASE = 'https://api.taktiq.app/api';
const ADMIN_EMAIL = 'admin@taktiq.app';
const ADMIN_PASSWORD = 'W3rt4juk@';

// Mapeamento de vídeos
const VIDEO_MAP = {
  'supino reto': 'https://www.youtube.com/watch?v=rT7DgCr-3pg',
  'supino inclinado': 'https://www.youtube.com/watch?v=8iPEnn-ltC8',
  'supino declinado': 'https://www.youtube.com/watch?v=LfyQBUKR8SE',
  'crucifixo': 'https://www.youtube.com/watch?v=eozdVDA78K0',
  'crossover': 'https://www.youtube.com/watch?v=taI4XduLpTk',
  'peck deck': 'https://www.youtube.com/watch?v=Z77JJjaIdLw',
  'pullover': 'https://www.youtube.com/watch?v=3FMhhq8PzAA',
  'flexão': 'https://www.youtube.com/watch?v=IODxDxX7oi4',
  'barra fixa': 'https://www.youtube.com/watch?v=eGo4IYlbE5g',
  'puxada': 'https://www.youtube.com/watch?v=CAwf7n6Luuc',
  'pulldown': 'https://www.youtube.com/watch?v=CAwf7n6Luuc',
  'remada': 'https://www.youtube.com/watch?v=FWJR5Ve8bnQ',
  'serrote': 'https://www.youtube.com/watch?v=roCP6wCXPqo',
  'agachamento': 'https://www.youtube.com/watch?v=aclHkVaku9U',
  'leg press': 'https://www.youtube.com/watch?v=IZxyjW7MPJQ',
  'cadeira extensora': 'https://www.youtube.com/watch?v=YyvSfEjZIz0',
  'mesa flexora': 'https://www.youtube.com/watch?v=ELOCsoDSmrg',
  'levantamento terra': 'https://www.youtube.com/watch?v=op9kVnSso6Q',
  'deadlift': 'https://www.youtube.com/watch?v=op9kVnSso6Q',
  'stiff': 'https://www.youtube.com/watch?v=1uDiW5--rAE',
  'afundo': 'https://www.youtube.com/watch?v=QOVaHwm-Q6U',
  'lunge': 'https://www.youtube.com/watch?v=QOVaHwm-Q6U',
  'búlgaro': 'https://www.youtube.com/watch?v=2C-uNgKwPLE',
  'bulgarian': 'https://www.youtube.com/watch?v=2C-uNgKwPLE',
  'adutora': 'https://www.youtube.com/watch?v=Drebsv8w5cY',
  'abdutora': 'https://www.youtube.com/watch?v=8sG08epoche',
  'hack squat': 'https://www.youtube.com/watch?v=0tn5K9NlCfo',
  'panturrilha': 'https://www.youtube.com/watch?v=gwLzBJYoWlI',
  'calf': 'https://www.youtube.com/watch?v=gwLzBJYoWlI',
  'desenvolvimento': 'https://www.youtube.com/watch?v=qEwKCR5JCog',
  'elevação lateral': 'https://www.youtube.com/watch?v=3VcKaXpzqRo',
  'elevação frontal': 'https://www.youtube.com/watch?v=-t7fuZ0KhDA',
  'remada alta': 'https://www.youtube.com/watch?v=2TEWqSqF-Ig',
  'arnold': 'https://www.youtube.com/watch?v=6Z15_WdXmVw',
  'voo posterior': 'https://www.youtube.com/watch?v=tXawPJGmT1o',
  'rosca direta': 'https://www.youtube.com/watch?v=ykJmrZ5v0Oo',
  'rosca alternada': 'https://www.youtube.com/watch?v=sAq_ocpRh_I',
  'rosca martelo': 'https://www.youtube.com/watch?v=zC3nLlEvin4',
  'rosca scott': 'https://www.youtube.com/watch?v=fIWP-FRFNU0',
  'rosca concentrada': 'https://www.youtube.com/watch?v=Jvj2wV0vOdw',
  'rosca 21': 'https://www.youtube.com/watch?v=qJJZfBfJVWw',
  'rosca inversa': 'https://www.youtube.com/watch?v=nRgxYX2Ve9w',
  'tríceps testa': 'https://www.youtube.com/watch?v=d_KZxkY_0cM',
  'tríceps polia': 'https://www.youtube.com/watch?v=-xa-6cQaZKY',
  'pushdown': 'https://www.youtube.com/watch?v=-xa-6cQaZKY',
  'tríceps francês': 'https://www.youtube.com/watch?v=YbX7Wd8jQ-Q',
  'mergulho': 'https://www.youtube.com/watch?v=2z8JmcrW-As',
  'dip': 'https://www.youtube.com/watch?v=2z8JmcrW-As',
  'tríceps coice': 'https://www.youtube.com/watch?v=6SS6K3lAwZ8',
  'kickback': 'https://www.youtube.com/watch?v=6SS6K3lAwZ8',
  'abdominal': 'https://www.youtube.com/watch?v=Xyd_fa5zoEU',
  'crunch': 'https://www.youtube.com/watch?v=Xyd_fa5zoEU',
  'prancha': 'https://www.youtube.com/watch?v=ASdvN_XEl_c',
  'plank': 'https://www.youtube.com/watch?v=ASdvN_XEl_c',
  'elevação de pernas': 'https://www.youtube.com/watch?v=JB2oyawG9KI',
  'bicicleta': 'https://www.youtube.com/watch?v=9FGilxCbdz8',
  'bicycle': 'https://www.youtube.com/watch?v=9FGilxCbdz8',
  'burpee': 'https://www.youtube.com/watch?v=auBLPXO8Fww',
  'mountain climber': 'https://www.youtube.com/watch?v=nmwgirgXLYM',
  'escalador': 'https://www.youtube.com/watch?v=nmwgirgXLYM',
  'jumping jack': 'https://www.youtube.com/watch?v=c4DAnQ6DtF8',
  'polichinelo': 'https://www.youtube.com/watch?v=c4DAnQ6DtF8',
  'high knee': 'https://www.youtube.com/watch?v=8opcQdC-V-U',
  'bird dog': 'https://www.youtube.com/watch?v=wiFNA3sqjCA',
  'ponte': 'https://www.youtube.com/watch?v=wPM8icPu6H8',
  'bridge': 'https://www.youtube.com/watch?v=wPM8icPu6H8',
  'superman': 'https://www.youtube.com/watch?v=z6PJMT2y8GQ',
  'pistol': 'https://www.youtube.com/watch?v=vq5-vdgJc0I',
  'wall sit': 'https://www.youtube.com/watch?v=y-wV4Venusw'
};

const IMAGE_BASE = 'https://www.inspireusafoundation.org/wp-content/uploads';
const IMAGE_MAP = {
  'supino reto': IMAGE_BASE + '/2022/03/barbell-bench-press.gif',
  'supino inclinado': IMAGE_BASE + '/2022/10/incline-dumbbell-press.gif',
  'crucifixo': IMAGE_BASE + '/2022/02/dumbbell-chest-fly.gif',
  'flexão': IMAGE_BASE + '/2021/06/push-up.gif',
  'barra fixa': IMAGE_BASE + '/2022/02/pull-up.gif',
  'puxada': IMAGE_BASE + '/2022/02/lat-pulldown.gif',
  'remada': IMAGE_BASE + '/2022/02/barbell-row.gif',
  'serrote': IMAGE_BASE + '/2022/02/one-arm-dumbbell-row.gif',
  'agachamento': IMAGE_BASE + '/2021/11/barbell-squat.gif',
  'leg press': IMAGE_BASE + '/2022/02/leg-press.gif',
  'cadeira extensora': IMAGE_BASE + '/2022/02/leg-extension.gif',
  'mesa flexora': IMAGE_BASE + '/2022/02/lying-leg-curl.gif',
  'levantamento terra': IMAGE_BASE + '/2022/02/barbell-deadlift.gif',
  'afundo': IMAGE_BASE + '/2022/02/walking-lunge.gif',
  'búlgaro': IMAGE_BASE + '/2022/02/bulgarian-split-squat.gif',
  'panturrilha': IMAGE_BASE + '/2022/02/standing-calf-raise.gif',
  'desenvolvimento': IMAGE_BASE + '/2022/02/dumbbell-shoulder-press.gif',
  'elevação lateral': IMAGE_BASE + '/2022/02/dumbbell-lateral-raise.gif',
  'elevação frontal': IMAGE_BASE + '/2022/02/dumbbell-front-raise.gif',
  'remada alta': IMAGE_BASE + '/2022/02/barbell-upright-row.gif',
  'rosca direta': IMAGE_BASE + '/2022/02/barbell-curl.gif',
  'rosca alternada': IMAGE_BASE + '/2022/02/alternating-dumbbell-curl.gif',
  'rosca martelo': IMAGE_BASE + '/2022/02/hammer-curl.gif',
  'rosca scott': IMAGE_BASE + '/2022/02/preacher-curl.gif',
  'tríceps testa': IMAGE_BASE + '/2022/02/barbell-skullcrusher.gif',
  'tríceps polia': IMAGE_BASE + '/2022/02/cable-pushdown.gif',
  'tríceps francês': IMAGE_BASE + '/2022/02/overhead-dumbbell-triceps-extension.gif',
  'mergulho': IMAGE_BASE + '/2022/02/triceps-dips.gif',
  'abdominal': IMAGE_BASE + '/2022/02/crunch.gif',
  'prancha': IMAGE_BASE + '/2022/02/plank.gif',
  'elevação de pernas': IMAGE_BASE + '/2022/02/lying-leg-raise.gif',
  'bicicleta': IMAGE_BASE + '/2022/02/bicycle-crunch.gif',
  'burpee': IMAGE_BASE + '/2022/02/burpee.gif',
  'mountain climber': IMAGE_BASE + '/2022/02/mountain-climber.gif',
  'escalador': IMAGE_BASE + '/2022/02/mountain-climber.gif',
  'jumping jack': IMAGE_BASE + '/2022/02/jumping-jack.gif',
  'bird dog': IMAGE_BASE + '/2023/06/bird-dog.gif',
  'ponte': IMAGE_BASE + '/2022/02/glute-bridge.gif',
  'superman': IMAGE_BASE + '/2022/02/superman-exercise.gif',
  'pistol': IMAGE_BASE + '/2022/02/pistol-squat.gif'
};

function makeRequest(url, method, data, headers) {
  return new Promise((resolve, reject) => {
    const urlObj = new URL(url);
    const options = {
      hostname: urlObj.hostname,
      port: urlObj.port || 443,
      path: urlObj.pathname + urlObj.search,
      method: method || 'GET',
      headers: Object.assign({ 'Content-Type': 'application/json' }, headers || {})
    };

    const req = https.request(options, (res) => {
      let body = '';
      res.on('data', (chunk) => body += chunk);
      res.on('end', () => {
        try {
          resolve(JSON.parse(body));
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

async function login() {
  console.log('Login...');
  const response = await makeRequest(API_BASE + '/auth/login', 'POST', {
    email: ADMIN_EMAIL,
    password: ADMIN_PASSWORD
  });
  return response.token;
}

async function getExercises(token) {
  console.log('Buscando exercicios...');
  return await makeRequest(API_BASE + '/exercises', 'GET', null, {
    'Authorization': 'Bearer ' + token
  });
}

async function updateExercise(token, id, updates) {
  return await makeRequest(API_BASE + '/exercises/' + id, 'PUT', updates, {
    'Authorization': 'Bearer ' + token
  });
}

function findMatch(name, map) {
  if (!name) return null;
  const lower = name.toLowerCase();
  for (const key in map) {
    if (lower.includes(key)) return map[key];
  }
  return null;
}

async function main() {
  console.log('=== Adicionando videos e imagens ===\n');

  try {
    const token = await login();
    console.log('Login OK\n');

    const exercises = await getExercises(token);
    console.log('Total: ' + exercises.length + ' exercicios\n');

    const needsUpdate = exercises.filter(ex =>
      !ex.videoUrl || ex.videoUrl === '' || !ex.imageUrl || ex.imageUrl === ''
    );

    console.log('Precisam de update: ' + needsUpdate.length + '\n');

    let stats = { updated: 0, skipped: 0, errors: 0 };

    for (let i = 0; i < needsUpdate.length; i++) {
      const ex = needsUpdate[i];
      const name = ex.name || 'SEM NOME';
      console.log('[' + (i + 1) + '/' + needsUpdate.length + '] ' + name);

      let updated = false;
      const updates = Object.assign({}, ex);

      if (!ex.videoUrl || ex.videoUrl === '') {
        const video = findMatch(ex.name, VIDEO_MAP);
        if (video) {
          updates.videoUrl = video;
          console.log('  Video adicionado');
          updated = true;
        }
      }

      if (!ex.imageUrl || ex.imageUrl === '') {
        const image = findMatch(ex.name, IMAGE_MAP);
        if (image) {
          updates.imageUrl = image;
          console.log('  Imagem adicionada');
          updated = true;
        }
      }

      if (updated) {
        try {
          await updateExercise(token, ex.id, updates);
          console.log('  Atualizado!\n');
          stats.updated++;
        } catch (error) {
          console.log('  ERRO: ' + error.message + '\n');
          stats.errors++;
        }
      } else {
        console.log('  Sem correspondencia\n');
        stats.skipped++;
      }

      await new Promise(resolve => setTimeout(resolve, 300));
    }

    console.log('\n=== RESULTADOS ===');
    console.log('Atualizados: ' + stats.updated);
    console.log('Sem correspondencia: ' + stats.skipped);
    console.log('Erros: ' + stats.errors);

  } catch (error) {
    console.error('ERRO FATAL:', error.message);
    process.exit(1);
  }
}

main();
