# Instruções para Deploy Manual no Azure

## Opção 1: Upload via Azure Portal (MAIS FÁCIL)

1. Acesse https://portal.azure.com
2. Encontre seu App Service (provavelmente "taktiq-api" ou similar)
3. No menu lateral esquerdo, procure por **"Advanced Tools"** ou **"Kudu"**
4. Clique em **"Go"** - abrirá uma nova aba
5. No Kudu, clique em **"Tools"** → **"Zip Push Deploy"**
6. Arraste o arquivo `deploy.zip` para a área indicada
7. Aguarde o upload e extração completar
8. Volte ao Azure Portal e clique em **"Restart"**

## Opção 2: Azure CLI (se tiver instalado)

```bash
# Substitua pelos seus valores
az webapp deployment source config-zip \
  --resource-group <seu-resource-group> \
  --name <seu-app-name> \
  --src deploy.zip
```

## Opção 3: Verificar Deployment Center

1. Azure Portal → Seu App Service
2. **"Deployment Center"** no menu lateral
3. Veja se há GitHub conectado
4. Se sim, veja os logs de deploy
5. Pode haver erro impedindo deploy automático
6. Tente **"Sync"** ou **"Disconnect"** e reconectar

## Após o Deploy

Teste novamente criando um plano de treino para um aluno!

## Problema Identificado e Corrigido

- ✅ Alunos agora têm PersonalTrainerId configurado (via SQL)
- ✅ Código corrigido para verificar permissões corretamente
- ⏳ Aguardando deploy do código corrigido
