'use client';

import { useState, useEffect } from 'react';
import { loadStripe, Stripe } from '@stripe/stripe-js';
import {
  Elements,
  PaymentElement,
  useStripe,
  useElements,
} from '@stripe/react-stripe-js';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Loader2, CreditCard, CheckCircle2, AlertCircle } from 'lucide-react';
import { apiClient } from '@/lib/api';

// Initialize Stripe - you'll need to set NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY in .env
// Only load Stripe if the key is available
const stripePromise = process.env.NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY
  ? loadStripe(process.env.NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY)
  : null;

interface StripeCheckoutProps {
  planId: string;
  planName: string;
  planPrice: number;
  onSuccess: () => void;
  onCancel: () => void;
}

interface CheckoutFormProps extends StripeCheckoutProps {
  paymentIntentId: string;
}

function CheckoutForm({
  planId,
  planName,
  planPrice,
  paymentIntentId,
  onSuccess,
  onCancel,
}: CheckoutFormProps) {
  const stripe = useStripe();
  const elements = useElements();
  const [isProcessing, setIsProcessing] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [paymentSucceeded, setPaymentSucceeded] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!stripe || !elements) {
      return;
    }

    setIsProcessing(true);
    setErrorMessage(null);

    try {
      // Submit the payment
      const { error: submitError } = await elements.submit();
      if (submitError) {
        setErrorMessage(submitError.message || 'Erro ao processar pagamento');
        setIsProcessing(false);
        return;
      }

      // Confirm the payment
      const { error } = await stripe.confirmPayment({
        elements,
        redirect: 'if_required',
      });

      if (error) {
        setErrorMessage(error.message || 'Erro ao confirmar pagamento');
        setIsProcessing(false);
      } else {
        // Payment succeeded
        setPaymentSucceeded(true);

        // Confirm with backend to clone the plan
        try {
          await apiClient.post('/payments/confirm', {
            paymentIntentId: paymentIntentId,
            workoutPlanId: planId,
          });

          setTimeout(() => {
            onSuccess();
          }, 2000);
        } catch (backendError: any) {
          setErrorMessage('Pagamento confirmado, mas houve um erro ao processar o plano. Entre em contato com o suporte.');
          setIsProcessing(false);
        }
      }
    } catch (error: any) {
      setErrorMessage('Erro inesperado ao processar pagamento');
      setIsProcessing(false);
    }
  };

  if (paymentSucceeded) {
    return (
      <div className="text-center py-8">
        <CheckCircle2 className="h-16 w-16 text-green-500 mx-auto mb-4 animate-scale-in" />
        <h3 className="text-xl font-bold mb-2">Pagamento Confirmado!</h3>
        <p className="text-muted-foreground">
          O plano está sendo adicionado à sua conta...
        </p>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {/* Plan Summary */}
      <div className="glass rounded-lg p-4 border">
        <h3 className="font-semibold mb-2">Resumo do Pedido</h3>
        <div className="flex justify-between items-center">
          <span className="text-muted-foreground">{planName}</span>
          <span className="text-xl font-bold text-primary">
            R$ {planPrice.toFixed(2)}
          </span>
        </div>
      </div>

      {/* Payment Element */}
      <div className="glass rounded-lg p-4 border">
        <PaymentElement />
      </div>

      {/* Error Message */}
      {errorMessage && (
        <div className="flex items-start gap-3 p-4 bg-destructive/10 border border-destructive/30 rounded-lg">
          <AlertCircle className="h-5 w-5 text-destructive flex-shrink-0 mt-0.5" />
          <p className="text-sm text-destructive">{errorMessage}</p>
        </div>
      )}

      {/* Action Buttons */}
      <div className="flex gap-3">
        <Button
          type="button"
          variant="outline"
          onClick={onCancel}
          disabled={isProcessing}
          className="flex-1"
        >
          Cancelar
        </Button>
        <Button
          type="submit"
          disabled={!stripe || isProcessing}
          className="flex-1 bg-gradient-to-r from-primary to-primary/80 hover:from-primary/90 hover:to-primary/70"
        >
          {isProcessing ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              Processando...
            </>
          ) : (
            <>
              <CreditCard className="mr-2 h-4 w-4" />
              Pagar R$ {planPrice.toFixed(2)}
            </>
          )}
        </Button>
      </div>
    </form>
  );
}

