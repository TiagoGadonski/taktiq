'use client';

import { useState } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Mail, MessageCircle, Github, Twitter, Linkedin, Instagram, Heart, Copy, Check, DollarSign, Info, FacebookIcon } from 'lucide-react';
import { useToast } from '@/hooks/use-toast';

export default function AboutPage() {
  const { toast } = useToast();
  const [copiedPix, setCopiedPix] = useState(false);

  // TODO: Atualize estas informações com suas redes sociais reais
  const socialLinks = [
    {
      name: 'Email',
      icon: Mail,
      href: 'mailto:contato@taktiq.app',
      username: 'contato@taktiq.app',
      description: 'Entre em contato por e-mail',
      color: 'hover:text-red-500',
    },
    {
      name: 'Instagram',
      icon: Instagram,
      href: 'https://www.instagram.com/taktiq.app/',
      username: '@taktiq.app',
      description: 'Siga-nos no Instagram',
      color: 'hover:text-pink-500',
    },
    {
      name: 'Facebook',
      icon: FacebookIcon,
      href: 'https://www.facebook.com/taktiqapp/',
      username: '@taktiq.app',
      description: 'Siga-nos no Facebook',
      color: 'hover:text-pink-500',
    },
    // {
    //   name: 'Twitter/X',
    //   icon: Twitter,
    //   href: 'https://twitter.com/gymhero',
    //   username: '@gymhero',
    //   description: 'Siga-nos no Twitter',
    //   color: 'hover:text-blue-400',
    // },
    {
      name: 'LinkedIn',
      icon: Linkedin,
      href: 'https://www.linkedin.com/company/taktiq-app/',
      username: 'Taktiq App',
      description: 'Conecte-se no LinkedIn',
      color: 'hover:text-blue-600',
    },
    
  ];

  // TODO: Atualize esta chave Pix com a sua chave real (pode ser email, CPF, CNPJ, telefone ou chave aleatória)
  const pixKey = '[REDACTED]';

  const copyPixKey = () => {
    navigator.clipboard.writeText(pixKey);
    setCopiedPix(true);
    toast({
      title: 'Chave Pix copiada!',
      description: 'A chave Pix foi copiada para a área de transferência.',
    });
    setTimeout(() => setCopiedPix(false), 2000);
  };

  return (
    <div className="space-y-6 max-w-4xl mx-auto">
      <div className="space-y-2">
        <h1 className="text-3xl font-bold tracking-tight bg-gradient-to-r from-primary to-primary/70 bg-clip-text text-transparent">
          Sobre Nós
        </h1>
        <p className="text-muted-foreground">
          Conheça mais sobre o TaktIQ e como você pode apoiar este projeto.
        </p>
      </div>

      {/* About the Project Section */}
      <Card className="glass-card border-primary/20">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Info className="h-5 w-5 text-primary" />
            Sobre o Projeto
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="prose dark:prose-invert max-w-none">
            <p className="text-muted-foreground">
              O <strong className="text-foreground">TaktIQ</strong> é uma plataforma completa de gerenciamento de treinos desenvolvida para
              ajudar atletas, personal trainers e entusiastas do fitness a alcançarem seus objetivos de forma organizada e eficiente.
            </p>
            <p className="text-muted-foreground">
              Nossa missão é democratizar o acesso a ferramentas profissionais de planejamento e acompanhamento de treinos,
              oferecendo uma solução moderna, intuitiva e poderosa que se adapta às necessidades de cada usuário.
            </p>
            <div className="bg-muted/50 p-4 rounded-lg space-y-2 mt-4">
              <h3 className="font-semibold text-sm">Principais Funcionalidades:</h3>
              <ul className="space-y-1 text-sm text-muted-foreground">
                <li className="flex items-start gap-2">
                  <span className="text-primary mt-0.5">•</span>
                  <span>Criação e gerenciamento de planos de treino personalizados</span>
                </li>
                <li className="flex items-start gap-2">
                  <span className="text-primary mt-0.5">•</span>
                  <span>Geração inteligente de treinos com IA</span>
                </li>
                <li className="flex items-start gap-2">
                  <span className="text-primary mt-0.5">•</span>
                  <span>Acompanhamento de progresso e evolução</span>
                </li>
                <li className="flex items-start gap-2">
                  <span className="text-primary mt-0.5">•</span>
                  <span>Desafios e conquistas para manter a motivação</span>
                </li>
                <li className="flex items-start gap-2">
                  <span className="text-primary mt-0.5">•</span>
                  <span>Compartilhamento de treinos com amigos</span>
                </li>
                <li className="flex items-start gap-2">
                  <span className="text-primary mt-0.5">•</span>
                  <span>Gestão de clientes para personal trainers</span>
                </li>
              </ul>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Social Media Section */}
      <Card className="glass-card border-primary/20">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Heart className="h-5 w-5 text-primary" />
            Redes Sociais
          </CardTitle>
          <CardDescription>
            Conecte-se conosco e fique por dentro de todas as novidades!
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-4 sm:grid-cols-2">
            {socialLinks.map((link) => (
              <a
                key={link.name}
                href={link.href}
                target="_blank"
                rel="noopener noreferrer"
                className="group"
              >
                <Card className="glass-card border-border/50 hover:border-primary/50 transition-all hover-lift tap-scale h-full">
                  <CardContent className="p-4">
                    <div className="flex items-start gap-3">
                      <div className={`p-2 rounded-lg bg-primary/10 group-hover:bg-primary/20 transition-colors`}>
                        <link.icon className={`h-5 w-5 text-primary ${link.color} transition-colors`} />
                      </div>
                      <div className="flex-1 min-w-0">
                        <h3 className="font-semibold text-sm mb-1">{link.name}</h3>
                        <p className="text-xs text-muted-foreground mb-1 truncate">
                          {link.username}
                        </p>
                        <p className="text-xs text-muted-foreground">
                          {link.description}
                        </p>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              </a>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Contact Section */}
      <Card className="glass-card border-primary/20">
        <CardHeader>
          <CardTitle>Entre em Contato</CardTitle>
          <CardDescription>
            Tem dúvidas, sugestões ou quer relatar um problema? Estamos aqui para ajudar!
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="bg-muted/50 p-4 rounded-lg space-y-2">
            <h3 className="font-semibold text-sm">Como você pode nos ajudar:</h3>
            <ul className="space-y-1 text-sm text-muted-foreground">
              <li className="flex items-start gap-2">
                <span className="text-primary mt-0.5">•</span>
                <span>Reporte bugs ou problemas que encontrar</span>
              </li>
              <li className="flex items-start gap-2">
                <span className="text-primary mt-0.5">•</span>
                <span>Sugira novas funcionalidades</span>
              </li>
              <li className="flex items-start gap-2">
                <span className="text-primary mt-0.5">•</span>
                <span>Compartilhe sua experiência com o app</span>
              </li>
              <li className="flex items-start gap-2">
                <span className="text-primary mt-0.5">•</span>
                <span>Participe da nossa comunidade</span>
              </li>
            </ul>
          </div>

          <div className="flex flex-col sm:flex-row gap-3">
            <Button
              variant="default"
              className="flex-1 hover-lift tap-scale"
              onClick={() => window.open('mailto:contato@taktiq.app?subject=Contato pelo TaktIQ', '_blank')}
            >
              <Mail className="mr-2 h-4 w-4" />
              Enviar E-mail
            </Button>
            {/* <Button
              variant="outline"
              className="flex-1 hover-lift tap-scale"
              onClick={() => window.open('https://github.com/gymhero/gymhero/issues', '_blank')}
            >
              <Github className="mr-2 h-4 w-4" />
              Reportar Bug
            </Button> */}
          </div>
        </CardContent>
      </Card>

      {/* Pix Donation Section */}
      <Card className="glass-card border-primary/20">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <DollarSign className="h-5 w-5 text-primary" />
            Apoie o Projeto
          </CardTitle>
          <CardDescription>
            O TaktIQ é um projeto independente. Sua contribuição ajuda a manter e melhorar a plataforma!
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="bg-gradient-to-br from-primary/10 to-primary/5 p-6 rounded-lg border border-primary/20">
            <div className="text-center space-y-4">
              <div className="space-y-2">
                <h3 className="font-semibold">Doe via Pix</h3>
                <p className="text-sm text-muted-foreground">
                  Qualquer valor é bem-vindo e faz a diferença!
                </p>
              </div>

              <div className="bg-background/80 p-4 rounded-lg border border-border">
                <div className="flex items-center gap-2 justify-center">
                  <Input
                    value={pixKey}
                    readOnly
                    className="text-center font-mono text-sm"
                  />
                  <Button
                    size="icon"
                    variant="outline"
                    onClick={copyPixKey}
                    className="shrink-0"
                  >
                    {copiedPix ? (
                      <Check className="h-4 w-4 text-green-500" />
                    ) : (
                      <Copy className="h-4 w-4" />
                    )}
                  </Button>
                </div>
                <p className="text-xs text-muted-foreground mt-2">
                  Clique no botão para copiar a chave Pix
                </p>
              </div>

              <div className="pt-2">
                <p className="text-xs text-muted-foreground">
                  💚 Muito obrigado pelo seu apoio!
                </p>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Footer */}
      <Card className="glass-card border-primary/20">
        <CardContent className="p-6">
          <div className="text-center space-y-2">
            <p className="text-sm text-muted-foreground">
              Desenvolvido com <Heart className="inline h-4 w-4 text-red-500" /> por Tiago Cordeiro.
            </p>
            <p className="text-xs text-muted-foreground">
              TaktIQ • {new Date().getFullYear()}
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
