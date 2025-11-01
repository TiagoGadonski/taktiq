'use client';

import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { ArrowLeft, Shield } from 'lucide-react';

export default function PrivacyPage() {
  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-500 to-purple-600 p-4 py-12">
      <div className="container mx-auto max-w-4xl">
        <Card className="glass">
          <CardHeader className="space-y-4">
            <Link href="/signup">
              <Button variant="ghost" size="sm" className="gap-2">
                <ArrowLeft className="h-4 w-4" />
                Voltar
              </Button>
            </Link>
            <div className="flex items-center gap-3">
              <Shield className="h-8 w-8 text-primary" />
              <CardTitle className="text-3xl font-bold">Política de Privacidade</CardTitle>
            </div>
            <p className="text-sm text-muted-foreground">
              Última atualização: {new Date().toLocaleDateString('pt-BR')}
            </p>
          </CardHeader>
          <CardContent className="space-y-6 text-sm">
            <section>
              <h2 className="text-xl font-semibold mb-3">1. Introdução</h2>
              <p className="text-muted-foreground leading-relaxed">
                Bem-vindo ao TaktIQ. Esta Política de Privacidade descreve como coletamos, usamos,
                armazenamos e protegemos suas informações pessoais quando você utiliza nossa aplicação
                de gestão de treinos e fitness.
              </p>
            </section>

            <section>
              <h2 className="text-xl font-semibold mb-3">2. Informações que Coletamos</h2>
              <div className="space-y-2 text-muted-foreground">
                <p className="font-medium text-foreground">2.1 Informações de Conta</p>
                <ul className="list-disc list-inside space-y-1 ml-4">
                  <li>Nome completo</li>
                  <li>Endereço de e-mail</li>
                  <li>Senha (armazenada de forma criptografada)</li>
                  <li>Foto de perfil (opcional)</li>
                  <li>Informações de perfil (altura, peso, localização, biografia)</li>
                </ul>

                <p className="font-medium text-foreground mt-4">2.2 Dados de Treino</p>
                <ul className="list-disc list-inside space-y-1 ml-4">
                  <li>Planos de treino criados e seguidos</li>
                  <li>Exercícios realizados e suas repetições</li>
                  <li>Histórico de sessões de treino</li>
                  <li>Métricas de progresso (peso levantado, repetições, tempo)</li>
                  <li>Desafios participados e concluídos</li>
                </ul>

                <p className="font-medium text-foreground mt-4">2.3 Dados de Uso</p>
                <ul className="list-disc list-inside space-y-1 ml-4">
                  <li>Informações sobre como você usa a aplicação</li>
                  <li>Frequência de acesso e padrões de uso</li>
                  <li>Interações com recursos da aplicação</li>
                </ul>
              </div>
            </section>

            <section>
              <h2 className="text-xl font-semibold mb-3">3. Como Utilizamos seus Dados</h2>
              <div className="space-y-2 text-muted-foreground">
                <p className="leading-relaxed">
                  Ao se cadastrar no TaktIQ, você concorda expressamente que seus dados de treino
                  e perfil sejam utilizados para:
                </p>
                <ul className="list-disc list-inside space-y-1 ml-4">
                  <li>
                    <strong className="text-foreground">Testes e desenvolvimento:</strong> Avaliar e melhorar
                    funcionalidades da aplicação
                  </li>
                  <li>
                    <strong className="text-foreground">Análise de desempenho:</strong> Identificar padrões
                    de uso e otimizar a experiência do usuário
                  </li>
                  <li>
                    <strong className="text-foreground">Aprimoramento de recursos:</strong> Desenvolver novos
                    recursos baseados em dados de uso reais
                  </li>
                  <li>
                    <strong className="text-foreground">Garantia de qualidade:</strong> Testar a estabilidade
                    e confiabilidade da plataforma
                  </li>
                  <li>
                    <strong className="text-foreground">Fornecimento do serviço:</strong> Permitir que você
                    use todas as funcionalidades da aplicação
                  </li>
                  <li>
                    <strong className="text-foreground">Comunicação:</strong> Enviar notificações importantes
                    sobre sua conta e atualizações do serviço
                  </li>
                </ul>
              </div>
            </section>

            <section>
              <h2 className="text-xl font-semibold mb-3">4. Compartilhamento de Dados</h2>
              <div className="space-y-2 text-muted-foreground">
                <p className="leading-relaxed">
                  Seus dados pessoais não serão vendidos ou compartilhados com terceiros para fins
                  comerciais. Podemos compartilhar dados apenas nas seguintes situações:
                </p>
                <ul className="list-disc list-inside space-y-1 ml-4">
                  <li>Com personal trainers designados (se você for um cliente)</li>
                  <li>Com outros usuários que você adicionar como amigos (dados limitados de perfil)</li>
                  <li>Quando exigido por lei ou ordem judicial</li>
                  <li>Para proteger os direitos e segurança do TaktIQ e seus usuários</li>
                </ul>
              </div>
            </section>

            <section>
              <h2 className="text-xl font-semibold mb-3">5. Armazenamento e Segurança</h2>
              <div className="space-y-2 text-muted-foreground leading-relaxed">
                <p>
                  Implementamos medidas de segurança técnicas e organizacionais apropriadas para
                  proteger seus dados pessoais contra acesso não autorizado, alteração, divulgação
                  ou destruição. Isso inclui:
                </p>
                <ul className="list-disc list-inside space-y-1 ml-4">
                  <li>Criptografia de senhas usando BCrypt</li>
                  <li>Comunicação segura via HTTPS</li>
                  <li>Controle de acesso baseado em função (RBAC)</li>
                  <li>Backup regular de dados</li>
                  <li>Monitoramento de segurança contínuo</li>
                </ul>
              </div>
            </section>

            <section>
              <h2 className="text-xl font-semibold mb-3">6. Retenção de Dados</h2>
              <p className="text-muted-foreground leading-relaxed">
                Mantemos seus dados pessoais enquanto sua conta estiver ativa ou conforme necessário
                para fornecer nossos serviços. Você pode solicitar a exclusão de sua conta a qualquer
                momento, após o qual seus dados serão removidos de nossos sistemas, exceto quando a
                retenção for necessária para cumprimento de obrigações legais.
              </p>
            </section>

            <section>
              <h2 className="text-xl font-semibold mb-3">7. Seus Direitos</h2>
              <div className="space-y-2 text-muted-foreground">
                <p className="leading-relaxed">
                  De acordo com a LGPD (Lei Geral de Proteção de Dados), você tem os seguintes direitos:
                </p>
                <ul className="list-disc list-inside space-y-1 ml-4">
                  <li>Acessar seus dados pessoais</li>
                  <li>Corrigir dados incompletos, inexatos ou desatualizados</li>
                  <li>Solicitar a exclusão de seus dados</li>
                  <li>Revogar seu consentimento a qualquer momento</li>
                  <li>Solicitar a portabilidade de seus dados</li>
                  <li>Obter informações sobre o tratamento de seus dados</li>
                </ul>
                <p className="mt-4 leading-relaxed">
                  Para exercer qualquer um desses direitos, entre em contato conosco através da sua
                  conta ou solicitando suporte aos administradores do sistema.
                </p>
              </div>
            </section>

            <section>
              <h2 className="text-xl font-semibold mb-3">8. Cookies e Tecnologias Similares</h2>
              <p className="text-muted-foreground leading-relaxed">
                Utilizamos tokens de autenticação e armazenamento local do navegador para manter
                você conectado e melhorar sua experiência. Esses dados são armazenados localmente
                no seu dispositivo e podem ser removidos ao fazer logout ou limpar os dados do navegador.
              </p>
            </section>

            <section>
              <h2 className="text-xl font-semibold mb-3">9. Menores de Idade</h2>
              <p className="text-muted-foreground leading-relaxed">
                Nossa aplicação não é direcionada a menores de 18 anos. Não coletamos intencionalmente
                informações pessoais de menores de idade. Se você é pai ou responsável e acredita que
                seu filho nos forneceu informações pessoais, entre em contato para que possamos remover
                essas informações.
              </p>
            </section>

            <section>
              <h2 className="text-xl font-semibold mb-3">10. Alterações nesta Política</h2>
              <p className="text-muted-foreground leading-relaxed">
                Podemos atualizar esta Política de Privacidade periodicamente. Notificaremos você sobre
                alterações significativas através da aplicação ou por e-mail. Recomendamos que você
                revise esta política regularmente para se manter informado sobre como protegemos suas
                informações.
              </p>
            </section>

            <section>
              <h2 className="text-xl font-semibold mb-3">11. Contato</h2>
              <div className="text-muted-foreground leading-relaxed space-y-2">
                <p>
                  Se você tiver dúvidas sobre esta Política de Privacidade ou sobre como tratamos
                  seus dados pessoais, entre em contato com os administradores do TaktIQ através
                  da aplicação.
                </p>
                <div className="mt-4 p-4 bg-muted/50 rounded-lg border border-border">
                  <p className="font-semibold text-foreground">TaktIQ - Gestão de Treinos</p>
                  <p>Desenvolvido para fins educacionais e de teste</p>
                  <p className="text-xs mt-2">
                    Esta é uma aplicação em desenvolvimento e seus dados serão utilizados para
                    aprimoramento contínuo do sistema.
                  </p>
                </div>
              </div>
            </section>

            <section className="pt-6 border-t">
              <p className="text-xs text-muted-foreground text-center">
                Ao utilizar o TaktIQ, você reconhece que leu e compreendeu esta Política de
                Privacidade e concorda com seus termos.
              </p>
            </section>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
