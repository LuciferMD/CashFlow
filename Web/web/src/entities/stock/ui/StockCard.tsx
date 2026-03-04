import type {Stock} from "../model/types.ts";

interface StockCardProps {
  stock: Stock;
  isExpanded: boolean;
  onToggle: () => void;
}

export function StockCard({ stock, isExpanded, onToggle }: StockCardProps) {
  const invested = stock.shares * stock.purchasePrice;
  const currentValue = stock.shares * stock.currentPrice;
  const gainLoss = currentValue - invested;
  const gainLossPercent = ((gainLoss / invested) * 100).toFixed(2);

  return (
    <div className="border rounded-lg overflow-hidden transition-all">
      <div 
        className="flex items-center justify-between p-4 hover:bg-accent transition-colors cursor-pointer"
        onClick={onToggle}
      >
        <div className="flex items-center gap-4 flex-1">
          <div className="w-12 h-12 bg-blue-100 text-blue-600 rounded-lg flex items-center justify-center">
            <span className="font-bold text-sm">{stock.symbol}</span>
          </div>
          <div className="flex-1">
            <p className="font-medium">{stock.name}</p>
            <p className="text-sm text-muted-foreground">
              {stock.shares} shares @ ${stock.currentPrice.toFixed(2)}
            </p>
          </div>
        </div>
        <div className="text-right mr-4">
          <p className="font-semibold">${currentValue.toFixed(2)}</p>
          <p className={`text-sm ${gainLoss >= 0 ? 'text-green-600' : 'text-red-600'}`}>
            {gainLoss >= 0 ? '+' : ''}${gainLoss.toFixed(2)} ({gainLoss >= 0 ? '+' : ''}{gainLossPercent}%)
          </p>
        </div>
      </div>
      
      {/* Expanded Details */}
      {isExpanded && (
        <div className="px-4 pb-4 pt-2 bg-accent/50 border-t">
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div className="space-y-1">
              <p className="text-xs text-muted-foreground">Shares Owned</p>
              <p className="text-lg font-semibold">{stock.shares}</p>
            </div>
            <div className="space-y-1">
              <p className="text-xs text-muted-foreground">Total Invested</p>
              <p className="text-lg font-semibold">${invested.toFixed(2)}</p>
              <p className="text-xs text-muted-foreground">${stock.purchasePrice.toFixed(2)}/share</p>
            </div>
            <div className="space-y-1">
              <p className="text-xs text-muted-foreground">Current Value</p>
              <p className="text-lg font-semibold">${currentValue.toFixed(2)}</p>
              <p className="text-xs text-muted-foreground">${stock.currentPrice.toFixed(2)}/share</p>
            </div>
            <div className="space-y-1">
              <p className="text-xs text-muted-foreground">Current Profit</p>
              <p className={`text-lg font-semibold ${gainLoss >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                {gainLoss >= 0 ? '+' : ''}${gainLoss.toFixed(2)}
              </p>
              <p className={`text-xs ${gainLoss >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                {gainLoss >= 0 ? '+' : ''}{gainLossPercent}%
              </p>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
