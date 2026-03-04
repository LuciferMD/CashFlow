import type {Stock} from "../../../entities/stock";

export function calculatePortfolioValue(stocks: Stock[]): number {
  return stocks.reduce((total, stock) => total + (stock.shares * stock.currentPrice), 0);
}

export function calculatePortfolioGainLoss(stocks: Stock[]): number {
  return stocks.reduce((total, stock) => {
    const invested = stock.shares * stock.purchasePrice;
    const current = stock.shares * stock.currentPrice;
    return total + (current - invested);
  }, 0);
}

export function calculatePortfolioChangePercent(stocks: Stock[]): string {
  const totalGainLoss = calculatePortfolioGainLoss(stocks);
  const totalValue = calculatePortfolioValue(stocks);
  
  if (totalGainLoss > 0) {
    return ((totalGainLoss / (totalValue - totalGainLoss)) * 100).toFixed(2);
  }
  return "0.00";
}
