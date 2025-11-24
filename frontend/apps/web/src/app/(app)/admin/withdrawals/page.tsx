"use client";

import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api";
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
import { CheckCircle, XCircle, Clock, AlertCircle } from "lucide-react";

interface PendingWithdrawal {
  id: string;
  trainerId: string;
  trainerName: string;
  trainerEmail: string;
  stripeAccountId: string | null;
  amount: number;
  currency: string;
  status: string;
  method: string;
  requestedAt: string;
  notes?: string;
}

interface WithdrawalsResponse {
  withdrawals: PendingWithdrawal[];
  totalCount: number;
}

export default function AdminWithdrawalsPage() {
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [showRejectDialog, setShowRejectDialog] = useState(false);
  const [selectedWithdrawal, setSelectedWithdrawal] = useState<PendingWithdrawal | null>(null);
  const [rejectionReason, setRejectionReason] = useState("");

  // Fetch pending withdrawals
  const { data: withdrawalsData, isLoading } = useQuery<WithdrawalsResponse>({
    queryKey: ["admin-pending-withdrawals"],
    queryFn: async () => {
      return apiClient.get("/withdrawals/admin/pending?page=1&pageSize=50");
    },
  });

  // Approve withdrawal mutation
  const approveWithdrawal = useMutation({
    mutationFn: async (withdrawalId: string) => {
      return await apiClient.post(`/withdrawals/admin/${withdrawalId}/approve`);
    },
    onSuccess: (response) => {
      toast({
        title: "Withdrawal Approved",
        description: "The withdrawal has been approved and processed successfully.",
      });
      queryClient.invalidateQueries({ queryKey: ["admin-pending-withdrawals"] });
    },
    onError: (error: any) => {
      toast({
        title: "Error",
        description: error.response?.data?.message || "Failed to approve withdrawal",
        variant: "destructive",
      });
    },
  });

  // Reject withdrawal mutation
  const rejectWithdrawal = useMutation({
    mutationFn: async (data: { withdrawalId: string; reason: string }) => {
      return await apiClient.post(`/withdrawals/admin/${data.withdrawalId}/reject`, {
        reason: data.reason,
      });
    },
    onSuccess: () => {
      toast({
        title: "Withdrawal Rejected",
        description: "The withdrawal request has been rejected.",
      });
      queryClient.invalidateQueries({ queryKey: ["admin-pending-withdrawals"] });
      setShowRejectDialog(false);
      setSelectedWithdrawal(null);
      setRejectionReason("");
    },
    onError: (error: any) => {
      toast({
        title: "Error",
        description: error.response?.data?.message || "Failed to reject withdrawal",
        variant: "destructive",
      });
    },
  });

  const handleApprove = (withdrawal: PendingWithdrawal) => {
    if (!withdrawal.stripeAccountId) {
      toast({
        title: "Cannot Approve",
        description: "This trainer has not connected their Stripe account yet.",
        variant: "destructive",
      });
      return;
    }

    if (window.confirm(`Approve withdrawal of R$ ${withdrawal.amount.toFixed(2)} for ${withdrawal.trainerName}?`)) {
      approveWithdrawal.mutate(withdrawal.id);
    }
  };

  const handleReject = (withdrawal: PendingWithdrawal) => {
    setSelectedWithdrawal(withdrawal);
    setShowRejectDialog(true);
  };

  const confirmReject = () => {
    if (!selectedWithdrawal || !rejectionReason.trim()) {
      toast({
        title: "Rejection Reason Required",
        description: "Please provide a reason for rejecting this withdrawal",
        variant: "destructive",
      });
      return;
    }

    rejectWithdrawal.mutate({
      withdrawalId: selectedWithdrawal.id,
      reason: rejectionReason,
    });
  };

  const totalPendingAmount = withdrawalsData?.withdrawals?.reduce(
    (sum: number, w: PendingWithdrawal) => sum + w.amount,
    0
  ) || 0;

  return (
    <div className="container mx-auto py-8 space-y-8">
      <div>
        <h1 className="text-3xl font-bold">Withdrawal Requests</h1>
        <p className="text-muted-foreground">Manage pending trainer withdrawal requests</p>
      </div>

      {/* Summary Cards */}
      <div className="grid gap-4 md:grid-cols-3">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Pending Requests</CardTitle>
            <Clock className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {withdrawalsData?.withdrawals?.length || 0}
            </div>
            <p className="text-xs text-muted-foreground">Awaiting approval</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Amount</CardTitle>
            <AlertCircle className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              R$ {totalPendingAmount.toFixed(2)}
            </div>
            <p className="text-xs text-muted-foreground">Pending withdrawals</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Average Request</CardTitle>
            <CheckCircle className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              R${" "}
              {withdrawalsData?.withdrawals && withdrawalsData.withdrawals.length > 0
                ? (totalPendingAmount / withdrawalsData.withdrawals.length).toFixed(2)
                : "0.00"}
            </div>
            <p className="text-xs text-muted-foreground">Per trainer</p>
          </CardContent>
        </Card>
      </div>

      {/* Pending Withdrawals Table */}
      <Card>
        <CardHeader>
          <CardTitle>Pending Withdrawal Requests</CardTitle>
          <CardDescription>
            Review and approve or reject trainer withdrawal requests
          </CardDescription>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="text-center py-8">Loading...</div>
          ) : withdrawalsData?.withdrawals?.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              No pending withdrawal requests
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Trainer</TableHead>
                  <TableHead>Email</TableHead>
                  <TableHead>Amount</TableHead>
                  <TableHead>Requested</TableHead>
                  <TableHead>Stripe Account</TableHead>
                  <TableHead>Notes</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {withdrawalsData?.withdrawals?.map((withdrawal: PendingWithdrawal) => (
                  <TableRow key={withdrawal.id}>
                    <TableCell className="font-medium">{withdrawal.trainerName}</TableCell>
                    <TableCell className="text-sm text-muted-foreground">
                      {withdrawal.trainerEmail}
                    </TableCell>
                    <TableCell className="font-bold">
                      R$ {withdrawal.amount.toFixed(2)}
                    </TableCell>
                    <TableCell className="text-sm">
                      {new Date(withdrawal.requestedAt).toLocaleDateString()}
                    </TableCell>
                    <TableCell>
                      {withdrawal.stripeAccountId ? (
                        <Badge className="bg-green-500">Connected</Badge>
                      ) : (
                        <Badge variant="destructive">Not Connected</Badge>
                      )}
                    </TableCell>
                    <TableCell className="max-w-xs truncate text-sm">
                      {withdrawal.notes || "-"}
                    </TableCell>
                    <TableCell className="text-right space-x-2">
                      <Button
                        size="sm"
                        variant="default"
                        onClick={() => handleApprove(withdrawal)}
                        disabled={
                          !withdrawal.stripeAccountId || approveWithdrawal.isPending
                        }
                      >
                        <CheckCircle className="h-4 w-4 mr-1" />
                        Approve
                      </Button>
                      <Button
                        size="sm"
                        variant="destructive"
                        onClick={() => handleReject(withdrawal)}
                        disabled={rejectWithdrawal.isPending}
                      >
                        <XCircle className="h-4 w-4 mr-1" />
                        Reject
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Reject Dialog */}
      <Dialog open={showRejectDialog} onOpenChange={setShowRejectDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Reject Withdrawal Request</DialogTitle>
            <DialogDescription>
              Provide a reason for rejecting this withdrawal request
            </DialogDescription>
          </DialogHeader>
          {selectedWithdrawal && (
            <div className="space-y-4 py-4">
              <div className="space-y-2">
                <p className="text-sm">
                  <span className="font-semibold">Trainer:</span>{" "}
                  {selectedWithdrawal.trainerName}
                </p>
                <p className="text-sm">
                  <span className="font-semibold">Amount:</span> R${" "}
                  {selectedWithdrawal.amount.toFixed(2)}
                </p>
              </div>
              <div className="space-y-2">
                <Label htmlFor="rejection-reason">Rejection Reason *</Label>
                <Textarea
                  id="rejection-reason"
                  placeholder="e.g., Missing documentation, Suspicious activity, etc."
                  value={rejectionReason}
                  onChange={(e) => setRejectionReason(e.target.value)}
                  rows={4}
                  required
                />
              </div>
            </div>
          )}
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => {
                setShowRejectDialog(false);
                setSelectedWithdrawal(null);
                setRejectionReason("");
              }}
            >
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={confirmReject}
              disabled={rejectWithdrawal.isPending || !rejectionReason.trim()}
            >
              {rejectWithdrawal.isPending ? "Rejecting..." : "Reject Withdrawal"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
