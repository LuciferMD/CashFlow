interface AggregationSummaryProps {
  totalIncome: number;
  totalExpenses: number;
  netAmount: number;
  count: number;
}

export function AggregationSummary({ totalIncome, totalExpenses, netAmount, count }: AggregationSummaryProps) {
  return (
    <div className="p-4 bg-accent/50 rounded-lg border">
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <div>
          <p className="text-xs text-muted-foreground mb-1">Transactions</p>
          <p className="text-lg font-semibold">{count}</p>
        </div>
        <div>
          <p className="text-xs text-muted-foreground mb-1">Total Income</p>
          <p className="text-lg font-semibold text-green-600">
            ${totalIncome.toLocaleString()}
          </p>
        </div>
        <div>
          <p className="text-xs text-muted-foreground mb-1">Total Expenses</p>
          <p className="text-lg font-semibold text-red-600">
            ${totalExpenses.toLocaleString()}
          </p>
        </div>
        <div>
          <p className="text-xs text-muted-foreground mb-1">Net Amount</p>
          <p className={`text-lg font-semibold ${netAmount >= 0 ? 'text-green-600' : 'text-red-600'}`}>
            {netAmount >= 0 ? '+' : ''}${netAmount.toLocaleString()}
          </p>
        </div>
      </div>
    </div>
  );
}
