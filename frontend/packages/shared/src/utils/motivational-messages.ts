/**
 * Motivational messages for workout achievements
 */

export const setCompletionMessages = [
  {
    title: '💪 Série Completa!',
    description: 'Mais forte a cada repetição!',
  },
  {
    title: '🔥 Arrasou!',
    description: 'Continue assim, você está no caminho certo!',
  },
  {
    title: '⚡ Energia Pura!',
    description: 'Cada série te aproxima do seu objetivo!',
  },
  {
    title: '🚀 Imparável!',
    description: 'Sua dedicação é inspiradora!',
  },
  {
    title: '💎 Série de Ouro!',
    description: 'Excelente execução!',
  },
  {
    title: '🏆 Mandou Bem!',
    description: 'Transformação acontece série por série!',
  },
  {
    title: '💥 Que Poder!',
    description: 'Você está evoluindo a cada treino!',
  },
  {
    title: '⭐ Estrela do Treino!',
    description: 'Foco e determinação definem campeões!',
  },
  {
    title: '🎯 No Alvo!',
    description: 'Precisão e força em perfeita harmonia!',
  },
  {
    title: '🌟 Brilhou!',
    description: 'Seu esforço não passa despercebido!',
  },
  {
    title: '🦾 Força Total!',
    description: 'Cada repetição é uma vitória!',
  },
  {
    title: '🔋 Bateria Carregada!',
    description: 'Sua energia está em outro nível!',
  },
  {
    title: '🎪 Show de Performance!',
    description: 'Técnica impecável, resultado garantido!',
  },
  {
    title: '🌈 Superação!',
    description: 'Limites existem para serem quebrados!',
  },
  {
    title: '🏅 Medalha Merecida!',
    description: 'Consistência é o segredo do sucesso!',
  },
];

export const workoutCompletionMessages = [
  {
    title: '🎉 Treino Concluído!',
    description: 'Parabéns! Você está mais perto dos seus objetivos!',
  },
  {
    title: '🏆 Missão Cumprida!',
    description: 'Treino de campeão! Continue nesse ritmo!',
  },
  {
    title: '💪 Guerreiro do Fitness!',
    description: 'Mais um dia de vitória sobre você mesmo!',
  },
  {
    title: '🔥 Treino Épico!',
    description: 'Você deu tudo de si hoje! Orgulhe-se!',
  },
  {
    title: '⚡ Energia Total!',
    description: 'Treino finalizado com sucesso! Você é imparável!',
  },
  {
    title: '🚀 Rumo ao Topo!',
    description: 'Cada treino te leva mais longe. Incrível!',
  },
  {
    title: '💎 Treino Premium!',
    description: 'Qualidade, foco e dedicação. Você é inspiração!',
  },
  {
    title: '⭐ Estrela do Dia!',
    description: 'Brilhou no treino! Resultado está chegando!',
  },
  {
    title: '🎯 Objetivo Atingido!',
    description: 'Mais um treino completo! Seu corpo agradece!',
  },
  {
    title: '🌟 Performance Excepcional!',
    description: 'Treino concluído! Você está evoluindo muito!',
  },
  {
    title: '🦁 Força de Leão!',
    description: 'Treino brutal! Você superou seus limites!',
  },
  {
    title: '🏅 Campeão do Dia!',
    description: 'Treino finalizado! A vitória é sua!',
  },
  {
    title: '💥 Explosão de Energia!',
    description: 'Treino completo! Você está on fire!',
  },
  {
    title: '🎪 Apresentação Perfeita!',
    description: 'Treino impecável! Continue assim!',
  },
  {
    title: '🌈 Transformação em Ação!',
    description: 'Mais um treino, mais uma evolução!',
  },
  {
    title: '👑 Realeza do Treino!',
    description: 'Treino de rei! Você merece todo o sucesso!',
  },
  {
    title: '🔱 Poder Absoluto!',
    description: 'Treino dominado! Nada te para!',
  },
  {
    title: '🎖️ Medalha de Honra!',
    description: 'Treino conquistado com garra e determinação!',
  },
];

/**
 * Get a random motivational message for set completion
 */
export function getRandomSetMessage() {
  const randomIndex = Math.floor(Math.random() * setCompletionMessages.length);
  return setCompletionMessages[randomIndex];
}

/**
 * Get a random motivational message for workout completion
 */
export function getRandomWorkoutMessage() {
  const randomIndex = Math.floor(Math.random() * workoutCompletionMessages.length);
  return workoutCompletionMessages[randomIndex];
}

/**
 * Get a milestone message based on the number of completed sets
 */
export function getMilestoneMessage(setCount: number) {
  const milestones: Record<number, { title: string; description: string }> = {
    5: {
      title: '🎯 5 Séries!',
      description: 'Você está pegando fogo! Continue assim!',
    },
    10: {
      title: '🔥 10 Séries!',
      description: 'Impressionante! Você está voando!',
    },
    15: {
      title: '💪 15 Séries!',
      description: 'Que máquina! Você não para!',
    },
    20: {
      title: '🚀 20 Séries!',
      description: 'Lendário! Você está em outro nível!',
    },
    25: {
      title: '🏆 25 Séries!',
      description: 'Campeão absoluto! Nada te para!',
    },
    30: {
      title: '👑 30 Séries!',
      description: 'REI/RAINHA DO TREINO! Você é uma lenda!',
    },
  };

  return milestones[setCount];
}
