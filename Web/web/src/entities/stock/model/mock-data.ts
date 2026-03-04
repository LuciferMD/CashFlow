import type {Stock} from "./types.ts";

export const mockStocks: Stock[] = [
  { id: 1, symbol: "AAPL", name: "Apple Inc.", shares: 10, purchasePrice: 150, currentPrice: 162 },
  { id: 2, symbol: "GOOGL", name: "Alphabet Inc.", shares: 5, purchasePrice: 250, currentPrice: 260 },
  { id: 3, symbol: "MSFT", name: "Microsoft Corporation", shares: 8, purchasePrice: 300, currentPrice: 310 },
];
