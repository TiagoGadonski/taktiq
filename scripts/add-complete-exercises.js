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

// Criar exercício
async function createExercise(token, exercise) {
  await makeRequest(
    `${API_BASE}/exercises`,
    'POST',
    exercise,
    { 'Authorization': `Bearer ${token}` }
  );
}

// Lista de exercícios completos
const EXERCISES = [
  // ========== PEITO ==========
  {
    name: 'Supino Reto com Barra',
    description: 'Deite-se no banco com os pés firmes no chão. Segure a barra com pegada um pouco mais larga que os ombros. Desça a barra controladamente até o meio do peito e empurre de volta até a extensão completa dos braços. Mantenha os ombros retraídos e o core contraído durante todo o movimento.',
    muscleGroup: 'Pectoralis major',
    category: 'Strength',
    equipment: 'Barbell, Bench',
    notes: 'Exercício fundamental para desenvolvimento do peitoral. Trabalha principalmente a porção média do peitoral, além dos tríceps e ombros. Mantenha os cotovelos a 45 graus do corpo para proteger os ombros.',
    videoUrl: 'https://www.youtube.com/watch?v=rT7DgCr-3pg',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/03/barbell-bench-press.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Supino Inclinado com Halteres',
    description: 'Ajuste o banco entre 30-45 graus. Deite-se e segure os halteres ao lado do peito. Empurre os halteres para cima até a extensão completa, mantendo-os alinhados. Desça controladamente até sentir alongamento no peitoral superior.',
    muscleGroup: 'Pectoralis major',
    category: 'Strength',
    equipment: 'Dumbbell, Bench',
    notes: 'Foca na porção clavicular (superior) do peitoral. Ideal para quem quer desenvolver a parte de cima do peito. Os halteres permitem maior amplitude de movimento que a barra.',
    videoUrl: 'https://www.youtube.com/watch?v=8iPEnn-ltC8',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/10/incline-dumbbell-press.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Crucifixo com Halteres',
    description: 'Deite-se no banco plano segurando os halteres acima do peito com os braços estendidos. Com uma leve flexão nos cotovelos, abra os braços em arco até sentir alongamento no peito. Retorne à posição inicial contraindo o peitoral.',
    muscleGroup: 'Pectoralis major',
    category: 'Strength',
    equipment: 'Dumbbell, Bench',
    notes: 'Exercício de isolamento para o peitoral. Excelente para trabalhar a amplitude de movimento e alongamento muscular. Não use peso excessivo para evitar lesões nos ombros.',
    videoUrl: 'https://www.youtube.com/watch?v=eozdVDA78K0',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/dumbbell-chest-fly.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Flexão de Braço Padrão',
    description: 'Posicione as mãos no chão na largura dos ombros. Mantenha o corpo reto da cabeça aos pés. Desça até o peito quase tocar o chão, mantendo os cotovelos próximos ao corpo. Empurre de volta até a posição inicial.',
    muscleGroup: 'Pectoralis major',
    category: 'Strength',
    equipment: 'none (bodyweight exercise)',
    notes: 'Exercício clássico de peso corporal. Trabalha peito, ombros e tríceps. Perfeito para treinar em casa. Pode ser modificado para diferentes níveis: joelhos para iniciantes, com elevação para avançados.',
    videoUrl: 'https://www.youtube.com/watch?v=IODxDxX7oi4',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2021/06/push-up.gif',
    workoutLocation: 'Home'
  },

  // ========== COSTAS ==========
  {
    name: 'Barra Fixa Pronada',
    description: 'Segure a barra com as palmas para frente, mãos um pouco mais largas que os ombros. Puxe o corpo para cima até o queixo passar da barra. Desça controladamente até a extensão completa dos braços. Mantenha o core contraído.',
    muscleGroup: 'Latissimus dorsi',
    category: 'Strength',
    equipment: 'Pull-up bar',
    notes: 'Um dos melhores exercícios para desenvolvimento das costas. Trabalha principalmente o grande dorsal, além de bíceps e antebraços. Use elástico ou máquina assistida se necessário.',
    videoUrl: 'https://www.youtube.com/watch?v=eGo4IYlbE5g',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/pull-up.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Remada Curvada com Barra',
    description: 'Fique em pé com joelhos levemente flexionados. Incline o tronco para frente mantendo as costas retas. Puxe a barra em direção ao abdômen, levando os cotovelos para trás. Contraia as escápulas no topo do movimento.',
    muscleGroup: 'Latissimus dorsi',
    category: 'Strength',
    equipment: 'Barbell',
    notes: 'Excelente para espessura das costas. Trabalha grande dorsal, trapézio médio e romboides. Mantenha sempre as costas retas para evitar lesões lombares.',
    videoUrl: 'https://www.youtube.com/watch?v=FWJR5Ve8bnQ',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/barbell-row.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Puxada Frontal',
    description: 'Sente-se na máquina com as coxas fixas. Segure a barra com pegada larga. Puxe a barra até a altura do peito, levando os cotovelos para baixo e para trás. Contraia as escápulas e retorne controladamente.',
    muscleGroup: 'Latissimus dorsi',
    category: 'Strength',
    equipment: 'Cable machine',
    notes: 'Variação da barra fixa com carga ajustável. Ótimo para iniciantes desenvolverem força para fazer barras fixas. Trabalha largura das costas.',
    videoUrl: 'https://www.youtube.com/watch?v=CAwf7n6Luuc',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/lat-pulldown.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Remada Unilateral com Halter',
    description: 'Apoie um joelho e uma mão no banco. Com a outra mão, segure o halter e puxe-o em direção ao quadril, mantendo o cotovelo próximo ao corpo. Contraia a escápula no topo do movimento.',
    muscleGroup: 'Latissimus dorsi',
    category: 'Strength',
    equipment: 'Dumbbell, Bench',
    notes: 'Permite focar em cada lado separadamente, corrigindo assimetrias. Excelente amplitude de movimento. Trabalha grande dorsal e trapézio.',
    videoUrl: 'https://www.youtube.com/watch?v=roCP6wCXPqo',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/one-arm-dumbbell-row.gif',
    workoutLocation: 'Gym'
  },

  // ========== PERNAS ==========
  {
    name: 'Agachamento Livre',
    description: 'Fique em pé com os pés na largura dos ombros. Desça flexionando os joelhos e quadris, mantendo o peito elevado e as costas retas. Desça até as coxas ficarem paralelas ao chão. Empurre pelos calcanhares para subir.',
    muscleGroup: 'Quadriceps femoris',
    category: 'Strength',
    equipment: 'none (bodyweight exercise)',
    notes: 'Exercício fundamental para pernas. Trabalha quadríceps, glúteos e posterior de coxa. Pode ser feito em casa sem equipamento. Mantenha os joelhos alinhados com os pés.',
    videoUrl: 'https://www.youtube.com/watch?v=aclHkVaku9U',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/10/bodyweight-squat.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Agachamento com Barra',
    description: 'Posicione a barra nas costas (trapézio). Pés na largura dos ombros, pontas levemente para fora. Desça controladamente mantendo o peito para cima até as coxas paralelas ao chão. Empurre pelos calcanhares para subir.',
    muscleGroup: 'Quadriceps femoris',
    category: 'Strength',
    equipment: 'Barbell, Squat rack',
    notes: 'O rei dos exercícios para pernas. Desenvolve força e massa muscular em toda a coxa e glúteos. Essencial manter a técnica correta para evitar lesões.',
    videoUrl: 'https://www.youtube.com/watch?v=ultWZbUMPL8',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2021/11/barbell-squat.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Leg Press 45°',
    description: 'Sente-se na máquina com as costas totalmente apoiadas. Coloque os pés na plataforma na largura dos ombros. Empurre a plataforma até quase a extensão completa. Desça controladamente até os joelhos formarem 90 graus.',
    muscleGroup: 'Quadriceps femoris',
    category: 'Strength',
    equipment: 'Leg press machine',
    notes: 'Permite usar cargas altas com segurança. Trabalha quadríceps e glúteos. Não trave os joelhos na extensão completa.',
    videoUrl: 'https://www.youtube.com/watch?v=IZxyjW7MPJQ',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/leg-press.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Levantamento Terra',
    description: 'Fique em pé com a barra sobre os pés. Agache e segure a barra. Mantenha as costas retas, peito elevado. Empurre pelo chão, estendendo quadris e joelhos simultaneamente. Trave no topo e desça controladamente.',
    muscleGroup: 'Erector spinae',
    category: 'Strength',
    equipment: 'Barbell',
    notes: 'Exercício composto que trabalha corpo todo: posterior de coxa, glúteos, lombar, trapézio. Um dos 3 exercícios do powerlifting. Técnica é fundamental.',
    videoUrl: 'https://www.youtube.com/watch?v=op9kVnSso6Q',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/barbell-deadlift.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Afundo Caminhando',
    description: 'Dê um passo à frente e desça o joelho de trás em direção ao chão. O joelho da frente não deve ultrapassar a ponta do pé. Empurre pelo calcanhar da frente para levantar e dar o próximo passo.',
    muscleGroup: 'Quadriceps femoris',
    category: 'Strength',
    equipment: 'none (bodyweight exercise)',
    notes: 'Excelente para glúteos e quadríceps. Trabalha equilíbrio e coordenação. Pode ser feito com halteres para aumentar intensidade. Ideal para treino em casa.',
    videoUrl: 'https://www.youtube.com/watch?v=QOVaHwm-Q6U',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/walking-lunge.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Agachamento Búlgaro',
    description: 'Coloque um pé em um banco atrás de você. Com o outro pé, desça flexionando o joelho até formar 90 graus. A perna de trás serve apenas de apoio. Empurre pelo calcanhar da frente para subir.',
    muscleGroup: 'Quadriceps femoris',
    category: 'Strength',
    equipment: 'Bench',
    notes: 'Variação unilateral do agachamento. Excelente para glúteos e equilíbrio. Pode ser feito com halteres. Corrige assimetrias entre as pernas.',
    videoUrl: 'https://www.youtube.com/watch?v=2C-uNgKwPLE',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/bulgarian-split-squat.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Cadeira Extensora',
    description: 'Sente-se na máquina com as costas apoiadas. Coloque os tornozelos sob o rolo. Estenda as pernas até a contração completa dos quadríceps. Desça controladamente sem deixar o peso bater.',
    muscleGroup: 'Quadriceps femoris',
    category: 'Strength',
    equipment: 'Leg extension machine',
    notes: 'Isolamento para quadríceps. Bom para finalizar o treino de pernas ou pré-exaustão. Não use peso excessivo para proteger os joelhos.',
    videoUrl: 'https://www.youtube.com/watch?v=YyvSfEjZIz0',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/leg-extension.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Mesa Flexora',
    description: 'Deite-se de bruços na máquina. Coloque os tornozelos sob o rolo. Flexione os joelhos trazendo os calcanhares em direção aos glúteos. Contraia os posteriores de coxa no topo. Desça controladamente.',
    muscleGroup: 'Biceps femoris',
    category: 'Strength',
    equipment: 'Leg curl machine',
    notes: 'Isolamento para posterior de coxa. Importante para equilibrar o desenvolvimento das pernas. Complementa agachamentos e levantamento terra.',
    videoUrl: 'https://www.youtube.com/watch?v=ELOCsoDSmrg',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/lying-leg-curl.gif',
    workoutLocation: 'Gym'
  },

  // ========== OMBROS ==========
  {
    name: 'Desenvolvimento com Barra em Pé',
    description: 'Fique em pé com a barra na altura dos ombros. Empurre a barra verticalmente acima da cabeça até a extensão completa dos braços. Desça controladamente até a altura do queixo. Mantenha o core contraído.',
    muscleGroup: 'Anterior deltoid',
    category: 'Strength',
    equipment: 'Barbell',
    notes: 'Exercício composto para ombros. Trabalha deltoide anterior, lateral e tríceps. A versão em pé recruta mais o core que a sentada.',
    videoUrl: 'https://www.youtube.com/watch?v=2yjwXTZQDDI',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/standing-barbell-shoulder-press.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Desenvolvimento com Halteres Sentado',
    description: 'Sente-se em um banco com encosto. Segure os halteres ao lado dos ombros. Empurre-os para cima até quase travar os cotovelos. Desça controladamente até a posição inicial.',
    muscleGroup: 'Anterior deltoid',
    category: 'Strength',
    equipment: 'Dumbbell, Bench',
    notes: 'Permite maior amplitude que a barra. Trabalha os ombros de forma balanceada. O encosto ajuda a manter postura e focar nos deltoides.',
    videoUrl: 'https://www.youtube.com/watch?v=qEwKCR5JCog',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/dumbbell-shoulder-press.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Elevação Lateral com Halteres',
    description: 'Fique em pé com os halteres ao lado do corpo. Eleve os braços lateralmente até a altura dos ombros, mantendo leve flexão nos cotovelos. Desça controladamente. Os cotovelos devem liderar o movimento.',
    muscleGroup: 'Lateral deltoid',
    category: 'Strength',
    equipment: 'Dumbbell',
    notes: 'Isolamento para deltoide lateral. Essencial para ombros largos. Use peso moderado e foque na técnica. Evite balançar o corpo.',
    videoUrl: 'https://www.youtube.com/watch?v=3VcKaXpzqRo',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/dumbbell-lateral-raise.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Elevação Frontal com Halteres',
    description: 'Fique em pé com os halteres à frente das coxas. Eleve um halter de cada vez (ou ambos) até a altura dos ombros. Mantenha os braços estendidos com leve flexão nos cotovelos. Desça controladamente.',
    muscleGroup: 'Anterior deltoid',
    category: 'Strength',
    equipment: 'Dumbbell',
    notes: 'Isolamento para deltoide anterior. Complementa desenvolvimento e supino. Pode ser feito alternado ou simultâneo.',
    videoUrl: 'https://www.youtube.com/watch?v=-t7fuZ0KhDA',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/dumbbell-front-raise.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Remada Alta com Barra',
    description: 'Segure a barra com pegada fechada (mãos próximas). Puxe a barra verticalmente até a altura do peito, elevando os cotovelos acima dos ombros. Desça controladamente.',
    muscleGroup: 'Trapezius',
    category: 'Strength',
    equipment: 'Barbell',
    notes: 'Trabalha trapézio superior e deltoides laterais. Evite se tiver problemas nos ombros. Não use peso excessivo.',
    videoUrl: 'https://www.youtube.com/watch?v=2TEWqSqF-Ig',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/barbell-upright-row.gif',
    workoutLocation: 'Gym'
  },

  // ========== BÍCEPS ==========
  {
    name: 'Rosca Direta com Barra',
    description: 'Fique em pé segurando a barra com pegada supinada (palmas para cima). Flexione os cotovelos levando a barra em direção aos ombros. Mantenha os cotovelos fixos ao lado do corpo. Desça controladamente.',
    muscleGroup: 'Biceps brachii',
    category: 'Strength',
    equipment: 'Barbell',
    notes: 'Exercício clássico para bíceps. Trabalha ambas as cabeças do bíceps. Evite balançar o corpo para fazer força.',
    videoUrl: 'https://www.youtube.com/watch?v=ykJmrZ5v0Oo',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/barbell-curl.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Rosca Alternada com Halteres',
    description: 'Fique em pé com os halteres ao lado do corpo. Flexione um braço de cada vez, supinando o punho durante o movimento. Contraia o bíceps no topo. Desça controladamente e alterne os braços.',
    muscleGroup: 'Biceps brachii',
    category: 'Strength',
    equipment: 'Dumbbell',
    notes: 'Permite foco individual em cada braço. A supinação do punho aumenta a ativação do bíceps. Evite usar impulso.',
    videoUrl: 'https://www.youtube.com/watch?v=sAq_ocpRh_I',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/alternating-dumbbell-curl.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Rosca Martelo',
    description: 'Fique em pé com os halteres ao lado do corpo, palmas voltadas para dentro. Flexione os cotovelos mantendo essa pegada neutra. Contraia o bíceps no topo. Desça controladamente.',
    muscleGroup: 'Biceps brachii',
    category: 'Strength',
    equipment: 'Dumbbell',
    notes: 'Trabalha bíceps e braquial (músculo entre bíceps e tríceps). Ajuda a aumentar o volume do braço. Menos estresse no punho que a rosca tradicional.',
    videoUrl: 'https://www.youtube.com/watch?v=zC3nLlEvin4',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/hammer-curl.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Rosca Scott',
    description: 'Sente-se no banco Scott com os braços apoiados. Segure a barra com pegada supinada. Flexione os cotovelos levando a barra em direção aos ombros. Desça controladamente sem esticar totalmente os braços.',
    muscleGroup: 'Biceps brachii',
    category: 'Strength',
    equipment: 'EZ bar, Scott bench',
    notes: 'Isolamento total do bíceps. O apoio elimina ajuda do corpo. Excelente para pico do bíceps. Cuidado para não hiperestender os cotovelos.',
    videoUrl: 'https://www.youtube.com/watch?v=fIWP-FRFNU0',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/preacher-curl.gif',
    workoutLocation: 'Gym'
  },

  // ========== TRÍCEPS ==========
  {
    name: 'Tríceps Testa com Barra',
    description: 'Deite-se no banco segurando a barra acima da cabeça. Flexione apenas os cotovelos, descendo a barra em direção à testa. Estenda os cotovelos retornando à posição inicial. Cotovelos ficam fixos.',
    muscleGroup: 'Triceps brachii',
    category: 'Strength',
    equipment: 'Barbell, Bench',
    notes: 'Exercício de isolamento para tríceps. Trabalha principalmente a cabeça longa. Use barra W para menos estresse nos punhos.',
    videoUrl: 'https://www.youtube.com/watch?v=d_KZxkY_0cM',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/barbell-skullcrusher.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Tríceps na Polia',
    description: 'Fique em frente à polia alta segurando a barra. Cotovelos fixos ao lado do corpo. Empurre a barra para baixo até a extensão completa dos braços. Retorne controladamente.',
    muscleGroup: 'Triceps brachii',
    category: 'Strength',
    equipment: 'Cable machine',
    notes: 'Isolamento para tríceps com tensão constante. Ótimo para definição. Pode usar diferentes pegadores (barra, corda, V).',
    videoUrl: 'https://www.youtube.com/watch?v=-xa-6cQaZKY',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/cable-pushdown.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Tríceps Francês com Halter',
    description: 'Fique em pé ou sentado segurando um halter com ambas as mãos acima da cabeça. Flexione os cotovelos descendo o halter atrás da cabeça. Estenda os cotovelos retornando à posição inicial.',
    muscleGroup: 'Triceps brachii',
    category: 'Strength',
    equipment: 'Dumbbell',
    notes: 'Excelente para cabeça longa do tríceps. Permite grande amplitude de movimento. Cuidado para não forçar os cotovelos.',
    videoUrl: 'https://www.youtube.com/watch?v=YbX7Wd8jQ-Q',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/overhead-dumbbell-triceps-extension.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Mergulho em Paralelas',
    description: 'Segure nas barras paralelas com os braços estendidos. Incline levemente o tronco à frente. Desça flexionando os cotovelos até 90 graus. Empurre de volta até a extensão completa.',
    muscleGroup: 'Triceps brachii',
    category: 'Strength',
    equipment: 'Parallel bars',
    notes: 'Exercício composto que trabalha tríceps, peito e ombros. A inclinação do tronco determina o foco: reto = tríceps, inclinado = peito.',
    videoUrl: 'https://www.youtube.com/watch?v=2z8JmcrW-As',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/triceps-dips.gif',
    workoutLocation: 'Gym'
  },

  // ========== ABDÔMEN ==========
  {
    name: 'Abdominal Tradicional',
    description: 'Deite-se de costas com joelhos flexionados. Coloque as mãos atrás da cabeça. Eleve o tronco contraindo o abdômen até as escápulas saírem do chão. Desça controladamente.',
    muscleGroup: 'Rectus abdominis',
    category: 'Strength',
    equipment: 'Gym mat',
    notes: 'Exercício clássico para abdômen. Trabalha principalmente a porção superior do reto abdominal. Evite puxar o pescoço.',
    videoUrl: 'https://www.youtube.com/watch?v=Xyd_fa5zoEU',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/crunch.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Prancha Abdominal',
    description: 'Apoie os antebraços e pontas dos pés no chão. Mantenha o corpo reto da cabeça aos pés. Contraia o abdômen e glúteos. Mantenha a posição respirando normalmente.',
    muscleGroup: 'Rectus abdominis',
    category: 'Strength',
    equipment: 'Gym mat',
    notes: 'Exercício isométrico para core. Trabalha abdômen, lombar e estabilizadores. Essencial para força funcional e postura. Não deixe o quadril cair.',
    videoUrl: 'https://www.youtube.com/watch?v=ASdvN_XEl_c',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/plank.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Elevação de Pernas',
    description: 'Deite-se de costas com as mãos sob os glúteos. Mantenha as pernas estendidas. Eleve as pernas até formarem 90 graus com o tronco. Desça controladamente sem tocar o chão.',
    muscleGroup: 'Rectus abdominis',
    category: 'Strength',
    equipment: 'Gym mat',
    notes: 'Foca na porção inferior do abdômen. Mantenha a lombar colada no chão. Pode flexionar levemente os joelhos se tiver tensão lombar.',
    videoUrl: 'https://www.youtube.com/watch?v=JB2oyawG9KI',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/lying-leg-raise.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Abdominal Bicicleta',
    description: 'Deite-se de costas com mãos atrás da cabeça. Eleve ombros e pernas. Traga o joelho direito em direção ao cotovelo esquerdo, e vice-versa, alternando em movimento de pedalar.',
    muscleGroup: 'Obliquus externus abdominis',
    category: 'Strength',
    equipment: 'Gym mat',
    notes: 'Um dos exercícios mais completos para abdômen. Trabalha reto abdominal e oblíquos. Mantenha movimento controlado, não apressado.',
    videoUrl: 'https://www.youtube.com/watch?v=9FGilxCbdz8',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/bicycle-crunch.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Prancha Lateral',
    description: 'Deite-se de lado apoiado no antebraço. Eleve o quadril formando linha reta do ombro aos pés. Mantenha a posição contraindo o core. Troque de lado.',
    muscleGroup: 'Obliquus externus abdominis',
    category: 'Strength',
    equipment: 'Gym mat',
    notes: 'Trabalha oblíquos e estabilizadores laterais. Importante para equilíbrio e prevenção de lesões. Mantenha o corpo alinhado.',
    videoUrl: 'https://www.youtube.com/watch?v=K2VljzCC16g',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/side-plank.gif',
    workoutLocation: 'Home'
  },

  // ========== CARDIO/FUNCIONAL ==========
  {
    name: 'Burpee',
    description: 'Comece em pé. Agache e apoie as mãos no chão. Jogue as pernas para trás (prancha). Faça uma flexão. Puxe as pernas de volta. Salte verticalmente com as mãos acima da cabeça.',
    muscleGroup: 'Geral',
    category: 'Cardio',
    equipment: 'none (bodyweight exercise)',
    notes: 'Exercício de corpo inteiro. Excelente para condicionamento e queima de calorias. Pode ser modificado removendo a flexão ou o salto para iniciantes.',
    videoUrl: 'https://www.youtube.com/watch?v=auBLPXO8Fww',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/burpee.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Mountain Climbers (Escalador)',
    description: 'Comece em posição de prancha. Leve um joelho em direção ao peito e retorne. Alterne as pernas em movimento rápido, como se estivesse escalando uma montanha.',
    muscleGroup: 'Geral',
    category: 'Cardio',
    equipment: 'none (bodyweight exercise)',
    notes: 'Exercício cardiovascular que trabalha core, ombros e pernas. Ótimo para HIIT. Mantenha o quadril estável durante o movimento.',
    videoUrl: 'https://www.youtube.com/watch?v=nmwgirgXLYM',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/mountain-climber.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Jumping Jacks (Polichinelos)',
    description: 'Comece em pé com pés juntos e braços ao lado. Salte abrindo pernas e elevando braços acima da cabeça simultaneamente. Retorne à posição inicial com outro salto.',
    muscleGroup: 'Geral',
    category: 'Cardio',
    equipment: 'none (bodyweight exercise)',
    notes: 'Exercício cardiovascular clássico. Ótimo para aquecimento. Trabalha coordenação e resistência. Baixo impacto, pode ser feito por todos.',
    videoUrl: 'https://www.youtube.com/watch?v=c4DAnQ6DtF8',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/jumping-jack.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'High Knees (Corrida Estacionária)',
    description: 'Corra no lugar elevando os joelhos até a altura do quadril. Mantenha o tronco ereto e balance os braços naturalmente. Aumente a velocidade para maior intensidade.',
    muscleGroup: 'Geral',
    category: 'Cardio',
    equipment: 'none (bodyweight exercise)',
    notes: 'Exercício cardiovascular de alta intensidade. Trabalha pernas, core e sistema cardiovascular. Perfeito para HIIT e aquecimento.',
    videoUrl: 'https://www.youtube.com/watch?v=8opcQdC-V-U',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/high-knees.gif',
    workoutLocation: 'Home'
  },

  // ========== EXERCÍCIOS CASA (PESO CORPORAL) ==========
  {
    name: 'Agachamento Pistol (Uma Perna)',
    description: 'Fique em uma perna com a outra estendida à frente. Agache descendo controladamente até sentar (quase). Mantenha a perna livre elevada. Empurre pelo calcanhar para subir.',
    muscleGroup: 'Quadriceps femoris',
    category: 'Strength',
    equipment: 'none (bodyweight exercise)',
    notes: 'Variação avançada do agachamento. Requer força, equilíbrio e flexibilidade. Use apoio se necessário. Excelente para corrigir assimetrias.',
    videoUrl: 'https://www.youtube.com/watch?v=vq5-vdgJc0I',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/pistol-squat.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Flexão Pike (Ombros)',
    description: 'Comece em posição de prancha. Eleve o quadril formando um V invertido. Flexione os cotovelos descendo a cabeça em direção ao chão. Empurre de volta à posição inicial.',
    muscleGroup: 'Anterior deltoid',
    category: 'Strength',
    equipment: 'none (bodyweight exercise)',
    notes: 'Exercício para ombros usando peso corporal. Progressão para flexões na parada de mão. Quanto mais elevado o quadril, maior o foco nos ombros.',
    videoUrl: 'https://www.youtube.com/watch?v=spoSDRVt4uQ',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/pike-push-up.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Ponte de Glúteos',
    description: 'Deite-se de costas com joelhos flexionados e pés apoiados. Empurre pelos calcanhares elevando o quadril até formar linha reta dos ombros aos joelhos. Contraia os glúteos no topo.',
    muscleGroup: 'Gluteus maximus',
    category: 'Strength',
    equipment: 'Gym mat',
    notes: 'Exercício de isolamento para glúteos. Também fortalece lombar. Pode ser feito com uma perna para maior dificuldade. Ótimo para ativação glútea.',
    videoUrl: 'https://www.youtube.com/watch?v=wPM8icPu6H8',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/glute-bridge.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Superman',
    description: 'Deite-se de bruços com braços estendidos à frente. Simultaneamente eleve braços, peito e pernas do chão. Mantenha por 2-3 segundos contraindo lombar e glúteos. Retorne controladamente.',
    muscleGroup: 'Erector spinae',
    category: 'Strength',
    equipment: 'Gym mat',
    notes: 'Fortalece lombar, glúteos e posterior de coxa. Importante para postura e prevenção de dores nas costas. Não force excessivamente.',
    videoUrl: 'https://www.youtube.com/watch?v=z6PJMT2y8GQ',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/superman-exercise.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Prancha com Toque no Ombro',
    description: 'Posição de prancha alta (braços estendidos). Mantenha quadril estável e toque o ombro esquerdo com a mão direita. Retorne e alterne. Evite girar o quadril.',
    muscleGroup: 'Rectus abdominis',
    category: 'Strength',
    equipment: 'none (bodyweight exercise)',
    notes: 'Variação dinâmica da prancha. Trabalha core, estabilidade e anti-rotação. Desafiador para equilíbrio. Mantenha movimento controlado.',
    videoUrl: 'https://www.youtube.com/watch?v=h9FMV2VQ9Ts',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/plank-shoulder-tap.gif',
    workoutLocation: 'Home'
  }
];

