import type {Transaction} from "./types.ts";

export const mockTransactions: Transaction[] = [
  { id: 1, type: "income", description: "Salary Payment", amount: 5000, date: "Mar 1, 2026", category: "Salary" },
  { id: 2, type: "payment", description: "Rent Payment", amount: -1200, date: "Mar 1, 2026", category: "Housing" },
  { id: 3, type: "income", description: "Freelance Project", amount: 2500, date: "Feb 28, 2026", category: "Freelance" },
  { id: 4, type: "payment", description: "Grocery Shopping", amount: -350, date: "Feb 27, 2026", category: "Food" },
  { id: 5, type: "payment", description: "Utilities", amount: -180, date: "Feb 25, 2026", category: "Bills" },
  { id: 6, type: "income", description: "Investment Return", amount: 850, date: "Feb 24, 2026", category: "Investment" },
];
