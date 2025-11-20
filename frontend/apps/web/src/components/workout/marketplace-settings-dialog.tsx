'use client';

import { useState, useEffect } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { ShoppingCart, DollarSign, Info } from 'lucide-react';
import { apiClient } from '@/lib/api';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { useToast } from '@/components/ui/use-toast';
import { Checkbox } from '@/components/ui/checkbox';
import { Alert, AlertDescription } from '@/components/ui/alert';

interface MarketplaceSettingsDialogProps {
  planId: string;
  planName: string;
  currentForSale?: boolean;
  currentPrice?: number;
  isPublic?: boolean;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function MarketplaceSettingsDialog({
  planId,
  planName,
  currentForSale = false,
  currentPrice = 0,
  isPublic = false,
  open,
  onOpenChange,
}: MarketplaceSettingsDialogProps) {
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [forSale, setForSale] = useState(currentForSale);
  const [price, setPrice] = useState(currentPrice?.toString() || '0');

  useEffect(() => {
    setForSale(currentForSale);
    setPrice(currentPrice?.toString() || '0');
  }, [currentForSale, currentPrice]);

  const updateMarketplaceMutation = useMutation({
    mutationFn: async (data: { forSale: boolean; price: number | null }) => {
      return apiClient.patch(`/workout-plans/${planId}/marketplace`, {
        forSale: data.forSale,
        price: data.price,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workout-plans'] });
      toast({
        title: 'Configurações atualizadas!',
        description: 'As configurações do marketplace foram atualizadas com sucesso.',
      });
      onOpenChange(false);
    },
    onError: (error: any) => {
      const errorMessage = error?.response?.data?.message || 'Não foi possível atualizar as configurações.';
      toast({
        title: 'Erro',
        description: errorMessage,
        variant: 'destructive',
      });
    },
  });

  const handleSave = () => {
    const priceValue = parseFloat(price);

    if (forSale && (isNaN(priceValue) || priceValue < 0)) {
      toast({
        title: 'Preço inválido',
        description: 'Por favor, insira um preço válido.',
        variant: 'destructive',
      });
      return;
    }

    updateMarketplaceMutation.mutate({
      forSale,
      price: forSale && priceValue > 0 ? priceValue : null,
    });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <ShoppingCart className="h-5 w-5" />
            Configurações do Marketplace
          </DialogTitle>
          <DialogDescription>
            Configure se este plano estará disponível para venda no marketplace
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-6 py-4">
          {!isPublic && (
            <Alert className="border-yellow-500/50 bg-yellow-500/10">
              <Info className="h-4 w-4 text-yellow-500" />
              <AlertDescription className="text-yellow-500/80">
                Este plano deve ser público para ser vendido no marketplace. Configure a visibilidade primeiro.
              </AlertDescription>
            </Alert>
          )}

          <div className="flex items-start space-x-3 p-4 rounded-lg bg-muted/50">
            <Checkbox
              id="forSale"
              checked={forSale}
              onCheckedChange={(checked) => setForSale(checked as boolean)}
              disabled={!isPublic}
            />
            <div className="flex-1 space-y-1">
              <Label
                htmlFor="forSale"
                className="text-sm font-medium leading-none cursor-pointer"
              >
                Disponível para venda
              </Label>
              <p className="text-sm text-muted-foreground">
                Outros usuários poderão comprar este plano no marketplace
              </p>
            </div>
          </div>

          {forSale && (
            <div className="space-y-2">
              <Label htmlFor="price">Preço (R$)</Label>
              <div className="relative">
                <DollarSign className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                <Input
                  id="price"
                  type="number"
                  min="0"
                  step="0.01"
                  value={price}
                  onChange={(e) => setPrice(e.target.value)}
                  className="pl-10"
                  placeholder="0.00"
                />
              </div>
              <p className="text-xs text-muted-foreground">
                {parseFloat(price) === 0 || !price
                  ? 'Plano gratuito - usuários podem adicionar sem pagamento'
                  : `Os alunos pagarão R$ ${parseFloat(price).toFixed(2)} para acessar este plano`}
              </p>
            </div>
          )}

          <div className="space-y-2 p-4 rounded-lg bg-primary/5 border border-primary/20">
            <h4 className="text-sm font-medium flex items-center gap-2">
              <Info className="h-4 w-4" />
              Como funciona
            </h4>
            <ul className="text-xs text-muted-foreground space-y-1 ml-6 list-disc">
              <li>Planos gratuitos (R$ 0,00) podem ser adicionados diretamente pelos alunos</li>
              <li>Planos pagos requerem pagamento via Stripe antes do acesso</li>
              <li>Você recebe {(100 - 10).toFixed(0)}% do valor (taxa de plataforma: 10%)</li>
              <li>O plano deve estar público para aparecer no marketplace</li>
            </ul>
          </div>
        </div>

        <DialogFooter>
          <Button
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={updateMarketplaceMutation.isPending}
          >
            Cancelar
          </Button>
          <Button
            onClick={handleSave}
            disabled={updateMarketplaceMutation.isPending || !isPublic}
          >
            {updateMarketplaceMutation.isPending ? 'Salvando...' : 'Salvar'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
