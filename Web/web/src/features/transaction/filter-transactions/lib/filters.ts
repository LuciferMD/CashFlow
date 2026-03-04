import type {Transaction} from "../../../../entities/transaction";


export function filterTransactionsBySearch(
  transactions: Transaction[],
  searchTerm: string
): Transaction[] {
  if (!searchTerm) return transactions;

  return transactions.filter((transaction) =>
    transaction.description.toLowerCase().includes(searchTerm.toLowerCase()) ||
    transaction.category.toLowerCase().includes(searchTerm.toLowerCase())
  );
}

export function filterTransactionsByTime(
  transactions: Transaction[],
  timeFilter: string
): Transaction[] {
  if (timeFilter === "all") return transactions;

  const today = new Date("Mar 2, 2026");
  const currentMonth = today.getMonth();
  const currentYear = today.getFullYear();

  return transactions.filter((transaction) => {
    const transactionDate = new Date(transaction.date);
    const transactionMonth = transactionDate.getMonth();
    const transactionYear = transactionDate.getFullYear();

    switch (timeFilter) {
      case "this-week":
        const weekAgo = new Date(today);
        weekAgo.setDate(today.getDate() - 7);
        return transactionDate >= weekAgo && transactionDate <= today;
      
      case "this-month":
        return transactionMonth === currentMonth && transactionYear === currentYear;
      
      case "last-month":
        const lastMonth = currentMonth === 0 ? 11 : currentMonth - 1;
        const lastMonthYear = currentMonth === 0 ? currentYear - 1 : currentYear;
        return transactionMonth === lastMonth && transactionYear === lastMonthYear;
      
      default:
        return true;
    }
  });
}
