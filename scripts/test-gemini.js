const https = require('https');

const GEMINI_API_KEY = 'AIzaSyDCIiSPd5IiaNLO_0OZ-LVcDGlN1OWqoNk';

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
        console.log('Status Code:', res.statusCode);
        console.log('Response Body:', body);
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

async function test() {
  const prompt = `Você é um especialista em educação física. Com base nas informações abaixo, sugira um nome de exercício apropriado em português brasileiro.

Grupo Muscular: Bíceps (original: Biceps brachii)
Equipamento: Barra (original: Barbell)

Responda APENAS com um JSON no seguinte formato (sem markdown, sem explicações adicionais):
{
  "name": "Nome do Exercício em Português",
  "description": "Descrição detalhada de como executar o exercício, músculos trabalhados, dicas de técnica e benefícios (máximo 300 palavras)"
}`;

  try {
    const response = await makeRequest(
      `https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key=${GEMINI_API_KEY}`,
      'POST',
      {
        contents: [
          {
            parts: [
              { text: prompt }
            ]
          }
        ]
      }
    );

    console.log('\n=== FULL RESPONSE ===');
    console.log(JSON.stringify(response, null, 2));

    if (response?.candidates?.[0]?.content?.parts?.[0]?.text) {
      const text = response.candidates[0].content.parts[0].text;
      console.log('\n=== TEXT CONTENT ===');
      console.log(text);

      // Tentar parsear JSON
      let jsonText = text.replace(/```json\n?/g, '').replace(/```\n?/g, '').trim();
      console.log('\n=== CLEANED JSON TEXT ===');
      console.log(jsonText);

      try {
        const parsed = JSON.parse(jsonText);
        console.log('\n=== PARSED JSON ===');
        console.log(parsed);
      } catch (e) {
        console.log('\n=== JSON PARSE ERROR ===');
        console.log(e.message);
      }
    }
  } catch (error) {
    console.error('Error:', error.message);
  }
}

test();