export function StripeCheckout({
  planId,
  planName,
  planPrice,
  onSuccess,
  onCancel,
}: StripeCheckoutProps) {
  const [clientSecret, setClientSecret] = useState<string | null>(null);
  const [paymentIntentId, setPaymentIntentId] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    // Create payment intent when component mounts
    const createPaymentIntent = async () => {
      try {
        const response = await apiClient.post<{
          clientSecret: string;
          paymentIntentId: string;
          amount: number;
          currency: string;
        }>('/payments/create-intent', {
          workoutPlanId: planId,
        });

        setClientSecret(response.clientSecret);
        setPaymentIntentId(response.paymentIntentId);
        setIsLoading(false);
      } catch (error: any) {
        setError(
          error?.response?.data?.message ||
            'Não foi possível iniciar o pagamento. Tente novamente.'
        );
        setIsLoading(false);
      }
    };

    createPaymentIntent();
  }, [planId]);

  if (isLoading) {
    return (
      <Card className="glass border-primary/20 p-8">
        <div className="text-center">
          <Loader2 className="h-12 w-12 text-primary animate-spin mx-auto mb-4" />
          <p className="text-muted-foreground">Preparando pagamento...</p>
        </div>
      </Card>
    );
  }

  if (error) {
    return (
      <Card className="glass border-destructive/30 bg-destructive/5 p-8">
        <div className="text-center">
          <AlertCircle className="h-12 w-12 text-destructive mx-auto mb-4" />
          <h3 className="font-semibold mb-2">Erro ao Iniciar Pagamento</h3>
          <p className="text-sm text-muted-foreground mb-4">{error}</p>
          <Button variant="outline" onClick={onCancel}>
            Voltar
          </Button>
        </div>
      </Card>
    );
  }

  if (!clientSecret || !paymentIntentId) {
    return (
      <Card className="glass border-destructive/30 bg-destructive/5 p-8">
        <div className="text-center">
          <AlertCircle className="h-12 w-12 text-destructive mx-auto mb-4" />
          <p className="text-muted-foreground">
            Não foi possível iniciar o pagamento.
          </p>
          <Button variant="outline" onClick={onCancel} className="mt-4">
            Voltar
          </Button>
        </div>
      </Card>
    );
  }

  // Check if Stripe is configured
  if (!stripePromise) {
    return (
      <Card className="glass border-destructive/30 bg-destructive/5 p-8">
        <div className="text-center">
          <AlertCircle className="h-12 w-12 text-destructive mx-auto mb-4" />
          <h3 className="font-semibold mb-2">Pagamentos Não Configurados</h3>
          <p className="text-sm text-muted-foreground mb-4">
            O sistema de pagamentos não está configurado. Entre em contato com o suporte.
          </p>
          <Button variant="outline" onClick={onCancel}>
            Voltar
          </Button>
        </div>
      </Card>
    );
  }

  const options = {
    clientSecret,
    appearance: {
      theme: 'night' as const,
      variables: {
        colorPrimary: '#10b981',
        colorBackground: '#0a0a0a',
        colorText: '#ffffff',
        colorDanger: '#ef4444',
        fontFamily: 'system-ui, sans-serif',
        borderRadius: '8px',
      },
    },
  };

  return (
    <Card className="glass border-primary/20 p-6">
      <div className="mb-6">
        <h2 className="text-2xl font-bold mb-2">Pagamento Seguro</h2>
        <p className="text-sm text-muted-foreground">
          Processado com segurança pelo Stripe
        </p>
      </div>

      <Elements stripe={stripePromise} options={options}>
        <CheckoutForm
          planId={planId}
          planName={planName}
          planPrice={planPrice}
          paymentIntentId={paymentIntentId}
          onSuccess={onSuccess}
          onCancel={onCancel}
        />
      </Elements>
    </Card>
  );
}
