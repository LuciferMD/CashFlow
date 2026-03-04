import { useState } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "../../../app/components/ui/card";
import { AddStock } from "../../../features/stock/add-stock";
import { StockActions } from "../../../features/stock/stock-actions";
import { PortfolioSummary } from "./PortfolioSummary";
import { calculatePortfolioValue, calculatePortfolioGainLoss, calculatePortfolioChangePercent } from "../lib/calculations";
import type {Stock} from "../../../entities/stock";

interface StockPortfolioProps {
  stocks: Stock[];
  onAddStock: (stock: { symbol: string; name: string; shares: string; purchasePrice: string; currentPrice: string }) => void;
  onDeleteStock: (id: number) => void;
}

export function StockPortfolio({ stocks, onAddStock, onDeleteStock }: StockPortfolioProps) {
  const [selectedStockId, setSelectedStockId] = useState<number | null>(null);

  const totalValue = calculatePortfolioValue(stocks);
  const totalGainLoss = calculatePortfolioGainLoss(stocks);
  const portfolioChange = calculatePortfolioChangePercent(stocks);

  const handleToggleStock = (id: number) => {
    setSelectedStockId(selectedStockId === id ? null : id);
  };

  return (
    <>
      <PortfolioSummary 
        totalValue={totalValue}
        totalGainLoss={totalGainLoss}
        portfolioChange={portfolioChange}
      />

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Your Stock Holdings</CardTitle>
              <CardDescription>Manage your stock portfolio</CardDescription>
            </div>
            <AddStock onAdd={onAddStock} />
          </div>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {stocks.length === 0 ? (
              <div className="text-center py-12 text-muted-foreground">
                <p>No stocks in your portfolio yet.</p>
                <p className="text-sm mt-2">Click "Add Stock" to get started.</p>
              </div>
            ) : (
              stocks.map((stock) => (
                <div key={stock.id} className="relative">
                  <div className="border rounded-lg overflow-hidden transition-all">
                    <div 
                      className="flex items-center justify-between p-4 hover:bg-accent transition-colors cursor-pointer"
                      onClick={() => handleToggleStock(stock.id)}
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
                        <p className="font-semibold">${(stock.shares * stock.currentPrice).toFixed(2)}</p>
                        <p className={`text-sm ${(stock.shares * stock.currentPrice - stock.shares * stock.purchasePrice) >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                          {(stock.shares * stock.currentPrice - stock.shares * stock.purchasePrice) >= 0 ? '+' : ''}${(stock.shares * stock.currentPrice - stock.shares * stock.purchasePrice).toFixed(2)} ({(stock.shares * stock.currentPrice - stock.shares * stock.purchasePrice) >= 0 ? '+' : ''}{(((stock.shares * stock.currentPrice - stock.shares * stock.purchasePrice) / (stock.shares * stock.purchasePrice)) * 100).toFixed(2)}%)
                        </p>
                      </div>
                      <StockActions
                        onSell={() => onDeleteStock(stock.id)}
                        onBuyMore={() => {/* TODO: Add buy more functionality */}}
                      />
                    </div>
                    
                    {/* Expanded Details */}
                    {selectedStockId === stock.id && (
                      <div className="px-4 pb-4 pt-2 bg-accent/50 border-t">
                        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                          <div className="space-y-1">
                            <p className="text-xs text-muted-foreground">Shares Owned</p>
                            <p className="text-lg font-semibold">{stock.shares}</p>
                          </div>
                          <div className="space-y-1">
                            <p className="text-xs text-muted-foreground">Total Invested</p>
                            <p className="text-lg font-semibold">${(stock.shares * stock.purchasePrice).toFixed(2)}</p>
                            <p className="text-xs text-muted-foreground">${stock.purchasePrice.toFixed(2)}/share</p>
                          </div>
                          <div className="space-y-1">
                            <p className="text-xs text-muted-foreground">Current Value</p>
                            <p className="text-lg font-semibold">${(stock.shares * stock.currentPrice).toFixed(2)}</p>
                            <p className="text-xs text-muted-foreground">${stock.currentPrice.toFixed(2)}/share</p>
                          </div>
                          <div className="space-y-1">
                            <p className="text-xs text-muted-foreground">Current Profit</p>
                            <p className={`text-lg font-semibold ${(stock.shares * stock.currentPrice - stock.shares * stock.purchasePrice) >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                              {(stock.shares * stock.currentPrice - stock.shares * stock.purchasePrice) >= 0 ? '+' : ''}${(stock.shares * stock.currentPrice - stock.shares * stock.purchasePrice).toFixed(2)}
                            </p>
                            <p className={`text-xs ${(stock.shares * stock.currentPrice - stock.shares * stock.purchasePrice) >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                              {(stock.shares * stock.currentPrice - stock.shares * stock.purchasePrice) >= 0 ? '+' : ''}{(((stock.shares * stock.currentPrice - stock.shares * stock.purchasePrice) / (stock.shares * stock.purchasePrice)) * 100).toFixed(2)}%
                            </p>
                          </div>
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              ))
            )}
          </div>
        </CardContent>
      </Card>
    </>
  );
}