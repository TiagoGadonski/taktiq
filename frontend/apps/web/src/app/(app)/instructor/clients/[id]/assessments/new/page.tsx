'use client';

import { useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { apiClient } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { useToast } from '@/hooks/use-toast';
import { ArrowLeft, Plus, X, Save } from 'lucide-react';

type SeverityLevel = 'None' | 'Mild' | 'Moderate' | 'Severe';
type AssessmentType = 'Postural' | 'Physical' | 'Custom';

interface CustomField {
  fieldName: string;
  fieldValue: string;
  fieldType: 'text' | 'number' | 'select';
}

export default function NewAssessmentPage() {
  const params = useParams();
  const router = useRouter();
  const { toast } = useToast();
  const studentId = params?.id as string;

  const [assessmentType, setAssessmentType] = useState<AssessmentType>('Postural');
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Postural fields
  const [forwardHead, setForwardHead] = useState<SeverityLevel>('None');
  const [roundedShoulders, setRoundedShoulders] = useState<SeverityLevel>('None');
  const [anteriorPelvicTilt, setAnteriorPelvicTilt] = useState<SeverityLevel>('None');
  const [posteriorPelvicTilt, setPosteriorPelvicTilt] = useState<SeverityLevel>('None');
  const [kneeValgus, setKneeValgus] = useState<SeverityLevel>('None');
  const [kneeVarus, setKneeVarus] = useState<SeverityLevel>('None');
  const [flatFeet, setFlatFeet] = useState<SeverityLevel>('None');
  const [scoliosis, setScoliosis] = useState<SeverityLevel>('None');

  // Physical fields
  const [bodyFatPercentage, setBodyFatPercentage] = useState('');
  const [muscleMass, setMuscleMass] = useState('');
  const [flexibilityScore, setFlexibilityScore] = useState('');
  const [strengthScore, setStrengthScore] = useState('');
  const [cardioScore, setCardioScore] = useState('');

  // Common fields
  const [trainerNotes, setTrainerNotes] = useState('');
  const [customFields, setCustomFields] = useState<CustomField[]>([]);

  const addCustomField = () => {
    setCustomFields([...customFields, { fieldName: '', fieldValue: '', fieldType: 'text' }]);
  };

  const removeCustomField = (index: number) => {
    setCustomFields(customFields.filter((_, i) => i !== index));
  };

  const updateCustomField = (index: number, field: Partial<CustomField>) => {
    const updated = [...customFields];
    updated[index] = { ...updated[index], ...field };
    setCustomFields(updated);
  };

  const handleSubmit = async () => {
    setIsSubmitting(true);
    try {
      const payload: any = {
        studentId,
        assessmentType,
        trainerNotes: trainerNotes.trim() || undefined,
        customFields: customFields.filter(f => f.fieldName && f.fieldValue).length > 0
          ? customFields.filter(f => f.fieldName && f.fieldValue)
          : undefined
      };

      if (assessmentType === 'Postural') {
        payload.forwardHead = forwardHead !== 'None' ? forwardHead : undefined;
        payload.roundedShoulders = roundedShoulders !== 'None' ? roundedShoulders : undefined;
        payload.anteriorPelvicTilt = anteriorPelvicTilt !== 'None' ? anteriorPelvicTilt : undefined;
        payload.posteriorPelvicTilt = posteriorPelvicTilt !== 'None' ? posteriorPelvicTilt : undefined;
        payload.kneeValgus = kneeValgus !== 'None' ? kneeValgus : undefined;
        payload.kneeVarus = kneeVarus !== 'None' ? kneeVarus : undefined;
        payload.flatFeet = flatFeet !== 'None' ? flatFeet : undefined;
        payload.scoliosis = scoliosis !== 'None' ? scoliosis : undefined;
      } else if (assessmentType === 'Physical') {
        if (bodyFatPercentage) payload.bodyFatPercentage = parseFloat(bodyFatPercentage);
        if (muscleMass) payload.muscleMass = parseFloat(muscleMass);
        if (flexibilityScore) payload.flexibilityScore = parseFloat(flexibilityScore);
        if (strengthScore) payload.strengthScore = parseFloat(strengthScore);
        if (cardioScore) payload.cardioScore = parseFloat(cardioScore);
      }

      await apiClient.post('/assessments', payload);

      toast({
        title: 'Avaliação criada!',
        description: 'A avaliação foi salva com sucesso.',
      });

      router.push(`/instructor/clients/${studentId}/assessments`);
    } catch (error: any) {
      toast({
        title: 'Erro',
        description: error.response?.data?.message || 'Não foi possível criar a avaliação.',
        variant: 'destructive',
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  const SeveritySelect = ({
    value,
    onChange,
    label
  }: {
    value: SeverityLevel;
    onChange: (value: SeverityLevel) => void;
    label: string;
  }) => (
    <div className="space-y-2">
      <Label>{label}</Label>
      <Select value={value} onValueChange={(v) => onChange(v as SeverityLevel)}>
        <SelectTrigger>
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="None">Ausente</SelectItem>
          <SelectItem value="Mild">Leve</SelectItem>
          <SelectItem value="Moderate">Moderado</SelectItem>
          <SelectItem value="Severe">Severo</SelectItem>
        </SelectContent>
      </Select>
    </div>
  );

  const ScoreInput = ({
    value,
    onChange,
    label,
    placeholder
  }: {
    value: string;
    onChange: (value: string) => void;
    label: string;
    placeholder: string;
  }) => (
    <div className="space-y-2">
      <Label>{label}</Label>
      <Input
        type="number"
        step="0.1"
        min="0"
        max="10"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
      />
    </div>
  );

  return (
    <div className="container mx-auto p-6 max-w-4xl space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => router.back()}>
          <ArrowLeft className="h-5 w-5" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold">Nova Avaliação</h1>
          <p className="text-muted-foreground">Criar avaliação para o aluno</p>
        </div>
      </div>

      {/* Assessment Type Tabs */}
      <Tabs value={assessmentType} onValueChange={(v) => setAssessmentType(v as AssessmentType)}>
        <TabsList className="grid w-full grid-cols-3">
          <TabsTrigger value="Postural">Avaliação Postural</TabsTrigger>
          <TabsTrigger value="Physical">Avaliação Física</TabsTrigger>
          <TabsTrigger value="Custom">Personalizada</TabsTrigger>
        </TabsList>

        {/* Postural Assessment */}
        <TabsContent value="Postural" className="space-y-6 mt-6">
          <Card>
            <CardHeader>
              <CardTitle>Avaliação Postural</CardTitle>
              <CardDescription>
                Identifique desvios posturais para personalizar os treinos
              </CardDescription>
            </CardHeader>
            <CardContent className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <SeveritySelect
                label="Cabeça Anteriorizada"
                value={forwardHead}
                onChange={setForwardHead}
              />
              <SeveritySelect
                label="Ombros Protusos"
                value={roundedShoulders}
                onChange={setRoundedShoulders}
              />
              <SeveritySelect
                label="Inclinação Pélvica Anterior"
                value={anteriorPelvicTilt}
                onChange={setAnteriorPelvicTilt}
              />
              <SeveritySelect
                label="Inclinação Pélvica Posterior"
                value={posteriorPelvicTilt}
                onChange={setPosteriorPelvicTilt}
              />
              <SeveritySelect
                label="Joelhos Valgos (para dentro)"
                value={kneeValgus}
                onChange={setKneeValgus}
              />
              <SeveritySelect
                label="Joelhos Varos (para fora)"
                value={kneeVarus}
                onChange={setKneeVarus}
              />
              <SeveritySelect
                label="Pés Planos"
                value={flatFeet}
                onChange={setFlatFeet}
              />
              <SeveritySelect
                label="Escoliose"
                value={scoliosis}
                onChange={setScoliosis}
              />
            </CardContent>
          </Card>
        </TabsContent>

        {/* Physical Assessment */}
        <TabsContent value="Physical" className="space-y-6 mt-6">
          <Card>
            <CardHeader>
              <CardTitle>Avaliação Física</CardTitle>
              <CardDescription>
                Medições e scores de condicionamento físico
              </CardDescription>
            </CardHeader>
            <CardContent className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Percentual de Gordura (%)</Label>
                <Input
                  type="number"
                  step="0.1"
                  min="0"
                  max="100"
                  value={bodyFatPercentage}
                  onChange={(e) => setBodyFatPercentage(e.target.value)}
                  placeholder="Ex: 15.5"
                />
              </div>
              <div className="space-y-2">
                <Label>Massa Muscular (kg)</Label>
                <Input
                  type="number"
                  step="0.1"
                  min="0"
                  value={muscleMass}
                  onChange={(e) => setMuscleMass(e.target.value)}
                  placeholder="Ex: 60.0"
                />
              </div>
              <ScoreInput
                label="Score de Flexibilidade (0-10)"
                value={flexibilityScore}
                onChange={setFlexibilityScore}
                placeholder="Ex: 7.5"
              />
              <ScoreInput
                label="Score de Força (0-10)"
                value={strengthScore}
                onChange={setStrengthScore}
                placeholder="Ex: 8.0"
              />
              <ScoreInput
                label="Score de Cardio (0-10)"
                value={cardioScore}
                onChange={setCardioScore}
                placeholder="Ex: 6.5"
              />
            </CardContent>
          </Card>
        </TabsContent>

        {/* Custom Assessment */}
        <TabsContent value="Custom" className="space-y-6 mt-6">
          <Card>
            <CardHeader>
              <CardTitle>Avaliação Personalizada</CardTitle>
              <CardDescription>
                Crie campos customizados para avaliações específicas
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              {customFields.map((field, index) => (
                <div key={index} className="flex gap-3 items-end">
                  <div className="flex-1 space-y-2">
                    <Label>Nome do Campo</Label>
                    <Input
                      value={field.fieldName}
                      onChange={(e) => updateCustomField(index, { fieldName: e.target.value })}
                      placeholder="Ex: Equilíbrio"
                    />
                  </div>
                  <div className="flex-1 space-y-2">
                    <Label>Valor</Label>
                    <Input
                      value={field.fieldValue}
                      onChange={(e) => updateCustomField(index, { fieldValue: e.target.value })}
                      placeholder="Ex: Bom"
                    />
                  </div>
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => removeCustomField(index)}
                  >
                    <X className="h-4 w-4" />
                  </Button>
                </div>
              ))}

              <Button
                type="button"
                variant="outline"
                onClick={addCustomField}
                className="w-full"
              >
                <Plus className="h-4 w-4 mr-2" />
                Adicionar Campo
              </Button>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Trainer Notes (Common to all types) */}
      <Card>
        <CardHeader>
          <CardTitle>Observações do PT (Privado)</CardTitle>
          <CardDescription>
            Estas anotações são privadas e não serão compartilhadas com o aluno
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Textarea
            value={trainerNotes}
            onChange={(e) => setTrainerNotes(e.target.value)}
            placeholder="Observações, recomendações, pontos de atenção..."
            className="min-h-[120px] resize-none"
            maxLength={1000}
          />
          <p className="text-xs text-muted-foreground mt-2 text-right">
            {trainerNotes.length}/1000 caracteres
          </p>
        </CardContent>
      </Card>

      {/* Actions */}
      <div className="flex justify-end gap-3">
        <Button variant="outline" onClick={() => router.back()} disabled={isSubmitting}>
          Cancelar
        </Button>
        <Button onClick={handleSubmit} disabled={isSubmitting}>
          <Save className="h-4 w-4 mr-2" />
          {isSubmitting ? 'Salvando...' : 'Salvar Avaliação'}
        </Button>
      </div>
    </div>
  );
}