// Função principal
async function main() {
  console.log('📚 Adicionando exercícios completos ao banco de dados\n');

  try {
    // Login
    const token = await login();
    console.log('✅ Login realizado\n');

    console.log(`📋 ${EXERCISES.length} exercícios para adicionar\n`);

    // Adicionar cada exercício
    let stats = { added: 0, errors: 0 };

    for (let i = 0; i < EXERCISES.length; i++) {
      const exercise = EXERCISES[i];
      try {
        await createExercise(token, exercise);
        stats.added++;
        console.log(`[${i + 1}/${EXERCISES.length}] ✅ Adicionado: ${exercise.name}`);

        // Delay para não sobrecarregar a API
        await new Promise(resolve => setTimeout(resolve, 300));
      } catch (error) {
        stats.errors++;
        console.log(`[${i + 1}/${EXERCISES.length}] ❌ Erro ao adicionar "${exercise.name}": ${error.message}`);
      }
    }

    // Resultados finais
    console.log('\n' + '='.repeat(60));
    console.log('📊 RESULTADOS');
    console.log('='.repeat(60));
    console.log(`✅ Adicionados: ${stats.added}`);
    console.log(`❌ Erros: ${stats.errors}`);
    console.log(`📦 Total: ${EXERCISES.length}`);
    console.log('='.repeat(60));

    console.log('\n🎯 RESUMO POR CATEGORIA:');
    console.log('   - Peito: 4 exercícios');
    console.log('   - Costas: 4 exercícios');
    console.log('   - Pernas: 8 exercícios');
    console.log('   - Ombros: 5 exercícios');
    console.log('   - Bíceps: 4 exercícios');
    console.log('   - Tríceps: 4 exercícios');
    console.log('   - Abdômen: 5 exercícios');
    console.log('   - Cardio/Funcional: 4 exercícios');
    console.log('   - Peso Corporal (Casa): 5 exercícios');

  } catch (error) {
    console.error('\n❌ Erro fatal:', error.message);
    process.exit(1);
  }
}

// Executar
main();
