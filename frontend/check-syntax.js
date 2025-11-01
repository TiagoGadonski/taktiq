const fs = require('fs');

const files = [
  'packages/shared/src/utils/motivational-messages.ts',
  'apps/web/src/app/(app)/workout/page.tsx',
  'apps/web/src/components/workout/share-settings-dialog.tsx',
  'apps/web/src/app/(app)/plans/discover/page.tsx',
  'packages/shared/src/api/endpoints.ts'
];

let allGood = true;

files.forEach(file => {
  try {
    const content = fs.readFileSync(file, 'utf8');
    const lines = content.split('\n');

    // Count brackets
    const openBraces = (content.match(/\{/g) || []).length;
    const closeBraces = (content.match(/\}/g) || []).length;
    const openParens = (content.match(/\(/g) || []).length;
    const closeParens = (content.match(/\)/g) || []).length;

    console.log(`OK ${file}`);
    console.log(`  Lines: ${lines.length}, Braces: ${openBraces}/${closeBraces}, Parens: ${openParens}/${closeParens}`);

    if (openBraces !== closeBraces) {
      console.log(`  WARNING: Unmatched braces!`);
      allGood = false;
    }
    if (openParens !== closeParens) {
      console.log(`  WARNING: Unmatched parentheses!`);
      allGood = false;
    }
  } catch (e) {
    console.log(`ERROR ${file}: ${e.message}`);
    allGood = false;
  }
});

console.log('');
console.log(allGood ? 'SUCCESS: All files passed basic syntax checks' : 'FAIL: Some files have issues');
process.exit(allGood ? 0 : 1);
