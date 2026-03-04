import type {Transaction} from "../../../entities/transaction";

interface TransactionAggregations {
  totalIncome: number;
  totalExpenses: number;
  netAmount: number;
  count: number;
}

export function calculateTransactionAggregations(transactions: Transaction[]): TransactionAggregations {
  const totalIncome = transactions
    .filter((t) => t.type === "income")
    .reduce((sum, t) => sum + t.amount, 0);
  
  const totalExpenses = Math.abs(
    transactions
      .filter((t) => t.type === "payment")
      .reduce((sum, t) => sum + t.amount, 0)
  );
  
  const netAmount = totalIncome - totalExpenses;

  return {
    totalIncome,
    totalExpenses,
    netAmount,
    count: transactions.length,
  };
}
