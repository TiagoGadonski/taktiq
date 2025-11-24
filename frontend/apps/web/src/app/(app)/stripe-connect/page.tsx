"use client";

import { useState, useEffect } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useRouter, useSearchParams } from "next/navigation";
import { apiClient } from "@/lib/api";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { useToast } from "@/components/ui/use-toast";
import { CheckCircle, XCircle, AlertCircle, Loader2, ExternalLink } from "lucide-react";
import {
  Alert,
  AlertDescription,
  AlertTitle,
} from "@/components/ui/alert";

interface StripeAccountStatus {
  connected: boolean;
  accountId?: string;
  chargesEnabled: boolean;
  payoutsEnabled: boolean;
  detailsSubmitted: boolean;
  requirements?: string[];
  pendingVerification?: string[];
}

interface StripeOnboardingResponse {
  url: string;
}

export default function StripeConnectPage() {
  const { toast } = useToast();
  const router = useRouter();
  const searchParams = useSearchParams();
  const queryClient = useQueryClient();
  const [connectingToStripe, setConnectingToStripe] = useState(false);

  const isRefresh = searchParams.get("refresh") === "true";

  // Fetch account status
  const { data: status, isLoading } = useQuery<StripeAccountStatus>({
    queryKey: ["stripe-connect-status"],
    queryFn: async () => {
      return await apiClient.get("/api/stripe/connect/status") as StripeAccountStatus;
    },
  });

  // Show success message if returning from Stripe
  useEffect(() => {
    if (isRefresh) {
      toast({
        title: "Returning from Stripe",
        description: "Checking your account status...",
      });
      queryClient.invalidateQueries({ queryKey: ["stripe-connect-status"] });
    }
  }, [isRefresh, toast, queryClient]);

  // Connect account mutation
  const connectAccount = useMutation<StripeOnboardingResponse>({
    mutationFn: async () => {
      setConnectingToStripe(true);
      return await apiClient.post("/api/stripe/connect/create-account") as StripeOnboardingResponse;
    },
    onSuccess: (data) => {
      // Redirect to Stripe onboarding
      window.location.href = data.url;
    },
    onError: (error: any) => {
      setConnectingToStripe(false);
      toast({
        title: "Error",
        description: error.response?.data?.message || "Failed to connect Stripe account",
        variant: "destructive",
      });
    },
  });

  // Refresh onboarding URL
  const refreshUrl = useMutation<StripeOnboardingResponse>({
    mutationFn: async () => {
      return await apiClient.get("/api/stripe/connect/refresh-url") as StripeOnboardingResponse;
    },
    onSuccess: (data) => {
      window.location.href = data.url;
    },
    onError: (error: any) => {
      toast({
        title: "Error",
        description: error.response?.data?.message || "Failed to refresh onboarding URL",
        variant: "destructive",
      });
    },
  });

  // Disconnect account mutation
  const disconnectAccount = useMutation({
    mutationFn: async () => {
      return await apiClient.post("/api/stripe/connect/disconnect");
    },
    onSuccess: () => {
      toast({
        title: "Account Disconnected",
        description: "Your Stripe account has been disconnected successfully",
      });
      queryClient.invalidateQueries({ queryKey: ["stripe-connect-status"] });
    },
    onError: (error: any) => {
      toast({
        title: "Error",
        description: error.response?.data?.message || "Failed to disconnect account",
        variant: "destructive",
      });
    },
  });

  const handleConnect = () => {
    connectAccount.mutate();
  };

  const handleContinueOnboarding = () => {
    refreshUrl.mutate();
  };

  const handleDisconnect = () => {
    if (
      window.confirm(
        "Are you sure you want to disconnect your Stripe account? You won't be able to receive payments until you reconnect."
      )
    ) {
      disconnectAccount.mutate();
    }
  };

  if (isLoading) {
    return (
      <div className="container mx-auto py-8">
        <div className="flex items-center justify-center min-h-[400px]">
          <Loader2 className="h-8 w-8 animate-spin" />
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-8 max-w-4xl space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Stripe Connect</h1>
        <p className="text-muted-foreground">
          Connect your Stripe account to receive payments from workout plan sales
        </p>
      </div>

      {/* Account Status Card */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>Account Status</CardTitle>
            {status?.connected ? (
              <Badge className="bg-green-500">
                <CheckCircle className="h-3 w-3 mr-1" />
                Connected
              </Badge>
            ) : (
              <Badge variant="outline">
                <XCircle className="h-3 w-3 mr-1" />
                Not Connected
              </Badge>
            )}
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          {!status?.connected ? (
            <>
              <p className="text-muted-foreground">
                Connect your Stripe account to start receiving payments from students who
                purchase your workout plans.
              </p>
              <div className="space-y-2">
                <h3 className="font-semibold">What you&apos;ll need:</h3>
                <ul className="list-disc list-inside space-y-1 text-sm text-muted-foreground">
                  <li>Valid government-issued ID</li>
                  <li>Bank account information</li>
                  <li>Tax ID or CPF number (for Brazil)</li>
                  <li>Business details (if applicable)</li>
                </ul>
              </div>
              <Button
                onClick={handleConnect}
                disabled={connectAccount.isPending || connectingToStripe}
                size="lg"
                className="w-full sm:w-auto"
              >
                {connectAccount.isPending || connectingToStripe ? (
                  <>
                    <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                    Connecting to Stripe...
                  </>
                ) : (
                  <>
                    <ExternalLink className="h-4 w-4 mr-2" />
                    Connect with Stripe
                  </>
                )}
              </Button>
            </>
          ) : (
            <div className="space-y-4">
              {/* Account Details */}
              <div className="grid gap-4 md:grid-cols-2">
                <div className="space-y-1">
                  <p className="text-sm text-muted-foreground">Account ID</p>
                  <p className="font-mono text-sm">{status.accountId}</p>
                </div>
                <div className="space-y-1">
                  <p className="text-sm text-muted-foreground">Details Submitted</p>
                  <div className="flex items-center gap-2">
                    {status.detailsSubmitted ? (
                      <Badge className="bg-green-500">
                        <CheckCircle className="h-3 w-3 mr-1" />
                        Complete
                      </Badge>
                    ) : (
                      <Badge variant="destructive">
                        <XCircle className="h-3 w-3 mr-1" />
                        Incomplete
                      </Badge>
                    )}
                  </div>
                </div>
                <div className="space-y-1">
                  <p className="text-sm text-muted-foreground">Charges Enabled</p>
                  <div className="flex items-center gap-2">
                    {status.chargesEnabled ? (
                      <Badge className="bg-green-500">Enabled</Badge>
                    ) : (
                      <Badge variant="outline">Disabled</Badge>
                    )}
                  </div>
                </div>
                <div className="space-y-1">
                  <p className="text-sm text-muted-foreground">Payouts Enabled</p>
                  <div className="flex items-center gap-2">
                    {status.payoutsEnabled ? (
                      <Badge className="bg-green-500">Enabled</Badge>
                    ) : (
                      <Badge variant="outline">Disabled</Badge>
                    )}
                  </div>
                </div>
              </div>

              {/* Requirements Alert */}
              {!status.detailsSubmitted && status.requirements && status.requirements.length > 0 && (
                <Alert>
                  <AlertCircle className="h-4 w-4" />
                  <AlertTitle>Action Required</AlertTitle>
                  <AlertDescription>
                    You need to complete your Stripe onboarding to receive payments.
                    <ul className="list-disc list-inside mt-2 text-sm">
                      {status.requirements.slice(0, 5).map((req, idx) => (
                        <li key={idx}>{req.replace(/_/g, " ")}</li>
                      ))}
                    </ul>
                  </AlertDescription>
                </Alert>
              )}

              {/* Pending Verification Alert */}
              {status.pendingVerification && status.pendingVerification.length > 0 && (
                <Alert>
                  <AlertCircle className="h-4 w-4" />
                  <AlertTitle>Verification Pending</AlertTitle>
                  <AlertDescription>
                    Some information is pending verification by Stripe. This may take a few days.
                  </AlertDescription>
                </Alert>
              )}

              {/* Action Buttons */}
              <div className="flex flex-col sm:flex-row gap-2">
                {!status.detailsSubmitted && (
                  <Button
                    onClick={handleContinueOnboarding}
                    disabled={refreshUrl.isPending}
                  >
                    {refreshUrl.isPending ? (
                      <>
                        <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                        Loading...
                      </>
                    ) : (
                      <>
                        <ExternalLink className="h-4 w-4 mr-2" />
                        Continue Onboarding
                      </>
                    )}
                  </Button>
                )}
                <Button
                  variant="outline"
                  onClick={() => router.push("/earnings")}
                >
                  Go to Earnings
                </Button>
                <Button
                  variant="destructive"
                  onClick={handleDisconnect}
                  disabled={disconnectAccount.isPending}
                >
                  {disconnectAccount.isPending ? (
                    <>
                      <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                      Disconnecting...
                    </>
                  ) : (
                    "Disconnect Account"
                  )}
                </Button>
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Information Card */}
      <Card>
        <CardHeader>
          <CardTitle>About Stripe Connect</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div>
            <h3 className="font-semibold mb-2">How it works</h3>
            <ol className="list-decimal list-inside space-y-2 text-sm text-muted-foreground">
              <li>Connect your Stripe account by clicking the button above</li>
              <li>Complete the onboarding process with your bank and tax information</li>
              <li>Start selling workout plans to students</li>
              <li>Request withdrawals from your earnings dashboard</li>
              <li>Receive payments directly to your bank account</li>
            </ol>
          </div>
          <div>
            <h3 className="font-semibold mb-2">Security & Privacy</h3>
            <p className="text-sm text-muted-foreground">
              Your financial information is handled securely by Stripe, a leading payment
              processor trusted by millions of businesses worldwide. GymHero never stores
              your bank account or card information.
            </p>
          </div>
          <div>
            <h3 className="font-semibold mb-2">Fees</h3>
            <p className="text-sm text-muted-foreground">
              Stripe charges a standard processing fee for each transaction. You&apos;ll see the
              exact fees during the onboarding process. GymHero takes a small platform fee
              to maintain and improve the service.
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
