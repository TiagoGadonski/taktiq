# Configuração de Envio de E-mail (SendGrid)

Este guia explica como configurar o envio de e-mails na plataforma TaktIQ/GymHero usando SendGrid.

## Status da Implementação

✅ **Já Implementado no Código:**
- Serviço de e-mail (`EmailService.cs`)
- Templates HTML profissionais
- Integração com SendGrid
- 3 tipos de e-mail:
  1. **Reset de senha** - Código de 6 dígitos
  2. **Convite de aluno** - Link de ativação de conta
  3. **Boas-vindas** - E-mail de boas-vindas (opcional)

❌ **Falta Configurar:**
- API Key do SendGrid no Azure
- Verificação do domínio (para evitar spam)

---

## Passo 1: Criar Conta e Obter API Key do SendGrid

### 1.1. Criar Conta no SendGrid

1. Acesse: https://sendgrid.com/
2. Clique em **Sign Up** (ou **Start for Free**)
3. Preencha seus dados:
   - E-mail
   - Nome
   - Empresa: TaktIQ
   - Senha
4. Confirme seu e-mail
5. Complete o questionário inicial (escolha "Web Application" como tipo)

### 1.2. Criar API Key

1. Faça login no SendGrid
2. No menu lateral, vá em: **Settings** > **API Keys**
3. Clique no botão **Create API Key**
4. Configure:
   - **API Key Name**: `TaktIQ Production`
   - **API Key Permissions**: Escolha **Restricted Access**
   - Em **Mail Send**, marque: **Full Access**
   - Deixe os demais em **No Access**
5. Clique em **Create & View**
6. **IMPORTANTE**: Copie a chave agora! Ela começa com `SG.` e não será mostrada novamente
   - Exemplo: `SG.abcd1234efgh5678ijkl9012mnop3456.xyz789...`
7. Guarde a chave em local seguro (LastPass, 1Password, etc.)

---

## Passo 2: Configurar no Azure

### Opção A: Via Azure Portal (Recomendado - Mais Visual)

1. Acesse: https://portal.azure.com
2. No menu lateral ou busca, encontre seu **App Service**
   - Nome provável: `gymhero` ou `taktiq-api`
3. No menu lateral do App Service, vá em: **Configuration**
4. Na aba **Application settings**, clique em: **+ New application setting**
5. Preencha:
   - **Name**: `SENDGRID_API_KEY`
   - **Value**: Cole a chave copiada (ex: `SG.abcd1234...`)
6. Clique em **OK**
7. Clique em **Save** no topo da página
8. Aguarde a confirmação e clique em **Continue**
9. **IMPORTANTE**: Reinicie o App Service:
   - Clique em **Overview** no menu lateral
   - Clique em **Restart** no topo
   - Confirme

### Opção B: Via Azure CLI (Mais Rápido - Se você tem CLI instalado)

```bash
# Substitua os valores conforme seu ambiente
az webapp config appsettings set \
  --name gymhero \
  --resource-group GymHero \
  --settings SENDGRID_API_KEY="SG.sua-chave-aqui"

# Reiniciar o App Service
az webapp restart \
  --name gymhero \
  --resource-group GymHero
```

---

## Passo 3: Verificar Domínio no SendGrid (Recomendado)

Verificar seu domínio melhora a entregabilidade dos e-mails e evita que caiam em spam.

### 3.1. Opção Simples: Single Sender Verification

**Use se:** Você quer configurar rápido para testes

1. No SendGrid, vá em: **Settings** > **Sender Authentication**
2. Clique em: **Verify a Single Sender**
3. Clique em: **Create New Sender**
4. Preencha o formulário:
   - **From Name**: `TaktIQ`
   - **From Email Address**: `noreply@taktiq.app` (ou seu e-mail real)
   - **Reply To**: Seu e-mail de contato
   - **Company Address**: Endereço da empresa
   - **City**: Cidade
   - **State**: Estado
   - **Zip Code**: CEP
   - **Country**: Brazil
5. Clique em **Create**
6. Você receberá um e-mail de verificação no endereço informado
7. Abra o e-mail e clique em **Verify Single Sender**

### 3.2. Opção Profissional: Domain Authentication (Recomendado para Produção)

**Use se:** Você tem acesso ao DNS do domínio `taktiq.app`

