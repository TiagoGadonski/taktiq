"use client";

import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "@/lib/api";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { useToast } from "@/components/ui/use-toast";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { DollarSign, TrendingUp, Clock, CheckCircle, XCircle, AlertCircle } from "lucide-react";
import Link from "next/link";
import { useRouter } from "next/navigation";

interface Balance {
  availableBalance: number;
  totalEarnings: number;
  totalWithdrawn: number;
  pendingWithdrawals: number;
  transactionCount: number;
  currency: string;
}

interface Withdrawal {
  id: string;
  amount: number;
  currency: string;
  status: string;
  method: string;
  requestedAt: string;
  processedAt?: string;
  notes?: string;
  rejectionReason?: string;
}

export default function EarningsPage() {
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const router = useRouter();
  const [showRequestDialog, setShowRequestDialog] = useState(false);
  const [withdrawalAmount, setWithdrawalAmount] = useState("");
  const [withdrawalNotes, setWithdrawalNotes] = useState("");

  // Fetch balance
  const { data: balance, isLoading: balanceLoading } = useQuery<Balance>({
    queryKey: ["balance"],
    queryFn: async () => {
      const { data } = await api.get("/api/withdrawals/balance");
      return data;
    },
  });

  // Fetch Stripe Connect status
  const { data: stripeStatus } = useQuery({
    queryKey: ["stripe-connect-status"],
    queryFn: async () => {
      try {
        const { data } = await api.get("/api/stripe/connect/status");
        return data;
      } catch (error) {
        return { connected: false, chargesEnabled: false, payoutsEnabled: false };
      }
    },
  });

  // Fetch withdrawal history
  const { data: historyData, isLoading: historyLoading } = useQuery({
    queryKey: ["withdrawal-history"],
    queryFn: async () => {
      const { data } = await api.get("/api/withdrawals/history?page=1&pageSize=20");
      return data;
    },
  });

  // Request withdrawal mutation
  const requestWithdrawal = useMutation({
    mutationFn: async (data: { amount: number; notes?: string }) => {
      return await api.post("/api/withdrawals/request", data);
    },
    onSuccess: () => {
      toast({
        title: "Withdrawal Requested",
        description: "Your withdrawal request has been submitted successfully. It will be processed within 2-3 business days.",
      });
      queryClient.invalidateQueries({ queryKey: ["balance"] });
      queryClient.invalidateQueries({ queryKey: ["withdrawal-history"] });
      setShowRequestDialog(false);
      setWithdrawalAmount("");
      setWithdrawalNotes("");
    },
    onError: (error: any) => {
      toast({
        title: "Error",
        description: error.response?.data?.message || "Failed to request withdrawal",
        variant: "destructive",
      });
    },
  });

  // Cancel withdrawal mutation
  const cancelWithdrawal = useMutation({
    mutationFn: async (withdrawalId: string) => {
      return await api.delete(`/api/withdrawals/${withdrawalId}`);
    },
    onSuccess: () => {
      toast({
        title: "Withdrawal Cancelled",
        description: "Your withdrawal request has been cancelled.",
      });
      queryClient.invalidateQueries({ queryKey: ["balance"] });
      queryClient.invalidateQueries({ queryKey: ["withdrawal-history"] });
    },
    onError: (error: any) => {
      toast({
        title: "Error",
        description: error.response?.data?.message || "Failed to cancel withdrawal",
        variant: "destructive",
      });
    },
  });

  const handleRequestWithdrawal = () => {
    const amount = parseFloat(withdrawalAmount);
    if (isNaN(amount) || amount <= 0) {
      toast({
        title: "Invalid Amount",
        description: "Please enter a valid amount",
        variant: "destructive",
      });
      return;
    }

    if (balance && amount > balance.availableBalance) {
      toast({
        title: "Insufficient Balance",
        description: `You only have R$ ${balance.availableBalance.toFixed(2)} available`,
        variant: "destructive",
      });
      return;
    }

    requestWithdrawal.mutate({
      amount,
      notes: withdrawalNotes || undefined,
    });
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case "Completed":
        return <Badge className="bg-green-500">Completed</Badge>;
      case "Pending":
        return <Badge className="bg-yellow-500">Pending</Badge>;
      case "Processing":
        return <Badge className="bg-blue-500">Processing</Badge>;
      case "Rejected":
        return <Badge variant="destructive">Rejected</Badge>;
      case "Failed":
        return <Badge variant="destructive">Failed</Badge>;
      case "Cancelled":
        return <Badge variant="outline">Cancelled</Badge>;
      default:
        return <Badge>{status}</Badge>;
    }
  };

  if (balanceLoading) {
    return (
      <div className="container mx-auto py-8">
        <div className="flex items-center justify-center min-h-[400px]">
          <div className="text-center">Loading...</div>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-8 space-y-8">
      <div>
        <h1 className="text-3xl font-bold">Earnings & Withdrawals</h1>
        <p className="text-muted-foreground">Manage your earnings from workout plan sales</p>
      </div>

      {/* Balance Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Available Balance</CardTitle>
            <DollarSign className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              R$ {balance?.availableBalance.toFixed(2) || "0.00"}
            </div>
            <p className="text-xs text-muted-foreground">Ready to withdraw</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Earnings</CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              R$ {balance?.totalEarnings.toFixed(2) || "0.00"}
            </div>
            <p className="text-xs text-muted-foreground">
              From {balance?.transactionCount || 0} sales
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Pending</CardTitle>
            <Clock className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              R$ {balance?.pendingWithdrawals.toFixed(2) || "0.00"}
            </div>
            <p className="text-xs text-muted-foreground">Being processed</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Withdrawn</CardTitle>
            <CheckCircle className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              R$ {balance?.totalWithdrawn.toFixed(2) || "0.00"}
            </div>
            <p className="text-xs text-muted-foreground">Lifetime withdrawals</p>
          </CardContent>
        </Card>
      </div>

      {/* Stripe Connect Alert */}
      {!stripeStatus?.connected && (
        <Alert>
          <AlertCircle className="h-4 w-4" />
          <AlertTitle>Connect Your Stripe Account</AlertTitle>
          <AlertDescription className="space-y-2">
            <p>
              You need to connect your Stripe account to receive payments from workout plan
              sales and request withdrawals.
            </p>
            <Button onClick={() => router.push("/stripe-connect")} variant="outline" size="sm">
              Connect Stripe Account
            </Button>
          </AlertDescription>
        </Alert>
      )}

      {stripeStatus?.connected && !stripeStatus?.payoutsEnabled && (
        <Alert>
          <AlertCircle className="h-4 w-4" />
          <AlertTitle>Complete Your Stripe Onboarding</AlertTitle>
          <AlertDescription className="space-y-2">
            <p>
              Your Stripe account is connected but not fully set up. Complete the onboarding
              process to enable payouts.
            </p>
            <Button onClick={() => router.push("/stripe-connect")} variant="outline" size="sm">
              Complete Onboarding
            </Button>
          </AlertDescription>
        </Alert>
      )}

      {/* Request Withdrawal Button */}
      <Card>
        <CardHeader>
          <CardTitle>Request Withdrawal</CardTitle>
          <CardDescription>
            Withdraw your available earnings to your bank account via Stripe
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Button
            onClick={() => setShowRequestDialog(true)}
            disabled={!balance || balance.availableBalance <= 0 || !stripeStatus?.payoutsEnabled}
            size="lg"
          >
            Request Withdrawal
          </Button>
          {balance && balance.availableBalance <= 0 && (
            <p className="text-sm text-muted-foreground mt-2">
              No available balance to withdraw
            </p>
          )}
          {!stripeStatus?.payoutsEnabled && balance && balance.availableBalance > 0 && (
            <p className="text-sm text-muted-foreground mt-2">
              Connect and verify your Stripe account to request withdrawals
            </p>
          )}
        </CardContent>
      </Card>

      {/* Withdrawal History */}
      <Card>
        <CardHeader>
          <CardTitle>Withdrawal History</CardTitle>
          <CardDescription>View all your withdrawal requests</CardDescription>
        </CardHeader>
        <CardContent>
          {historyLoading ? (
            <div className="text-center py-8">Loading history...</div>
          ) : historyData?.withdrawals?.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              No withdrawal requests yet
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Date</TableHead>
                  <TableHead>Amount</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Method</TableHead>
                  <TableHead>Notes</TableHead>
                  <TableHead>Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {historyData?.withdrawals?.map((withdrawal: Withdrawal) => (
                  <TableRow key={withdrawal.id}>
                    <TableCell>
                      {new Date(withdrawal.requestedAt).toLocaleDateString()}
                    </TableCell>
                    <TableCell className="font-medium">
                      R$ {withdrawal.amount.toFixed(2)}
                    </TableCell>
                    <TableCell>{getStatusBadge(withdrawal.status)}</TableCell>
                    <TableCell>{withdrawal.method}</TableCell>
                    <TableCell className="max-w-xs truncate">
                      {withdrawal.notes || "-"}
                      {withdrawal.rejectionReason && (
                        <p className="text-sm text-red-500 mt-1">
                          Reason: {withdrawal.rejectionReason}
                        </p>
                      )}
                    </TableCell>
                    <TableCell>
                      {withdrawal.status === "Pending" && (
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => cancelWithdrawal.mutate(withdrawal.id)}
                        >
                          Cancel
                        </Button>
                      )}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Request Withdrawal Dialog */}
      <Dialog open={showRequestDialog} onOpenChange={setShowRequestDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Request Withdrawal</DialogTitle>
            <DialogDescription>
              Enter the amount you want to withdraw from your available balance
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="amount">Amount (BRL)</Label>
              <Input
                id="amount"
                type="number"
                step="0.01"
                min="0"
                max={balance?.availableBalance || 0}
                placeholder="0.00"
                value={withdrawalAmount}
                onChange={(e) => setWithdrawalAmount(e.target.value)}
              />
              <p className="text-sm text-muted-foreground">
                Available: R$ {balance?.availableBalance.toFixed(2) || "0.00"}
              </p>
            </div>
            <div className="space-y-2">
              <Label htmlFor="notes">Notes (Optional)</Label>
              <Textarea
                id="notes"
                placeholder="Add any notes about this withdrawal..."
                value={withdrawalNotes}
                onChange={(e) => setWithdrawalNotes(e.target.value)}
                rows={3}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowRequestDialog(false)}>
              Cancel
            </Button>
            <Button onClick={handleRequestWithdrawal} disabled={requestWithdrawal.isPending}>
              {requestWithdrawal.isPending ? "Requesting..." : "Request Withdrawal"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
