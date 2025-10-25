'use client';

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Mail, MessageCircle, Github, Twitter, Linkedin, Globe, Heart } from 'lucide-react';

export default function ContactPage() {
  const socialLinks = [
    {
      name: 'Email',
      icon: Mail,
      href: 'mailto:contact@gymhero.app',
      username: 'contact@gymhero.app',
      description: 'Entre em contato por e-mail',
      color: 'hover:text-red-500',
    },
    {
      name: 'Twitter/X',
      icon: Twitter,
      href: 'https://twitter.com/gymhero',
      username: '@gymhero',
      description: 'Siga-nos no Twitter',
      color: 'hover:text-blue-400',
    },
    {
      name: 'LinkedIn',
      icon: Linkedin,
      href: 'https://linkedin.com/company/gymhero',
      username: 'GymHero',
      description: 'Conecte-se no LinkedIn',
      color: 'hover:text-blue-600',
    },
    {
      name: 'GitHub',
      icon: Github,
      href: 'https://github.com/gymhero',
      username: 'github.com/gymhero',
      description: 'Veja nosso código',
      color: 'hover:text-gray-900 dark:hover:text-gray-100',
    },
    {
      name: 'Discord',
      icon: MessageCircle,
      href: 'https://discord.gg/gymhero',
      username: 'discord.gg/gymhero',
      description: 'Junte-se à nossa comunidade',
      color: 'hover:text-indigo-500',
    },
    {
      name: 'Website',
      icon: Globe,
      href: 'https://gymhero.app',
      username: 'gymhero.app',
      description: 'Visite nosso site',
      color: 'hover:text-green-500',
    },
  ];

  return (
    <div className="space-y-6 max-w-4xl mx-auto">
      <div className="space-y-2">
        <h1 className="text-3xl font-bold tracking-tight bg-gradient-to-r from-primary to-primary/70 bg-clip-text text-transparent">
          Contato
        </h1>
        <p className="text-muted-foreground">
          Entre em contato conosco através das nossas redes sociais e canais de comunicação.
        </p>
      </div>

      <Card className="glass-card border-primary/20">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Heart className="h-5 w-5 text-primary" />
            Estamos aqui para ajudar!
          </CardTitle>
          <CardDescription>
            Tem dúvidas, sugestões ou quer apenas bater um papo? Escolha o canal que preferir para se conectar com a gente.
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

      <Card className="glass-card border-primary/20">
        <CardHeader>
          <CardTitle>Feedback e Sugestões</CardTitle>
          <CardDescription>
            Sua opinião é muito importante para nós! Estamos sempre buscando melhorar.
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
                <span>Participe da nossa comunidade no Discord</span>
              </li>
            </ul>
          </div>

          <div className="flex flex-col sm:flex-row gap-3">
            <Button
              variant="default"
              className="flex-1 hover-lift tap-scale"
              onClick={() => window.open('mailto:feedback@gymhero.app?subject=Feedback sobre o TaktIQ', '_blank')}
            >
              <Mail className="mr-2 h-4 w-4" />
              Enviar Feedback
            </Button>
            <Button
              variant="outline"
              className="flex-1 hover-lift tap-scale"
              onClick={() => window.open('https://github.com/gymhero/gymhero/issues', '_blank')}
            >
              <Github className="mr-2 h-4 w-4" />
              Reportar Bug
            </Button>
          </div>
        </CardContent>
      </Card>

      <Card className="glass-card border-primary/20">
        <CardContent className="p-6">
          <div className="text-center space-y-2">
            <p className="text-sm text-muted-foreground">
              Desenvolvido com <Heart className="inline h-4 w-4 text-red-500" /> pela equipe TaktIQ
            </p>
            <p className="text-xs text-muted-foreground">
              Versão 1.0.0 • {new Date().getFullYear()}
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
