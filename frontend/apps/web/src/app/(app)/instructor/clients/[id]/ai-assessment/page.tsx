'use client';

import { useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { toast } from '@/components/ui/use-toast';
import { Sparkles, Loader2, Camera, AlertCircle, CheckCircle2 } from 'lucide-react';
import { apiClient } from '@/lib/api';
import { Alert, AlertDescription } from '@/components/ui/alert';

interface PosturalAnalysis {
  forwardHead: string;
  roundedShoulders: string;
  anteriorPelvicTilt: string;
  posteriorPelvicTilt: string;
  kneeValgus: string;
  kneeVarus: string;
  scoliosis: string;
  flatFeet: string;
  observations: string;
  recommendations: string;
}

const severityColors = {
  None: 'bg-green-100 text-green-800 border-green-300',
  Mild: 'bg-yellow-100 text-yellow-800 border-yellow-300',
  Moderate: 'bg-orange-100 text-orange-800 border-orange-300',
  Severe: 'bg-red-100 text-red-800 border-red-300',
};

const severityLabels = {
  None: 'Normal',
  Mild: 'Leve',
  Moderate: 'Moderado',
  Severe: 'Severo',
};

function DeviationItem({ label, value }: { label: string; value: string }) {
  const colorClass = severityColors[value as keyof typeof severityColors] || 'bg-gray-100 text-gray-800';
  const labelText = severityLabels[value as keyof typeof severityLabels] || value;

  return (
    <div className="flex items-center justify-between p-3 border rounded-lg">
      <span className="font-medium">{label}</span>
      <span className={`px-3 py-1 rounded-full text-sm font-medium border ${colorClass}`}>
        {labelText}
      </span>
    </div>
  );
}

export default function AIPosturalAssessment() {
  const params = useParams();
  const router = useRouter();
  const clientId = params.id as string;

  const [frontPhoto, setFrontPhoto] = useState<File | null>(null);
  const [sidePhoto, setSidePhoto] = useState<File | null>(null);
  const [backPhoto, setBackPhoto] = useState<File | null>(null);
  const [analysis, setAnalysis] = useState<PosturalAnalysis | null>(null);
  const [isAnalyzing, setIsAnalyzing] = useState(false);

  const handleFileChange = (setter: React.Dispatch<React.SetStateAction<File | null>>) => (
    e: React.ChangeEvent<HTMLInputElement>
  ) => {
    const file = e.target.files?.[0];
    if (file) {
      // Validate file size (max 5MB)
      if (file.size > 5 * 1024 * 1024) {
        toast({
          title: 'Arquivo muito grande',
          description: 'Cada foto deve ter no máximo 5MB',
          variant: 'destructive',
        });
        return;
      }
      setter(file);
    }
  };

  const handleAnalyze = async () => {
    if (!frontPhoto || !sidePhoto || !backPhoto) {
      toast({
        title: 'Fotos incompletas',
        description: 'Por favor, envie as 3 fotos necessárias (frontal, lateral e costas)',
        variant: 'destructive',
      });
      return;
    }

    setIsAnalyzing(true);
    const formData = new FormData();
    formData.append('frontPhoto', frontPhoto);
    formData.append('sidePhoto', sidePhoto);
    formData.append('backPhoto', backPhoto);

    try {
      const result = await apiClient.post<PosturalAnalysis>('/ai/analyze-posture', formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });

      setAnalysis(result);
      toast({
        title: 'Análise concluída!',
        description: 'Revise os resultados abaixo antes de salvar',
      });
    } catch (error: any) {
      toast({
        title: 'Erro na análise',
        description: error.response?.data?.message || 'Falha ao analisar fotos. Tente novamente.',
        variant: 'destructive',
      });
    } finally {
      setIsAnalyzing(false);
    }
  };

  const handleSaveAssessment = async () => {
    if (!analysis) return;

    try {
      // Save assessment to database
      await apiClient.post('/assessments', {
        studentId: clientId,
        assessmentType: 'Postural',
        assessmentDate: new Date().toISOString(),
        isActive: true,
        forwardHead: analysis.forwardHead,
        roundedShoulders: analysis.roundedShoulders,
        anteriorPelvicTilt: analysis.anteriorPelvicTilt,
        posteriorPelvicTilt: analysis.posteriorPelvicTilt,
        kneeValgus: analysis.kneeValgus,
        kneeVarus: analysis.kneeVarus,
        scoliosis: analysis.scoliosis,
        flatFeet: analysis.flatFeet,
        trainerNotes: `ANÁLISE POR IA:\n\nObservações: ${analysis.observations}\n\nRecomendações: ${analysis.recommendations}`,
      });

      toast({
        title: 'Avaliação salva!',
        description: 'A avaliação postural foi salva com sucesso',
      });

      // Redirect to assessments list
      router.push(`/instructor/clients/${clientId}/assessments`);
    } catch (error) {
      toast({
        title: 'Erro ao salvar',
        description: 'Não foi possível salvar a avaliação. Tente novamente.',
        variant: 'destructive',
      });
    }
  };

  return (
    <div className="container max-w-5xl py-8 space-y-6">
      {/* Header */}
      <div className="flex items-center gap-3">
        <div className="p-3 bg-primary/10 rounded-lg">
          <Sparkles className="h-6 w-6 text-primary" />
        </div>
        <div>
          <h1 className="text-3xl font-bold">Avaliação Postural por IA</h1>
          <p className="text-muted-foreground">Análise automática usando GPT-4 Vision</p>
        </div>
      </div>

      {/* Instructions Card */}
      <Card className="border-blue-200 bg-blue-50/50 dark:bg-blue-950/10">
        <CardHeader>
          <CardTitle className="text-sm flex items-center gap-2">
            <Camera className="h-4 w-4" />
            Instruções para Fotos
          </CardTitle>
        </CardHeader>
        <CardContent className="text-sm space-y-2">
          <p><CheckCircle2 className="h-4 w-4 inline mr-2 text-green-600" />Aluno com roupa justa ou sem camisa (homens)</p>
          <p><CheckCircle2 className="h-4 w-4 inline mr-2 text-green-600" />Fundo neutro (parede branca ou clara)</p>
          <p><CheckCircle2 className="h-4 w-4 inline mr-2 text-green-600" />Boa iluminação natural ou artificial</p>
          <p><CheckCircle2 className="h-4 w-4 inline mr-2 text-green-600" />Aluno em postura natural, relaxada (não forçada)</p>
          <p><CheckCircle2 className="h-4 w-4 inline mr-2 text-green-600" />Corpo inteiro visível da cabeça aos pés</p>
          <p><CheckCircle2 className="h-4 w-4 inline mr-2 text-green-600" />Câmera na altura do quadril do aluno</p>
          <p><CheckCircle2 className="h-4 w-4 inline mr-2 text-green-600" />Fotos de 3 ângulos: frontal, lateral direita e posterior</p>
        </CardContent>
      </Card>

      {/* Photo Upload Grid */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {/* Front Photo */}
        <Card>
          <CardHeader>
            <CardTitle className="text-sm">Foto Frontal</CardTitle>
            <CardDescription>Vista de frente</CardDescription>
          </CardHeader>
          <CardContent className="space-y-3">
            <Input
              type="file"
              accept="image/jpeg,image/jpg,image/png"
              onChange={handleFileChange(setFrontPhoto)}
              className="cursor-pointer"
            />
            {frontPhoto && (
              <div className="relative">
                <img
                  src={URL.createObjectURL(frontPhoto)}
                  alt="Preview frontal"
                  className="w-full h-48 object-cover rounded border-2 border-green-500"
                />
                <div className="absolute top-2 right-2 bg-green-500 text-white px-2 py-1 rounded text-xs font-medium">
                  ✓ Carregada
                </div>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Side Photo */}
        <Card>
          <CardHeader>
            <CardTitle className="text-sm">Foto Lateral</CardTitle>
            <CardDescription>Vista de perfil (lado direito)</CardDescription>
          </CardHeader>
          <CardContent className="space-y-3">
            <Input
              type="file"
              accept="image/jpeg,image/jpg,image/png"
              onChange={handleFileChange(setSidePhoto)}
              className="cursor-pointer"
            />
            {sidePhoto && (
              <div className="relative">
                <img
                  src={URL.createObjectURL(sidePhoto)}
                  alt="Preview lateral"
                  className="w-full h-48 object-cover rounded border-2 border-green-500"
                />
                <div className="absolute top-2 right-2 bg-green-500 text-white px-2 py-1 rounded text-xs font-medium">
                  ✓ Carregada
                </div>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Back Photo */}
        <Card>
          <CardHeader>
            <CardTitle className="text-sm">Foto de Costas</CardTitle>
            <CardDescription>Vista posterior</CardDescription>
          </CardHeader>
          <CardContent className="space-y-3">
            <Input
              type="file"
              accept="image/jpeg,image/jpg,image/png"
              onChange={handleFileChange(setBackPhoto)}
              className="cursor-pointer"
            />
            {backPhoto && (
              <div className="relative">
                <img
                  src={URL.createObjectURL(backPhoto)}
                  alt="Preview costas"
                  className="w-full h-48 object-cover rounded border-2 border-green-500"
                />
                <div className="absolute top-2 right-2 bg-green-500 text-white px-2 py-1 rounded text-xs font-medium">
                  ✓ Carregada
                </div>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Analyze Button */}
      <Button
        onClick={handleAnalyze}
        disabled={!frontPhoto || !sidePhoto || !backPhoto || isAnalyzing}
        className="w-full"
        size="lg"
      >
        {isAnalyzing ? (
          <>
            <Loader2 className="mr-2 h-5 w-5 animate-spin" />
            Analisando fotos com IA... (pode levar até 30 segundos)
          </>
        ) : (
          <>
            <Sparkles className="mr-2 h-5 w-5" />
            Analisar Postura com IA
          </>
        )}
      </Button>

      {/* Analysis Results */}
      {analysis && (
        <Card className="border-2 border-primary">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Sparkles className="h-5 w-5 text-primary" />
              Resultado da Análise (IA)
            </CardTitle>
            <CardDescription>
              Revise os resultados e ajuste se necessário antes de salvar
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-6">
            {/* Deviations Grid */}
            <div className="space-y-3">
              <h3 className="font-semibold text-lg">Desvios Posturais Identificados</h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                <DeviationItem label="Cabeça Anteriorizada" value={analysis.forwardHead} />
                <DeviationItem label="Ombros Protusos" value={analysis.roundedShoulders} />
                <DeviationItem label="Inclinação Pélvica Anterior" value={analysis.anteriorPelvicTilt} />
                <DeviationItem label="Inclinação Pélvica Posterior" value={analysis.posteriorPelvicTilt} />
                <DeviationItem label="Joelhos Valgos (X)" value={analysis.kneeValgus} />
                <DeviationItem label="Joelhos Varos (Parênteses)" value={analysis.kneeVarus} />
                <DeviationItem label="Escoliose" value={analysis.scoliosis} />
                <DeviationItem label="Pés Planos" value={analysis.flatFeet} />
              </div>
            </div>

            {/* Observations */}
            <div className="space-y-2">
              <Label className="text-base font-semibold">Observações da IA</Label>
              <Alert>
                <AlertCircle className="h-4 w-4" />
                <AlertDescription className="text-sm whitespace-pre-wrap">
                  {analysis.observations}
                </AlertDescription>
              </Alert>
            </div>

            {/* Recommendations */}
            <div className="space-y-2">
              <Label className="text-base font-semibold">Recomendações de Exercícios Corretivos</Label>
              <Alert className="border-green-200 bg-green-50 dark:bg-green-950/10">
                <CheckCircle2 className="h-4 w-4 text-green-600" />
                <AlertDescription className="text-sm whitespace-pre-wrap">
                  {analysis.recommendations}
                </AlertDescription>
              </Alert>
            </div>

            {/* Action Buttons */}
            <div className="flex gap-3 pt-4">
              <Button
                variant="outline"
                onClick={() => setAnalysis(null)}
                className="flex-1"
              >
                Refazer Análise
              </Button>
              <Button
                onClick={handleSaveAssessment}
                className="flex-1"
              >
                <CheckCircle2 className="mr-2 h-4 w-4" />
                Salvar Avaliação
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Cost Info */}
      <Alert className="border-yellow-200 bg-yellow-50 dark:bg-yellow-950/10">
        <AlertCircle className="h-4 w-4 text-yellow-600" />
        <AlertDescription className="text-sm">
          <strong>Custo:</strong> Cada análise custa aproximadamente R$ 0,05 (usando GPT-4 Vision).
          Use com responsabilidade.
        </AlertDescription>
      </Alert>
    </div>
  );
}