1. No SendGrid, vá em: **Settings** > **Sender Authentication**
2. Clique em: **Authenticate Your Domain**
3. Selecione:
   - **DNS Host**: Seu provedor de DNS (ex: GoDaddy, Cloudflare, AWS Route 53, etc.)
   - **Domain**: `taktiq.app`
   - **Advanced Settings**: Deixe padrão
4. Clique em **Next**
5. O SendGrid mostrará 3 registros DNS que você precisa adicionar:

   **Exemplo de registros (seus serão diferentes):**
   ```
   Tipo: CNAME
   Nome: s1._domainkey.taktiq.app
   Valor: s1.domainkey.u1234567.wl123.sendgrid.net

   Tipo: CNAME
   Nome: s2._domainkey.taktiq.app
   Valor: s2.domainkey.u1234567.wl123.sendgrid.net

   Tipo: CNAME
   Nome: em1234.taktiq.app
   Valor: u1234567.wl123.sendgrid.net
   ```

6. **Adicione esses registros no seu provedor DNS:**

   **Se usar Cloudflare:**
   - Faça login no Cloudflare
   - Selecione o domínio `taktiq.app`
   - Vá em **DNS** > **Records**
   - Clique em **Add record**
   - Para cada registro:
     - Type: `CNAME`
     - Name: Cole o nome (ex: `s1._domainkey`)
     - Target: Cole o valor
     - Proxy status: **DNS only** (nuvem cinza, não laranja)
     - TTL: Auto
     - Clique em **Save**

   **Se usar GoDaddy, AWS Route 53, etc:**
   - O processo é similar, consulte a documentação do seu provedor

7. Volte ao SendGrid e clique em **Verify**
8. A verificação pode levar de 5 minutos a 48 horas
9. Você receberá um e-mail quando a verificação estiver completa

---

## Passo 4: Testar o Envio de E-mails

### 4.1. Testar Reset de Senha

1. Abra o site: https://taktiq.app (ou sua URL de produção)
2. Vá para a página de login
3. Clique em **Esqueci minha senha**
4. Digite seu e-mail
5. Clique em **Enviar código**
6. Verifique sua caixa de entrada (e spam)
7. Você deve receber um e-mail com:
   - Assunto: "Redefinição de Senha - TaktIQ"
   - Código de 6 dígitos
   - Link para redefinir senha
8. Use o código para redefinir a senha

### 4.2. Testar Convite de Aluno

1. Faça login como **Personal Trainer**
2. Vá para o painel do instrutor
3. Procure a opção de **Convidar Aluno** ou **Adicionar Cliente**
4. Preencha:
   - E-mail do aluno (use um e-mail que você tenha acesso para testar)
   - Nome do aluno
   - Opcionalmente, selecione um plano de treino
5. Clique em **Enviar Convite**
6. O aluno receberá um e-mail com:
   - Assunto: "[Seu Nome] te convidou para o TaktIQ! 🎯"
   - Template bonito com gradiente roxo
   - Botão "Ativar Minha Conta"
   - Link de ativação
7. Clique no link ou botão
8. Preencha os dados (nome, senha, local de treino preferido)
9. A conta será criada automaticamente vinculada ao PT

### 4.3. Verificar Logs no Azure

Se algo não funcionar, verifique os logs:

1. No Azure Portal, vá para seu App Service
2. Menu lateral: **Monitoring** > **Log stream**
3. Ou vá em: **Monitoring** > **Logs**
4. Execute uma query:
   ```kusto
   traces
   | where message contains "email" or message contains "SendGrid"
   | order by timestamp desc
   | take 50
   ```

---

## Passo 5: Teste Local (Opcional - Para Desenvolvimento)

Se quiser testar localmente antes de fazer deploy:

### 5.1. Criar Arquivo de Configuração Local

1. Crie o arquivo: `src/GymHero.Api/appsettings.Development.Local.json`

   ```json
   {
     "SendGrid": {
       "ApiKey": "SG.sua-chave-aqui"
     }
   }
   ```

2. Este arquivo já está no `.gitignore`, então não será commitado

### 5.2. Rodar a API Localmente

```bash
cd src/GymHero.Api
dotnet run
```

### 5.3. Testar com cURL ou Postman

**Teste de Reset de Senha:**
```bash
curl -X POST http://localhost:5000/api/auth/forgot-password \
  -H "Content-Type: application/json" \
  -d '{"email": "seu-email@teste.com"}'
```

