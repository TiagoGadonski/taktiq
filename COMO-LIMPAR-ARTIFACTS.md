# ⚠️ AÇÃO URGENTE: Limpar Artifacts do GitHub

## 🔴 Problema
Sua conta GitHub atingiu a cota de armazenamento de artifacts (500 MB).
**Você precisa deletar os artifacts antigos manualmente.**

---

## ✅ SOLUÇÃO 1: Via Web (RECOMENDADO - Mais Fácil)

### Passo a Passo:

1. **Abra seu navegador** e acesse:
   ```
   https://github.com/TiagoGadonski/taktiq/actions
   ```

2. **Na lista de workflows**, você verá várias execuções recentes. Clique em CADA UMA delas.

3. **Dentro de cada execução**, role até o final da página.

4. **Procure a seção "Artifacts"** (geralmente aparece com ícone de pacote 📦)

5. **Clique no ícone de LIXEIRA** (🗑️) ao lado de cada artifact para deletá-lo.

6. **Repita para TODAS as execuções** que tiverem artifacts.

**IMPORTANTE:** Você precisa deletar artifacts de AMBOS os workflows:
- ✅ Deploy Next.js to Azure (frontend)
- ✅ Build and deploy ASP.Net Core (backend)

### Exemplo Visual:
```
Workflow Run #123
├── Summary
├── Jobs
└── Artifacts                    ← PROCURE AQUI
    ├── deploy-archive (45 MB)   [🗑️ DELETAR] ← CLIQUE AQUI
    └── .net-app (123 MB)        [🗑️ DELETAR] ← CLIQUE AQUI
```

---

## ✅ SOLUÇÃO 2: Via PowerShell (Automático)

Se preferir automatizar:

1. **Abra PowerShell como Administrador**
   - Clique com botão direito no menu Iniciar
   - Selecione "Windows PowerShell (Admin)" ou "Terminal (Admin)"

2. **Navegue até a pasta do projeto:**
   ```powershell
   cd C:\Users\cwbcordeti\source\gymhero2
   ```

3. **Execute o script:**
   ```powershell
   .\cleanup-github-artifacts.ps1
   ```

4. **Siga as instruções na tela**
   - O script vai instalar o GitHub CLI (se necessário)
   - Vai pedir login no GitHub
   - Vai listar todos os artifacts
   - Vai perguntar se você quer deletar

---

## ⏰ Após Limpar

**AGUARDE 6-12 HORAS** para que o GitHub recalcule sua quota.

Após esse período, você poderá fazer push/deploy normalmente.

---

## 📊 Verificar Uso de Armazenamento

Após limpar, verifique em:
```
https://github.com/TiagoGadonski/taktiq/settings/billing
```

Na seção **"Storage for Actions and Packages"**, você verá quanto espaço está usando.

---

## ❓ FAQ

### Por que isso aconteceu?
Os artifacts estavam sendo mantidos por 90 dias (padrão). Com builds frequentes, eles acumularam e ultrapassaram os 500 MB grátis.

### Isso vai acontecer de novo?
NÃO! Já corrigimos os workflows para manter artifacts por apenas 1 dia (`retention-days: 1`).

### Posso deletar só alguns artifacts?
Sim, mas recomendamos deletar TODOS para liberar o máximo de espaço.

### Quanto espaço vou recuperar?
Provavelmente entre 200-500 MB, dependendo de quantos builds você tem.

---

## 🆘 Precisa de Ajuda?

Se tiver dificuldades, me avise e posso:
- Fazer screenshots do processo
- Criar um vídeo tutorial
- Tentar outras alternativas

**O importante é limpar os artifacts o quanto antes para o deploy voltar a funcionar!** ⚡
