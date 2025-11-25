"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/lib/api";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Badge } from "@/components/ui/badge";
import { useRouter } from "next/navigation";
import { Heart, Dumbbell, Calendar, User, Star } from "lucide-react";

interface WorkoutPlan {
  id: string;
  name: string;
  goal: string;
  duration: number;
  forSale: boolean;
  price?: number;
  ownerName: string;
  ownerId: string;
  rating?: number;
  purchaseCount?: number;
}

export default function DiscoverPlansPage() {
  const router = useRouter();
  const [activeTab, setActiveTab] = useState<"free" | "premium" | "friends">("free");

  // Fetch all public plans
  const { data: allPlans, isLoading: plansLoading } = useQuery<WorkoutPlan[]>({
    queryKey: ["public-plans"],
    queryFn: async () => {
      return apiClient.get("/api/workout-plans/public");
    },
  });

  // Fetch friend/followed trainer plans
  const { data: friendPlans, isLoading: friendsLoading } = useQuery<WorkoutPlan[]>({
    queryKey: ["friend-plans"],
    queryFn: async () => {
      try {
        return apiClient.get("/personal/following/plans");
      } catch (error) {
        return [];
      }
    },
    enabled: activeTab === "friends",
  });

  // Filter plans by type
  const freePlans = allPlans?.filter((plan) => !plan.forSale) || [];
  const premiumPlans = allPlans?.filter((plan) => plan.forSale && plan.price && plan.price > 0) || [];

  const handleViewPlan = (planId: string) => {
    router.push(`/plans/public/${planId}`);
  };

  const renderPlanCard = (plan: WorkoutPlan, isPremium: boolean = false, isFromFriend: boolean = false) => (
    <Card key={plan.id} className="hover:shadow-lg transition-shadow">
      <CardHeader>
        <div className="flex justify-between items-start">
          <div className="flex-1">
            <CardTitle className="text-xl mb-2">{plan.name}</CardTitle>
            <CardDescription className="flex items-center gap-2 text-sm">
              <User className="h-4 w-4" />
              <span>by {plan.ownerName}</span>
            </CardDescription>
          </div>
          <div className="flex flex-col gap-2 items-end">
            {isPremium && plan.price && (
              <Badge className="bg-yellow-500 hover:bg-yellow-600">
                R$ {plan.price.toFixed(2)}
              </Badge>
            )}
            {!isPremium && !isFromFriend && (
              <Badge className="bg-green-500 hover:bg-green-600">FREE</Badge>
            )}
            {isFromFriend && (
              <Badge className="bg-blue-500 hover:bg-blue-600">FRIEND</Badge>
            )}
          </div>
        </div>
      </CardHeader>
      <CardContent className="space-y-3">
        <div className="flex items-start gap-2">
          <Dumbbell className="h-5 w-5 text-muted-foreground mt-0.5 flex-shrink-0" />
          <p className="text-sm">{plan.goal}</p>
        </div>
        <div className="flex items-center gap-2 text-sm text-muted-foreground">
          <Calendar className="h-4 w-4" />
          <span>{plan.duration} weeks</span>
        </div>
        {plan.rating && (
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <Star className="h-4 w-4 fill-yellow-500 text-yellow-500" />
            <span>{plan.rating.toFixed(1)}</span>
            {plan.purchaseCount && (
              <span className="text-xs">({plan.purchaseCount} purchases)</span>
            )}
          </div>
        )}
      </CardContent>
      <CardFooter>
        <Button
          className="w-full"
          variant={isPremium ? "default" : "outline"}
          onClick={() => handleViewPlan(plan.id)}
        >
          {isPremium ? "Purchase Plan" : "View Details"}
        </Button>
      </CardFooter>
    </Card>
  );

  return (
    <div className="container mx-auto py-8">
      <div className="mb-8">
        <h1 className="text-4xl font-bold mb-2">Discover Workout Plans</h1>
        <p className="text-muted-foreground text-lg">
          Browse workout plans from trainers around the world
        </p>
      </div>

      <Tabs value={activeTab} onValueChange={(v) => setActiveTab(v as any)} className="space-y-6">
        <TabsList className="grid w-full grid-cols-3">
          <TabsTrigger value="free" className="text-base">
            <Heart className="h-4 w-4 mr-2" />
            Free Plans ({freePlans.length})
          </TabsTrigger>
          <TabsTrigger value="premium" className="text-base">
            <Star className="h-4 w-4 mr-2" />
            Premium Plans ({premiumPlans.length})
          </TabsTrigger>
          <TabsTrigger value="friends" className="text-base">
            <User className="h-4 w-4 mr-2" />
            From Friends ({friendPlans?.length || 0})
          </TabsTrigger>
        </TabsList>

        {/* Free Plans Tab */}
        <TabsContent value="free" className="space-y-6">
          {plansLoading ? (
            <div className="text-center py-12">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div>
              <p className="mt-4 text-muted-foreground">Loading free plans...</p>
            </div>
          ) : freePlans.length === 0 ? (
            <div className="text-center py-12">
              <Heart className="h-16 w-16 text-muted-foreground mx-auto mb-4" />
              <h3 className="text-xl font-semibold mb-2">No Free Plans Available</h3>
              <p className="text-muted-foreground">
                Check back later for new free workout plans from trainers
              </p>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {freePlans.map((plan) => renderPlanCard(plan, false, false))}
            </div>
          )}
        </TabsContent>

        {/* Premium Plans Tab */}
        <TabsContent value="premium" className="space-y-6">
          {plansLoading ? (
            <div className="text-center py-12">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div>
              <p className="mt-4 text-muted-foreground">Loading premium plans...</p>
            </div>
          ) : premiumPlans.length === 0 ? (
            <div className="text-center py-12">
              <Star className="h-16 w-16 text-muted-foreground mx-auto mb-4" />
              <h3 className="text-xl font-semibold mb-2">No Premium Plans Available</h3>
              <p className="text-muted-foreground">
                Premium workout plans from professional trainers will appear here
              </p>
            </div>
          ) : (
            <>
              <div className="bg-muted/50 rounded-lg p-4 mb-6">
                <h3 className="font-semibold mb-2">Premium Plans</h3>
                <p className="text-sm text-muted-foreground">
                  These plans are created by professional trainers and include detailed workout
                  routines, progression tracking, and expert guidance. Purchases support the trainers
                  directly.
                </p>
              </div>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {premiumPlans.map((plan) => renderPlanCard(plan, true, false))}
              </div>
            </>
          )}
        </TabsContent>

        {/* Friend Plans Tab */}
        <TabsContent value="friends" className="space-y-6">
          {friendsLoading ? (
            <div className="text-center py-12">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div>
              <p className="mt-4 text-muted-foreground">Loading plans from friends...</p>
            </div>
          ) : !friendPlans || friendPlans.length === 0 ? (
            <div className="text-center py-12">
              <User className="h-16 w-16 text-muted-foreground mx-auto mb-4" />
              <h3 className="text-xl font-semibold mb-2">No Plans from Friends</h3>
              <p className="text-muted-foreground mb-4">
                Follow trainers to see their public workout plans here
              </p>
              <Button onClick={() => router.push("/trainers")}>
                Browse Trainers
              </Button>
            </div>
          ) : (
            <>
              <div className="bg-muted/50 rounded-lg p-4 mb-6">
                <h3 className="font-semibold mb-2">Plans from Trainers You Follow</h3>
                <p className="text-sm text-muted-foreground">
                  These plans are from trainers you&apos;re following. Support your favorite trainers
                  by using their workout plans!
                </p>
              </div>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {friendPlans.map((plan) => renderPlanCard(plan, plan.forSale, true))}
              </div>
            </>
          )}
        </TabsContent>
      </Tabs>
    </div>
  );
}
