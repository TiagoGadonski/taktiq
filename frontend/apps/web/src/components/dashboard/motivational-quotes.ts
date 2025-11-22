// Motivational quotes for students
export const motivationalQuotes = [
  {
    text: "A única coisa impossível é aquilo que você não tenta.",
    author: "Desconhecido"
  },
  {
    text: "O corpo alcança o que a mente acredita.",
    author: "Desconhecido"
  },
  {
    text: "Não pare quando estiver cansado. Pare quando tiver terminado.",
    author: "Desconhecido"
  },
  {
    text: "Sua única limitação é você mesmo.",
    author: "Desconhecido"
  },
  {
    text: "Sucesso é a soma de pequenos esforços repetidos dia após dia.",
    author: "Robert Collier"
  },
  {
    text: "Quanto mais você sua no treino, menos você sangra na batalha.",
    author: "Desconhecido"
  },
  {
    text: "Não conte os dias. Faça os dias contarem.",
    author: "Muhammad Ali"
  },
  {
    text: "O corpo é a escultura da alma.",
    author: "Aristóteles"
  },
  {
    text: "Acredite em você mesmo e tudo será possível.",
    author: "Desconhecido"
  },
  {
    text: "Transforme suor em força, dor em poder.",
    author: "Desconhecido"
  }
];

export function getRandomQuote() {
  return motivationalQuotes[Math.floor(Math.random() * motivationalQuotes.length)];
}

export function getDailyQuote() {
  // Same quote for the entire day (based on day of year)
  const now = new Date();
  const start = new Date(now.getFullYear(), 0, 0);
  const diff = now.getTime() - start.getTime();
  const oneDay = 1000 * 60 * 60 * 24;
  const dayOfYear = Math.floor(diff / oneDay);

  return motivationalQuotes[dayOfYear % motivationalQuotes.length];
}
