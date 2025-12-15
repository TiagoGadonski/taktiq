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

// Lista expandida de exercícios
const EXERCISES = [
  // ========== KETTLEBELL ==========
  {
    name: 'Kettlebell Swing',
    description: 'Fique em pé com pés afastados, segurando o kettlebell com ambas as mãos. Flexione o quadril e balance o kettlebell entre as pernas. Empurre os quadris para frente explosivamente, balançando o kettlebell até a altura dos ombros. O movimento vem dos quadris, não dos braços.',
    muscleGroup: 'Gluteus maximus',
    category: 'Strength',
    equipment: 'Kettlebell',
    notes: 'Exercício balístico que trabalha posterior de coxa, glúteos e core. Excelente para potência e condicionamento. Mantenha as costas retas e use os quadris como motor do movimento.',
    videoUrl: 'https://www.youtube.com/watch?v=YSxHifyI6s8',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/09/two-arm-kettlebell-swing.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Kettlebell Goblet Squat',
    description: 'Segure o kettlebell na altura do peito com ambas as mãos, cotovelos apontando para baixo. Agache mantendo o peito elevado e o peso nos calcanhares. Desça até as coxas paralelas. Os cotovelos devem passar entre os joelhos.',
    muscleGroup: 'Quadriceps femoris',
    category: 'Strength',
    equipment: 'Kettlebell',
    notes: 'Excelente para aprender mecânica correta do agachamento. O peso na frente ajuda a manter postura ereta. Trabalha pernas e core simultaneamente.',
    videoUrl: 'https://www.youtube.com/watch?v=MeIiIdhvXT4',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/09/goblet-squat.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Kettlebell Turkish Get-Up',
    description: 'Deite-se com kettlebell estendido acima. Em uma sequência controlada, role para o cotovelo, depois para a mão, eleve o quadril, passe a perna por baixo e fique em pé. Reverta o movimento para deitar novamente.',
    muscleGroup: 'Geral',
    category: 'Strength',
    equipment: 'Kettlebell',
    notes: 'Exercício complexo de corpo inteiro. Desenvolve força, estabilidade e mobilidade. Aprenda o movimento sem peso primeiro. Excelente para atletas e treinamento funcional.',
    videoUrl: 'https://www.youtube.com/watch?v=0bWRPC49-KI',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/09/kettlebell-turkish-get-up.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Kettlebell Snatch',
    description: 'Comece com kettlebell entre os pés. Em um movimento explosivo, puxe o kettlebell acima da cabeça estendendo quadris e joelhos. Deixe-o girar ao redor do punho no topo. Desça controladamente.',
    muscleGroup: 'Geral',
    category: 'Strength',
    equipment: 'Kettlebell',
    notes: 'Movimento olímpico com kettlebell. Desenvolve potência, força e resistência. Técnica é crucial - pratique com peso leve primeiro.',
    videoUrl: 'https://www.youtube.com/watch?v=wZdhHbzYHkg',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/09/kettlebell-snatch.gif',
    workoutLocation: 'Gym'
  },

  // ========== ELÁSTICOS/BANDAS ==========
  {
    name: 'Remada com Elástico',
    description: 'Fixe o elástico à frente na altura do peito. Segure as pontas e afaste-se até criar tensão. Puxe os cotovelos para trás, contraindo as escápulas. Mantenha o tronco estável e retorne controladamente.',
    muscleGroup: 'Latissimus dorsi',
    category: 'Strength',
    equipment: 'Resistance band',
    notes: 'Exercício portátil para costas. Ótimo para treinar em casa ou viagem. A resistência aumenta conforme você puxa. Ajustável simplesmente mudando a distância.',
    videoUrl: 'https://www.youtube.com/watch?v=eVq55ilDu5c',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/03/resistance-band-row.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Agachamento com Elástico',
    description: 'Pise no meio do elástico com ambos os pés. Segure as pontas nos ombros. Agache mantendo tensão constante no elástico. A resistência aumenta conforme você sobe.',
    muscleGroup: 'Quadriceps femoris',
    category: 'Strength',
    equipment: 'Resistance band',
    notes: 'Variação do agachamento com resistência progressiva. Ideal para casa. Leve e portátil. Pode usar bandas de diferentes resistências.',
    videoUrl: 'https://www.youtube.com/watch?v=THE4i80LLvs',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/03/resistance-band-squat.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Afastamento Lateral com Mini Band',
    description: 'Coloque a mini band acima dos joelhos ou tornozelos. Fique em semi-agachamento. Dê passos laterais mantendo tensão constante na banda. Glúteos devem estar sempre contraídos.',
    muscleGroup: 'Gluteus medius',
    category: 'Strength',
    equipment: 'Resistance band',
    notes: 'Excelente para ativação de glúteo médio. Previne lesões de joelho. Importante para corredores e atletas. Ajuda a corrigir valgo de joelho.',
    videoUrl: 'https://www.youtube.com/watch?v=Nsa29JaJaH8',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/03/lateral-band-walk.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Puxada com Elástico',
    description: 'Fixe o elástico acima da cabeça. Ajoelhe-se ou fique em pé. Puxe o elástico até o peito levando os cotovelos para baixo e para trás. Contraia as escápulas. Retorne controladamente.',
    muscleGroup: 'Latissimus dorsi',
    category: 'Strength',
    equipment: 'Resistance band',
    notes: 'Simula puxada na máquina. Perfeito para casa. Ajustável pela espessura do elástico. Trabalha largura das costas.',
    videoUrl: 'https://www.youtube.com/watch?v=VKhCdso4FKs',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/03/resistance-band-lat-pulldown.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Rosca Bíceps com Elástico',
    description: 'Pise no elástico com ambos os pés. Segure as pontas com as mãos. Flexione os cotovelos levando as mãos em direção aos ombros. Mantenha os cotovelos fixos ao lado do corpo.',
    muscleGroup: 'Biceps brachii',
    category: 'Strength',
    equipment: 'Resistance band',
    notes: 'Portátil e eficaz para bíceps. A resistência progressiva cria tensão máxima no pico da contração. Ideal para viagens.',
    videoUrl: 'https://www.youtube.com/watch?v=OBhzBFMIEk0',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/03/resistance-band-bicep-curl.gif',
    workoutLocation: 'Home'
  },

  // ========== TRX/SUSPENSÃO ==========
  {
    name: 'TRX Remada',
    description: 'Segure as alças do TRX inclinando o corpo para trás. Mantenha o corpo reto. Puxe o corpo para cima levando os cotovelos para trás. Contraia as escápulas. Desça controladamente.',
    muscleGroup: 'Latissimus dorsi',
    category: 'Strength',
    equipment: 'TRX',
    notes: 'Exercício ajustável pela inclinação do corpo. Quanto mais horizontal, mais difícil. Trabalha costas e core simultaneamente.',
    videoUrl: 'https://www.youtube.com/watch?v=tCDLV8IMxas',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/04/trx-row.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'TRX Flexão',
    description: 'Coloque os pés nas alças do TRX. Posição de prancha com as mãos no chão. Faça flexões mantendo o corpo estável. O TRX adiciona instabilidade, aumentando trabalho do core.',
    muscleGroup: 'Pectoralis major',
    category: 'Strength',
    equipment: 'TRX',
    notes: 'Versão avançada da flexão. A instabilidade recruta mais músculos estabilizadores. Excelente para core e controle corporal.',
    videoUrl: 'https://www.youtube.com/watch?v=OYvGw8Gw7F8',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/04/trx-push-up.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'TRX Agachamento Pistol Assistido',
    description: 'Segure as alças do TRX. Levante uma perna à frente. Agache na perna de apoio usando o TRX para equilíbrio. Desça controladamente e empurre de volta usando principalmente a perna.',
    muscleGroup: 'Quadriceps femoris',
    category: 'Strength',
    equipment: 'TRX',
    notes: 'Progressão para agachamento pistol. O TRX ajuda no equilíbrio permitindo foco na força. Ótimo para corrigir assimetrias.',
    videoUrl: 'https://www.youtube.com/watch?v=jDwBGJYb-3E',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/04/trx-pistol-squat.gif',
    workoutLocation: 'Gym'
  },

  // ========== MOBILIDADE E ALONGAMENTO ==========
  {
    name: 'Cat-Cow (Gato-Vaca)',
    description: 'Posição de quatro apoios. Inspire arqueando as costas para baixo (vaca). Expire arredondando as costas para cima (gato). Movimento fluido e controlado sincronizado com a respiração.',
    muscleGroup: 'Erector spinae',
    category: 'Flexibility',
    equipment: 'Gym mat',
    notes: 'Excelente para mobilidade da coluna. Aquece a lombar e alivia tensão. Ótimo pela manhã ou antes de treinar. Ajuda com dores nas costas.',
    videoUrl: 'https://www.youtube.com/watch?v=kqnua4rHVVA',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/05/cat-cow-stretch.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Alongamento do Piriforme',
    description: 'Deite-se de costas. Cruze uma perna sobre a outra formando um "4". Puxe a perna de baixo em direção ao peito. Você sentirá alongamento profundo no glúteo da perna cruzada.',
    muscleGroup: 'Gluteus maximus',
    category: 'Flexibility',
    equipment: 'Gym mat',
    notes: 'Alivia tensão no piriforme e glúteos. Excelente para quem fica muito sentado. Ajuda com ciática. Mantenha 30-60 segundos cada lado.',
    videoUrl: 'https://www.youtube.com/watch?v=4BOTvaRaDjI',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/05/piriformis-stretch.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Mobilidade de Quadril 90/90',
    description: 'Sente-se com ambas as pernas formando 90 graus (uma à frente, outra ao lado). Incline o tronco sobre a perna da frente. Troque de lado. Trabalha rotação interna e externa do quadril.',
    muscleGroup: 'Gluteus maximus',
    category: 'Flexibility',
    equipment: 'Gym mat',
    notes: 'Essencial para mobilidade de quadril. Previne lesões. Importante para agachamento profundo. Ajuda com dores no quadril e lombar.',
    videoUrl: 'https://www.youtube.com/watch?v=FQqOi2-xEd4',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/05/90-90-hip-stretch.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Alongamento de Isquiotibiais Deitado',
    description: 'Deite-se de costas. Levante uma perna estendida. Segure atrás da coxa e puxe suavemente em direção ao peito. Mantenha a perna oposta no chão. Sinta alongamento na parte de trás da coxa.',
    muscleGroup: 'Biceps femoris',
    category: 'Flexibility',
    equipment: 'Gym mat',
    notes: 'Alongamento seguro para posteriores. Use cinto/toalha se necessário. Mantenha 30-60 segundos. Ajuda com flexibilidade e previne lesões.',
    videoUrl: 'https://www.youtube.com/watch?v=OHNQl40luUo',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/05/lying-hamstring-stretch.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Círculos de Braço (Mobilidade Ombro)',
    description: 'Fique em pé com braços estendidos lateralmente. Faça círculos pequenos, depois médios, depois grandes. Inverta a direção. Mantenha movimento controlado e fluido.',
    muscleGroup: 'Anterior deltoid',
    category: 'Flexibility',
    equipment: 'none (bodyweight exercise)',
    notes: 'Aquece e mobiliza os ombros. Ótimo antes de treino de empurrar ou puxar. Aumenta amplitude gradualmente. Previne lesões no ombro.',
    videoUrl: 'https://www.youtube.com/watch?v=5BuufDC3qT4',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/05/arm-circles.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Thread the Needle (Rotação Torácica)',
    description: 'Posição de quatro apoios. Passe um braço por baixo do outro, rotacionando o tronco. Volte e estenda o braço para cima abrindo o peito. Alterna os lados. Movimento controlado.',
    muscleGroup: 'Erector spinae',
    category: 'Flexibility',
    equipment: 'Gym mat',
    notes: 'Mobilidade da coluna torácica. Essencial para saúde postural. Alivia tensão nas costas médias. Ótimo para quem trabalha sentado.',
    videoUrl: 'https://www.youtube.com/watch?v=fMV8pGQEJ_s',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/05/thread-the-needle.gif',
    workoutLocation: 'Home'
  },

  // ========== TERAPÊUTICOS - DOR LOMBAR ==========
  {
    name: 'Bird Dog (Exercício para Lombar)',
    description: 'Posição de quatro apoios. Estenda braço direito e perna esquerda simultaneamente. Mantenha 3-5 segundos. Retorne e alterne. Mantenha core contraído e movimento lento.',
    muscleGroup: 'Erector spinae',
    category: 'Strength',
    equipment: 'Gym mat',
    notes: 'TERAPÊUTICO: Fortalece lombar e core. Recomendado para reabilitação de dor lombar. Melhora estabilidade e equilíbrio. Progride lentamente. Consulte profissional se dor persistir.',
    videoUrl: 'https://www.youtube.com/watch?v=wiFNA3sqjCA',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/06/bird-dog.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Ponte Glútea Terapêutica',
    description: 'Deite-se com joelhos flexionados. Eleve quadris contraindo glúteos. Mantenha 5 segundos no topo. Desça lentamente. Mantenha lombar neutra, sem arquear excessivamente.',
    muscleGroup: 'Gluteus maximus',
    category: 'Strength',
    equipment: 'Gym mat',
    notes: 'TERAPÊUTICO: Ativa glúteos reduzindo sobrecarga lombar. Recomendado para dor lombar crônica. Fortalece cadeia posterior. Pode fazer diariamente.',
    videoUrl: 'https://www.youtube.com/watch?v=wPM8icPu6H8',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2022/02/glute-bridge.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Dead Bug (Inseto Morto)',
    description: 'Deite-se com braços estendidos acima e joelhos a 90 graus. Estenda braço e perna oposta mantendo lombar no chão. Retorne e alterne. Movimento lento e controlado.',
    muscleGroup: 'Rectus abdominis',
    category: 'Strength',
    equipment: 'Gym mat',
    notes: 'TERAPÊUTICO: Fortalece core profundo sem estresse lombar. Excelente para estabilidade. Ensina manutenção de lombar neutra. Seguro para dor lombar.',
    videoUrl: 'https://www.youtube.com/watch?v=g_BYB0R-4Ws',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/06/dead-bug.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Joelhos ao Peito (Alongamento Lombar)',
    description: 'Deite-se de costas. Puxe ambos os joelhos em direção ao peito. Abrace as pernas. Mantenha 30-60 segundos. Respire profundamente. Gentilmente balance lado a lado se confortável.',
    muscleGroup: 'Erector spinae',
    category: 'Flexibility',
    equipment: 'Gym mat',
    notes: 'TERAPÊUTICO: Alivia tensão lombar. Recomendado após ficar muito tempo sentado. Seguro e suave. Pode fazer múltiplas vezes ao dia. Ajuda com espasmos lombares.',
    videoUrl: 'https://www.youtube.com/watch?v=KV_wbHaw0EQ',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/06/knees-to-chest.gif',
    workoutLocation: 'Home'
  },

  // ========== TERAPÊUTICOS - DOR NO JOELHO ==========
  {
    name: 'Elevação de Perna Estendida (Quadríceps)',
    description: 'Deite-se com uma perna flexionada. Mantenha a outra estendida, contraia o quadríceps e eleve 15-20cm. Mantenha 3-5 segundos. Desça lentamente. 10-15 repetições.',
    muscleGroup: 'Quadriceps femoris',
    category: 'Strength',
    equipment: 'Gym mat',
    notes: 'TERAPÊUTICO: Fortalece quadríceps sem estresse no joelho. Recomendado para condromalácia patelar. Baixo impacto. Essencial na reabilitação de joelho.',
    videoUrl: 'https://www.youtube.com/watch?v=6nRNswPw5M0',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/06/straight-leg-raise.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Mini Agachamento na Parede',
    description: 'Encoste as costas na parede. Desça até joelhos a 45 graus (não 90!). Mantenha 10-30 segundos. Suba lentamente. Não force se sentir dor no joelho.',
    muscleGroup: 'Quadriceps femoris',
    category: 'Strength',
    equipment: 'Wall',
    notes: 'TERAPÊUTICO: Fortalece quadríceps com amplitude segura. IMPORTANTE: NÃO desça muito - 45 graus é suficiente. Ótimo para condromalácia e instabilidade patelar.',
    videoUrl: 'https://www.youtube.com/watch?v=y-wV4Venusw',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/06/quarter-squat.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Concha (Clamshell)',
    description: 'Deite-se de lado com joelhos flexionados. Mantenha pés juntos e abra o joelho de cima como uma concha. Mantenha quadris empilhados. 15-20 repetições cada lado.',
    muscleGroup: 'Gluteus medius',
    category: 'Strength',
    equipment: 'Gym mat',
    notes: 'TERAPÊUTICO: Fortalece glúteo médio, crucial para estabilidade do joelho. Previne valgo de joelho. Recomendado para síndrome patelofemoral e prevenção de lesões.',
    videoUrl: 'https://www.youtube.com/watch?v=bEJhXfJLLLs',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/06/clamshell.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Alongamento de Quadríceps em Pé',
    description: 'Fique em pé (use apoio se necessário). Flexione um joelho trazendo calcanhar ao glúteo. Segure o tornozelo. Mantenha joelhos juntos. 30-60 segundos cada perna.',
    muscleGroup: 'Quadriceps femoris',
    category: 'Flexibility',
    equipment: 'none (bodyweight exercise)',
    notes: 'TERAPÊUTICO: Alonga quadríceps reduzindo tensão na patela. Importante para síndrome patelofemoral. Não force - deve sentir alongamento suave.',
    videoUrl: 'https://www.youtube.com/watch?v=nKB1YW4ezsw',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/06/standing-quad-stretch.gif',
    workoutLocation: 'Home'
  },

  // ========== TERAPÊUTICOS - DOR NO OMBRO ==========
  {
    name: 'Rotação Externa com Elástico',
    description: 'Cotovelo a 90 graus junto ao corpo. Segure elástico com resistência leve. Rotacione o antebraço para fora mantendo cotovelo fixo. Movimento lento e controlado. 15-20 repetições.',
    muscleGroup: 'Rotator cuff',
    category: 'Strength',
    equipment: 'Resistance band',
    notes: 'TERAPÊUTICO: Fortalece manguito rotador. Essencial para prevenção e reabilitação de lesões no ombro. Use resistência LEVE. Foco na técnica, não no peso.',
    videoUrl: 'https://www.youtube.com/watch?v=YJT0Qb9i0TI',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/06/external-rotation.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Deslizamento na Parede (Wall Slide)',
    description: 'Encoste costas e braços na parede. Deslize os braços para cima mantendo contato com a parede. Suba o máximo que conseguir sem dor. Desça lentamente. 10-15 repetições.',
    muscleGroup: 'Anterior deltoid',
    category: 'Flexibility',
    equipment: 'Wall',
    notes: 'TERAPÊUTICO: Melhora mobilidade e postura do ombro. Fortalece estabilizadores da escápula. Excelente para síndrome do impacto. Pare se sentir dor aguda.',
    videoUrl: 'https://www.youtube.com/watch?v=FYwj0SUqROI',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/06/wall-slide.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Pêndulo de Codman',
    description: 'Incline-se apoiando em uma mesa. Deixe o braço afetado pendurado. Balance suavemente em círculos pequenos. Não use músculo - deixe a gravidade fazer o trabalho. 1-2 minutos.',
    muscleGroup: 'Anterior deltoid',
    category: 'Flexibility',
    equipment: 'Table',
    notes: 'TERAPÊUTICO: Alivia dor e rigidez no ombro. Recomendado pós-lesão ou cirurgia. Movimento passivo - não force. Aumenta circulação sem estresse articular.',
    videoUrl: 'https://www.youtube.com/watch?v=KLtNve-cOz0',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/06/pendulum-exercise.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Y-T-W (Fortalecimento Escapular)',
    description: 'Deite-se de bruços. Forme Y, T e W com os braços elevando-os do chão. Contraia escápulas. Mantenha 3-5 segundos cada posição. Polegares para cima. Baixo peso ou sem peso.',
    muscleGroup: 'Trapezius',
    category: 'Strength',
    equipment: 'Gym mat',
    notes: 'TERAPÊUTICO: Fortalece estabilizadores da escápula. Previne lesões no ombro. Corrige postura arredondada. Essencial para saúde do ombro a longo prazo.',
    videoUrl: 'https://www.youtube.com/watch?v=L-Yd5UxaSmI',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/06/ytw-exercise.gif',
    workoutLocation: 'Home'
  },

  // ========== TERAPÊUTICOS - DOR CERVICAL/POSTURA ==========
  {
    name: 'Retração Cervical (Queixo para Dentro)',
    description: 'Sentado ou em pé, olhe para frente. Puxe o queixo para trás criando "duplo queixo". Não incline a cabeça - movimento horizontal. Mantenha 5-10 segundos. 10 repetições.',
    muscleGroup: 'Cervical',
    category: 'Flexibility',
    equipment: 'none (bodyweight exercise)',
    notes: 'TERAPÊUTICO: Corrige postura de cabeça para frente. Alivia tensão cervical. Fortalece flexores cervicais profundos. Essencial para quem trabalha em computador.',
    videoUrl: 'https://www.youtube.com/watch?v=2s59sLmxXxY',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/06/chin-tuck.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Alongamento Trapézio Superior',
    description: 'Sentado, incline a cabeça lateralmente aproximando orelha do ombro. Com a mão do mesmo lado, aplique leve pressão. Mantenha 30 segundos. Sinta alongamento no trapézio oposto.',
    muscleGroup: 'Trapezius',
    category: 'Flexibility',
    equipment: 'Chair',
    notes: 'TERAPÊUTICO: Alivia tensão no trapézio e pescoço. Comum em quem trabalha sentado. Alongamento suave - não force. Respire profundamente durante.',
    videoUrl: 'https://www.youtube.com/watch?v=pBP6dW_VYe4',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/06/upper-trap-stretch.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Rotação Cervical Controlada',
    description: 'Sentado com postura ereta. Gire lentamente a cabeça para um lado. Mantenha 5 segundos. Retorne ao centro. Alterne os lados. Movimento lento sem dor. 10 repetições cada lado.',
    muscleGroup: 'Cervical',
    category: 'Flexibility',
    equipment: 'Chair',
    notes: 'TERAPÊUTICO: Mantém mobilidade cervical. Previne rigidez. Importante fazer diariamente. Pare se sentir tontura ou dor. Movimento deve ser suave.',
    videoUrl: 'https://www.youtube.com/watch?v=T7PoMHP-zVY',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/06/neck-rotation.gif',
    workoutLocation: 'Home'
  },
  {
    name: 'Anjo na Parede (Postura)',
    description: 'Encoste cabeça, costas e glúteos na parede. Braços a 90 graus na parede. Deslize braços para cima e para baixo mantendo contato. Aperta escápulas juntas. 10-15 repetições.',
    muscleGroup: 'Trapezius',
    category: 'Strength',
    equipment: 'Wall',
    notes: 'TERAPÊUTICO: Corrige postura arredondada. Fortalece músculos posturais. Abre peito. Excelente para síndrome cruzada superior. Faça diariamente.',
    videoUrl: 'https://www.youtube.com/watch?v=7VRQfr33h-U',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/06/wall-angel.gif',
    workoutLocation: 'Home'
  },

  // ========== VARIAÇÕES AVANÇADAS ==========
  {
    name: 'Muscle-Up',
    description: 'Comece pendurado na barra. Com impulso explosivo, puxe até o peito passar a barra e empurre para cima finalizando em mergulho. Movimento requer força, técnica e timing.',
    muscleGroup: 'Latissimus dorsi',
    category: 'Strength',
    equipment: 'Pull-up bar',
    notes: 'AVANÇADO: Requer domínio de barra fixa e mergulho. Desenvolve força explosiva. Pratique transição separadamente. Use elástico para assistência inicial.',
    videoUrl: 'https://www.youtube.com/watch?v=tB3X4TjTIes',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/07/muscle-up.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Dragon Flag',
    description: 'Deite-se em banco segurando atrás da cabeça. Eleve corpo inteiro mantendo-o reto, apoiado apenas nos ombros. Desça controladamente. Core extremamente contraído durante todo movimento.',
    muscleGroup: 'Rectus abdominis',
    category: 'Strength',
    equipment: 'Bench',
    notes: 'AVANÇADO: Exercício de core de altíssima dificuldade. Requer força abdominal excepcional. Progressão: joelhos flexionados → uma perna → ambas estendidas.',
    videoUrl: 'https://www.youtube.com/watch?v=moyFIvRrS0s',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/07/dragon-flag.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Flexão Arqueira',
    description: 'Posição de flexão com mãos bem afastadas. Desça inclinando para um lado, estendendo o braço oposto. Empurre de volta e alterne. Um braço faz maior parte do trabalho.',
    muscleGroup: 'Pectoralis major',
    category: 'Strength',
    equipment: 'none (bodyweight exercise)',
    notes: 'AVANÇADO: Progressão para flexão de um braço. Desenvolve força unilateral. Mantenha corpo alinhado. Controle o movimento lateral.',
    videoUrl: 'https://www.youtube.com/watch?v=RVD_ils9936',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/07/archer-push-up.gif',
    workoutLocation: 'Home'
  },

  // ========== FOAM ROLLER (LIBERAÇÃO MIOFASCIAL) ==========
  {
    name: 'Foam Roller IT Band',
    description: 'Deite-se de lado com foam roller sob a lateral da coxa. Role lentamente da lateral do quadril até o joelho. Pause em pontos sensíveis 20-30 segundos. Evite rolar diretamente sobre o joelho.',
    muscleGroup: 'Tensor fasciae latae',
    category: 'Flexibility',
    equipment: 'Foam roller',
    notes: 'RECUPERAÇÃO: Libera tensão na banda iliotibial. Pode ser desconfortável - respire profundamente. Importante para corredores. Previne dor no joelho.',
    videoUrl: 'https://www.youtube.com/watch?v=5EDHJASaBbo',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/08/foam-roll-it-band.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Foam Roller Lombar',
    description: 'Deite-se com foam roller na lombar. Mãos atrás da cabeça. Role suavemente para cima e para baixo. Mantenha joelhos flexionados. Movimento lento e controlado.',
    muscleGroup: 'Erector spinae',
    category: 'Flexibility',
    equipment: 'Foam roller',
    notes: 'RECUPERAÇÃO: Libera tensão lombar. CUIDADO: Use pressão leve. Não role diretamente sobre ossos da coluna. Se dor aguda, consulte profissional.',
    videoUrl: 'https://www.youtube.com/watch?v=yTBFLdgRMI4',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/08/foam-roll-lower-back.gif',
    workoutLocation: 'Gym'
  },
  {
    name: 'Foam Roller Panturrilha',
    description: 'Sente-se com foam roller sob a panturrilha. Levante quadris do chão. Role da parte de trás do joelho até o tornozelo. Pode cruzar a perna oposta para mais pressão.',
    muscleGroup: 'Gastrocnemius',
    category: 'Flexibility',
    equipment: 'Foam roller',
    notes: 'RECUPERAÇÃO: Libera tensão nas panturrilhas. Importante após corrida ou treino de pernas. Ajuda a prevenir fascite plantar. Pause em pontos tensos.',
    videoUrl: 'https://www.youtube.com/watch?v=mnCT7FedTYI',
    imageUrl: 'https://www.inspireusafoundation.org/wp-content/uploads/2023/08/foam-roll-calf.gif',
    workoutLocation: 'Gym'
  }
];

