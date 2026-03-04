import { Card, CardDescription, CardHeader, CardTitle } from "../../../app/components/ui/card";

interface PortfolioSummaryProps {
  totalValue: number;
  totalGainLoss: number;
  portfolioChange: string;
}

export function PortfolioSummary({ totalValue, totalGainLoss, portfolioChange }: PortfolioSummaryProps) {
  return (
    <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-4">
      <Card>
        <CardHeader>
          <CardDescription>Total Portfolio Value</CardDescription>
          <CardTitle className="text-2xl mb-4">${totalValue.toFixed(2)}</CardTitle>
        </CardHeader>
      </Card>
      <Card>
        <CardHeader>
          <CardDescription>Total Gain/Loss</CardDescription>
          <CardTitle className={`text-2xl ${totalGainLoss >= 0 ? 'text-green-600' : 'text-red-600'}`}>
            {totalGainLoss >= 0 ? '+' : ''}${totalGainLoss.toFixed(2)}
          </CardTitle>
        </CardHeader>
      </Card>
      <Card>
        <CardHeader>
          <CardDescription>Portfolio Return</CardDescription>
          <CardTitle className={`text-2xl ${totalGainLoss >= 0 ? 'text-green-600' : 'text-red-600'}`}>
            {totalGainLoss >= 0 ? '+' : ''}{portfolioChange}%
          </CardTitle>
        </CardHeader>
      </Card>
    </div>
  );
}