**Teste de Convite (precisa estar autenticado como PT):**
```bash
curl -X POST http://localhost:5000/api/personal/invitations \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer SEU_TOKEN_JWT_AQUI" \
  -d '{
    "email": "aluno@teste.com",
    "name": "Aluno Teste",
    "workoutPlanId": null
  }'
```

---

## Informações Técnicas

### E-mails Configurados

| Tipo | Método | Gatilho | Remetente |
|------|--------|---------|-----------|
| **Reset de Senha** | `SendPasswordResetEmailAsync` | POST `/api/auth/forgot-password` | noreply@taktiq.app |
| **Convite de Aluno** | `SendStudentInvitationEmailAsync` | POST `/api/personal/invitations` | noreply@taktiq.app |
| **Boas-vindas** | `SendWelcomeEmailAsync` | (Atualmente não usado) | noreply@taktiq.app |

### Expirações

- **Token de Reset de Senha**: 1 hora
- **Convite de Aluno**: 7 dias

### Templates HTML

Os templates já estão implementados com:
- Design responsivo
- Gradientes roxos (brand colors)
- Botões de call-to-action
- Links alternativos (se o botão não funcionar)
- Footer com links para o site

### Arquivo de Implementação

Código completo está em:
```
src/GymHero.Infrastructure/Services/EmailService.cs
```

---

## Troubleshooting

### E-mail não está sendo enviado

1. **Verificar API Key no Azure:**
   - Azure Portal > App Service > Configuration > Application settings
   - Confirme que `SENDGRID_API_KEY` está configurado
   - Verifique se não tem espaços extras na chave

2. **Verificar logs:**
   - Azure Portal > App Service > Log stream
   - Procure por erros relacionados a "SendGrid" ou "email"

3. **Verificar status no SendGrid:**
   - SendGrid Dashboard > Activity
   - Veja se há e-mails processados, bounces ou bloqueios

### E-mail cai no spam

1. **Verificar domínio:**
   - Settings > Sender Authentication
   - Confirme que o domínio está verificado

2. **Usar domínio próprio:**
   - Em vez de `noreply@taktiq.app`, use um e-mail real que você possa verificar

3. **Evitar palavras de spam:**
   - Os templates já estão otimizados, mas evite adicionar palavras como "grátis", "clique aqui", etc.

### API Key inválida

Sintomas:
- Erro 401 nos logs
- Mensagem: "The provided authorization grant is invalid, expired, or revoked"

Solução:
1. Gere uma nova API Key no SendGrid
2. Atualize no Azure
3. Reinicie o App Service

### Limite de envios atingido

SendGrid Free Tier:
- 100 e-mails/dia

Se precisar de mais:
1. SendGrid Dashboard > Settings > Billing
2. Faça upgrade do plano (a partir de $19.95/mês para 50k e-mails)

---

## Custos do SendGrid

| Plano | Preço/mês | E-mails/mês | Ideal para |
|-------|-----------|-------------|------------|
| **Free** | $0 | 100/dia (3.000/mês) | Testes e MVP |
| **Essentials** | $19.95 | 50.000 | Produção inicial |
| **Pro 100K** | $89.95 | 100.000 | Escala média |

Link: https://sendgrid.com/pricing/

---

## Checklist de Configuração

- [ ] Conta criada no SendGrid
- [ ] API Key gerada e copiada
- [ ] API Key adicionada no Azure (`SENDGRID_API_KEY`)
- [ ] App Service reiniciado
- [ ] Domínio verificado (Single Sender ou Domain Authentication)
- [ ] Teste de reset de senha realizado
- [ ] Teste de convite de aluno realizado
- [ ] E-mails chegando na caixa de entrada (não no spam)
- [ ] Logs do Azure verificados (sem erros)

---

## Próximos Passos Após Configuração

1. **Monitoramento**: Configure alertas no SendGrid para bounces e bloqueios
2. **Templates**: Se quiser personalizar os e-mails, edite `EmailService.cs`
3. **Analytics**: Use o SendGrid Dashboard para ver taxa de abertura e cliques
4. **Backup**: Considere ter uma API Key de backup
5. **Produção**: Quando escalar, faça upgrade do plano do SendGrid

---

## Suporte

- **SendGrid Docs**: https://docs.sendgrid.com/
- **SendGrid Support**: https://support.sendgrid.com/
- **Azure Support**: https://portal.azure.com/#blade/Microsoft_Azure_Support/HelpAndSupportBlade

---

**Última atualização**: 2024-12-08
