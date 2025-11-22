"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { apiClient } from "@/lib/api";
import { toast } from "sonner";
import Link from "next/link";

interface UserData {
  id: string;
  name: string;
  email: string;
  injuries?: string | null;
  healthConditions?: string | null;
  exerciseGoal?: string | null;
  [key: string]: any;
}

export default function OnboardingPage() {
  const router = useRouter();
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState({
    injuries: "",
    healthConditions: "",
    exerciseGoal: "",
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);

    try {
      // Get current user data first
      const userData = await apiClient.get<UserData>("/me");

      // Update profile with onboarding data
      await apiClient.put("/me", {
        ...userData,
        injuries: formData.injuries || undefined,
        healthConditions: formData.healthConditions || undefined,
        exerciseGoal: formData.exerciseGoal || undefined,
      });

      toast.success("Perfil atualizado com sucesso!");
      router.push("/dashboard");
    } catch (error) {
      toast.error("Erro ao salvar informações. Tente novamente.");
    } finally {
      setIsLoading(false);
    }
  };

  const handleSkip = () => {
    router.push("/dashboard");
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-background to-muted p-4 sm:p-6 lg:p-8">
      <Card className="w-full max-w-2xl">
        <CardHeader className="space-y-2 sm:space-y-3">
          <CardTitle className="text-xl sm:text-2xl lg:text-3xl font-bold">Bem-vindo ao TaktIQ!</CardTitle>
          <CardDescription className="text-sm sm:text-base">
            Conte-nos um pouco sobre você para personalizarmos seus treinos.
            Essas informações nos ajudam a criar treinos mais seguros e eficazes.
          </CardDescription>
        </CardHeader>
        <CardContent className="px-4 sm:px-6">
          <form onSubmit={handleSubmit} className="space-y-4 sm:space-y-6">
            <div className="space-y-2">
              <Label htmlFor="injuries" className="text-sm sm:text-base">
                Lesões ou limitações físicas
                <span className="text-muted-foreground text-xs sm:text-sm ml-2">(opcional)</span>
              </Label>
              <Textarea
                id="injuries"
                placeholder="Ex: dor no joelho direito, problema no ombro esquerdo, hérnia de disco..."
                value={formData.injuries}
                onChange={(e) => setFormData({ ...formData, injuries: e.target.value })}
                rows={3}
                className="resize-none text-sm sm:text-base"
              />
              <p className="text-xs sm:text-sm text-muted-foreground">
                Informe qualquer lesão ou limitação física que devemos considerar ao criar seus treinos.
              </p>
            </div>

            <div className="space-y-2">
              <Label htmlFor="healthConditions" className="text-sm sm:text-base">
                Condições de saúde ou doenças
                <span className="text-muted-foreground text-xs sm:text-sm ml-2">(opcional)</span>
              </Label>
              <Textarea
                id="healthConditions"
                placeholder="Ex: diabetes, hipertensão, asma, problemas cardíacos..."
                value={formData.healthConditions}
                onChange={(e) => setFormData({ ...formData, healthConditions: e.target.value })}
                rows={3}
                className="resize-none text-sm sm:text-base"
              />
              <p className="text-xs sm:text-sm text-muted-foreground">
                Informe qualquer condição de saúde que possamos considerar ao planejar seus treinos.
              </p>
            </div>

            <div className="space-y-2">
              <Label htmlFor="exerciseGoal" className="text-sm sm:text-base">
                Qual é o seu objetivo principal?
                <span className="text-muted-foreground text-xs sm:text-sm ml-2">(opcional)</span>
              </Label>
              <Textarea
                id="exerciseGoal"
                placeholder="Ex: perder peso, ganhar massa muscular, melhorar condicionamento físico, reabilitação..."
                value={formData.exerciseGoal}
                onChange={(e) => setFormData({ ...formData, exerciseGoal: e.target.value })}
                rows={3}
                className="resize-none text-sm sm:text-base"
              />
              <p className="text-xs sm:text-sm text-muted-foreground">
                Conte-nos qual é o seu objetivo principal com os exercícios.
              </p>
            </div>

            <div className="flex flex-col sm:flex-row gap-3 pt-2">
              <Button
                type="submit"
                className="flex-1 h-10 sm:h-11 text-sm sm:text-base"
                disabled={isLoading}
              >
                {isLoading ? "Salvando..." : "Continuar"}
              </Button>
              <Button
                type="button"
                variant="outline"
                onClick={handleSkip}
                disabled={isLoading}
                className="flex-1 h-10 sm:h-11 text-sm sm:text-base"
              >
                Pular por agora
              </Button>
            </div>

            <p className="text-xs sm:text-sm text-center text-muted-foreground mt-4">
              Você pode adicionar ou editar essas informações mais tarde na página do seu perfil.
            </p>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
