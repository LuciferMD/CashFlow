import { TrendingDown, TrendingUp } from "lucide-react";
import { Button } from "../../../../app/components/ui/button";

interface StockActionsProps {
  onSell: () => void;
  onBuyMore: () => void;
}

export function StockActions({ onSell, onBuyMore }: StockActionsProps) {
  return (
    <div className="flex gap-2" onClick={(e) => e.stopPropagation()}>
      <Button
        variant="ghost"
        size="sm"
        onClick={onSell}
        className="text-red-600 hover:text-red-700 hover:bg-red-50"
        title="Sell stock"
      >
        <TrendingDown className="w-4 h-4" />
      </Button>
      <Button
        variant="ghost"
        size="sm"
        onClick={onBuyMore}
        className="text-green-600 hover:text-green-700 hover:bg-green-50"
        title="Buy more"
      >
        <TrendingUp className="w-4 h-4" />
      </Button>
    </div>
  );
}