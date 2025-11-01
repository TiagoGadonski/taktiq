const fs = require('fs');

const file = 'apps/web/src/app/(app)/plans/page.tsx';
const content = fs.readFileSync(file, 'utf8');
const lines = content.split('\n');
const openBraces = (content.match(/\{/g) || []).length;
const closeBraces = (content.match(/\}/g) || []).length;
const openParens = (content.match(/\(/g) || []).length;
const closeParens = (content.match(/\)/g) || []).length;

console.log('OK ' + file);
console.log('  Lines: ' + lines.length + ', Braces: ' + openBraces + '/' + closeBraces + ', Parens: ' + openParens + '/' + closeParens);

if (openBraces !== closeBraces || openParens !== closeParens) {
  console.log('  ERROR: Unmatched brackets!');
  process.exit(1);
}

console.log('\nSUCCESS: Plans page syntax is valid');