// Função principal
async function main() {
  console.log('🚀 Adicionando exercícios avançados e terapêuticos\n');

  try {
    // Login
    const token = await login();
    console.log('✅ Login realizado\n');

    console.log(`📋 ${EXERCISES.length} exercícios para adicionar\n`);

    // Adicionar cada exercício
    let stats = { added: 0, errors: 0 };
    let categories = {
      kettlebell: 0,
      bands: 0,
      trx: 0,
      mobility: 0,
      therapeutic: 0,
      advanced: 0,
      foam: 0
    };

    for (let i = 0; i < EXERCISES.length; i++) {
      const exercise = EXERCISES[i];
      try {
        await createExercise(token, exercise);
        stats.added++;

        // Categorizar
        if (exercise.equipment === 'Kettlebell') categories.kettlebell++;
        else if (exercise.equipment === 'Resistance band') categories.bands++;
        else if (exercise.equipment === 'TRX') categories.trx++;
        else if (exercise.category === 'Flexibility') categories.mobility++;
        else if (exercise.notes.includes('TERAPÊUTICO')) categories.therapeutic++;
        else if (exercise.notes.includes('AVANÇADO')) categories.advanced++;
        else if (exercise.equipment === 'Foam roller') categories.foam++;

        console.log(`[${i + 1}/${EXERCISES.length}] ✅ ${exercise.name}`);

        // Delay
        await new Promise(resolve => setTimeout(resolve, 300));
      } catch (error) {
        stats.errors++;
        console.log(`[${i + 1}/${EXERCISES.length}] ❌ Erro: ${exercise.name}`);
      }
    }

    // Resultados finais
    console.log('\n' + '='.repeat(70));
    console.log('📊 RESULTADOS');
    console.log('='.repeat(70));
    console.log(`✅ Adicionados: ${stats.added}`);
    console.log(`❌ Erros: ${stats.errors}`);
    console.log(`📦 Total: ${EXERCISES.length}`);
    console.log('='.repeat(70));

    console.log('\n🎯 DISTRIBUIÇÃO POR CATEGORIA:');
    console.log(`   💪 Kettlebell: ${categories.kettlebell} exercícios`);
    console.log(`   🎗️  Elásticos/Bandas: ${categories.bands} exercícios`);
    console.log(`   🔗 TRX/Suspensão: ${categories.trx} exercícios`);
    console.log(`   🧘 Mobilidade/Alongamento: ${categories.mobility} exercícios`);
    console.log(`   ⚕️  Terapêuticos (Reabilitação): ${categories.therapeutic} exercícios`);
    console.log(`   🔥 Avançados: ${categories.advanced} exercícios`);
    console.log(`   🎲 Foam Roller: ${categories.foam} exercícios`);

    console.log('\n📝 CATEGORIAS TERAPÊUTICAS:');
    console.log('   ✓ Dor Lombar: 4 exercícios');
    console.log('   ✓ Dor no Joelho: 4 exercícios');
    console.log('   ✓ Dor no Ombro: 4 exercícios');
    console.log('   ✓ Dor Cervical/Postura: 4 exercícios');

  } catch (error) {
    console.error('\n❌ Erro fatal:', error.message);
    process.exit(1);
  }
}

// Executar
main();
