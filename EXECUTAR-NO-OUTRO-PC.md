# 🖥️ GUIA: Limpar Artifacts no Computador com Admin

## 📋 O QUE VOCÊ PRECISA FAZER

Com 32 páginas de actions, deletar manualmente levaria HORAS! 😱
Vamos usar o script automático que criamos.

---

## ⚡ PASSO A PASSO RÁPIDO (5 minutos)

### 1️⃣ No Computador ATUAL (onde você está agora):

**Copie esta URL:**
```
https://github.com/TiagoGadonski/taktiq
```

Você vai precisar dela no outro PC.

---

### 2️⃣ No Computador COM ADMIN:

#### A) Abrir PowerShell como Administrador

1. Aperte **Windows + X**
2. Clique em **"Terminal (Admin)"** ou **"Windows PowerShell (Admin)"**
3. Se perguntar "Deseja permitir?", clique em **Sim**

#### B) Clonar o repositório (se não tiver)

Cole este comando:
```powershell
cd $HOME
git clone https://github.com/TiagoGadonski/taktiq.git
cd taktiq
```

**OU** se já tiver clonado, só atualizar:
```powershell
cd $HOME/taktiq
git pull
```

#### C) Executar o script de limpeza

Cole este comando:
```powershell
.\cleanup-github-artifacts.ps1
```

#### D) Seguir as instruções na tela

O script vai:
1. ✅ Instalar GitHub CLI automaticamente (se necessário)
2. ✅ Pedir para você fazer login no GitHub
3. ✅ Listar TODOS os artifacts (pode demorar 1-2 min)
4. ✅ Perguntar se quer deletar tudo
5. ✅ Deletar TODOS os artifacts automaticamente! 🎉

---

## 🔐 Login no GitHub CLI

Quando o script pedir para fazer login, você vai ver algo assim:

```
? What account do you want to log into?
> GitHub.com

? What is your preferred protocol for Git operations?
> HTTPS

? Authenticate Git with your GitHub credentials?
> Yes

? How would you like to authenticate GitHub CLI?
> Login with a web browser
```

**Escolha sempre as opções destacadas acima!**

Depois vai abrir uma página no navegador:
1. Digite seu **usuário e senha do GitHub**
2. Cole o código que apareceu no terminal
3. Clique em **Authorize**
4. Volte para o PowerShell

---

## 📊 O QUE VOCÊ VAI VER

```
========================================
GitHub Artifacts Cleanup Script
========================================

GitHub CLI encontrado!

Buscando artifacts do repositório TiagoGadonski/taktiq...

Encontrados 120 artifacts          👈 Pode ser esse número!
Tamanho total: 487.34 MB          👈 Liberando MUITA quota!

Lista de artifacts:
ID          Nome                Tamanho (MB)  Criado em
──────────  ──────────────────  ────────────  ──────────────
123456789   deploy-archive      45.2          2024-11-20
987654321   .net-app           123.4          2024-11-19
...

Deseja deletar TODOS os artifacts? (S/N):
```

**Digite `S` e aperte Enter**

---

## ⏱️ Tempo Estimado

| Etapa | Tempo |
|-------|-------|
| Clonar repo (se necessário) | 30s |
| Instalar GitHub CLI | 1 min |
| Login no GitHub | 1 min |
| Listar artifacts | 1-2 min |
| Deletar todos | 2-3 min |
| **TOTAL** | **5-7 minutos** |

---

## 🎯 COMANDOS PRONTOS (copie tudo de uma vez!)

Se quiser ir SUPER RÁPIDO, cole tudo isso de uma vez no PowerShell:

```powershell
# Navegar para a home
cd $HOME

# Clonar ou atualizar repositório
if (Test-Path "taktiq") {
    cd taktiq
    git pull
} else {
    git clone https://github.com/TiagoGadonski/taktiq.git
    cd taktiq
}

# Executar script
.\cleanup-github-artifacts.ps1
```

**IMPORTANTE:** Depois de colar, o script vai parar para pedir confirmação antes de deletar. Isso é normal! ✅

---

## ❓ PERGUNTAS FREQUENTES

### E se o git não estiver instalado?

Instale com este comando:
```powershell
winget install --id Git.Git -e --source winget
```

Depois feche e abra o PowerShell novamente.

### E se der erro de permissão?

Certifique-se de que abriu o PowerShell **como Administrador**.

### Posso cancelar no meio?

Sim! Aperte **Ctrl + C** a qualquer momento.

### Quanto espaço vou liberar?

Provavelmente **400-500 MB** (quase toda a quota!).

---

## 🎉 APÓS EXECUTAR

1. ✅ Aguarde **6-12 horas** para o GitHub recalcular a quota
2. ✅ Volte para este computador (ou qualquer outro)
3. ✅ Faça um novo commit/push
4. ✅ O deploy vai funcionar! 🚀

---

## 📞 PRECISA DE AJUDA?

Se der algum erro:
1. Tire um print da tela
2. Me mande a mensagem de erro
3. Vou te ajudar a resolver!

---

## 🔄 ALTERNATIVA: Deletar Via Web (se o script não funcionar)

Se por algum motivo o script não funcionar, você AINDA pode deletar via web, mas tem um truque:

1. Vá em: https://github.com/TiagoGadonski/taktiq/actions
2. **CTRL + F** e busque por "Artifacts"
3. Clique em cada execução que tiver a palavra "Artifacts"
4. Delete os artifacts dentro dela

Com 32 páginas, vai demorar, mas é a alternativa. O script é MUITO mais rápido! ⚡

---

## ✅ RESUMÃO

**No PC com Admin:**
```
1. Abrir PowerShell como Admin
2. Clonar repo (ou atualizar)
3. Executar: .\cleanup-github-artifacts.ps1
4. Fazer login no GitHub
5. Digitar 'S' para confirmar
6. Aguardar terminar
7. PRONTO! 🎉
```

**Boa sorte! O script vai fazer tudo automaticamente!** 💪
