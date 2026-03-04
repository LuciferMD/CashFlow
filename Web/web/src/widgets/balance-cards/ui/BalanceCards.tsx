import { ArrowUpRight, Wallet, CreditCard, Receipt, TrendingUp, TrendingDown } from "lucide-react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "../../../app/components/ui/card";

interface BalanceCardsProps {
  currentBalance: number;
  totalIncome: number;
  totalPayments: number;
  totalPortfolioValue: number;
  portfolioChange: string;
}

export function BalanceCards({
  currentBalance,
  totalIncome,
  totalPayments,
  totalPortfolioValue,
  portfolioChange
}: BalanceCardsProps) {
  return (
    <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
      <Card className="bg-gradient-to-br from-blue-600 to-indigo-600 text-white">
        <CardHeader>
          <CardDescription className="text-blue-100">Current Balance</CardDescription>
          <CardTitle className="text-3xl">${currentBalance.toLocaleString()}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center gap-2 text-sm">
            <ArrowUpRight className="w-4 h-4" />
            <span>+12.5% from last month</span>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardDescription>Total Income</CardDescription>
            <Wallet className="w-5 h-5 text-green-600" />
          </div>
          <CardTitle className="text-2xl text-green-600">${totalIncome.toLocaleString()}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <TrendingUp className="w-4 h-4 text-green-600" />
            <span>This month</span>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardDescription>Total Payments</CardDescription>
            <CreditCard className="w-5 h-5 text-red-600" />
          </div>
          <CardTitle className="text-2xl text-red-600">${totalPayments.toLocaleString()}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <TrendingDown className="w-4 h-4 text-red-600" />
            <span>This month</span>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardDescription>Stock Portfolio</CardDescription>
            <Receipt className="w-5 h-5 text-blue-600" />
          </div>
          <CardTitle className="text-2xl">${totalPortfolioValue.toFixed(2)}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center gap-2 text-sm">
            <ArrowUpRight className="w-4 h-4 text-green-600" />
            <span className="text-green-600">+{portfolioChange}%</span>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}