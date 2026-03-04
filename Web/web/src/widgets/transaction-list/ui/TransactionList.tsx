import { useState } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "../../../app/components/ui/card";
import type {Transaction} from "../../../entities/transaction";
import { AddTransaction } from "../../../features/transaction/add-transaction";
import { DeleteTransaction } from "../../../features/transaction/delete-transaction";
import { SearchTransactions } from "../../../features/transaction/search-transactions";
import { FilterTransactions, filterTransactionsBySearch, filterTransactionsByTime } from "../../../features/transaction/filter-transactions";
import { calculateTransactionAggregations } from "../lib/calculations";
import { AggregationSummary } from "./AggregationSummary";
import {TransactionCard} from "../../../entities/transaction";


interface TransactionListProps {
  transactions: Transaction[];
  onAddTransaction: (transaction: { type: string; description: string; amount: string; category: string }) => void;
  onDeleteTransaction: (id: number) => void;
}

export function TransactionList({ transactions, onAddTransaction, onDeleteTransaction }: TransactionListProps) {
  const [searchTerm, setSearchTerm] = useState("");
  const [timeFilter, setTimeFilter] = useState("all");

  // Apply filters
  let filteredTransactions = filterTransactionsBySearch(transactions, searchTerm);
  filteredTransactions = filterTransactionsByTime(filteredTransactions, timeFilter);

  const aggregations = calculateTransactionAggregations(filteredTransactions);
  const showAggregations = searchTerm || timeFilter !== "all";

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <div>
            <CardTitle>Recent Transactions</CardTitle>
            <CardDescription>Your latest financial activities</CardDescription>
          </div>
          <AddTransaction onAdd={onAddTransaction} />
        </div>

        {/* Search and Filter Controls */}
        <div className="mt-4 flex flex-col sm:flex-row gap-3">
          <SearchTransactions value={searchTerm} onChange={setSearchTerm} />
          <FilterTransactions value={timeFilter} onChange={setTimeFilter} />
        </div>

        {/* Aggregation Summary */}
        {showAggregations && (
          <div className="mt-4">
            <AggregationSummary {...aggregations} />
          </div>
        )}
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          {filteredTransactions.length === 0 ? (
            <div className="text-center py-12 text-muted-foreground">
              <p>No transactions found.</p>
              <p className="text-sm mt-2">Try adjusting your search or filters.</p>
            </div>
          ) : (
            filteredTransactions.map((transaction) => (
              <div 
                key={transaction.id}
                className="flex items-center justify-between p-4 border rounded-lg hover:bg-accent transition-colors"
              >
                <TransactionCard transaction={transaction} />
                <div className="flex items-center gap-4">
                  <div className="text-right">
                    <p className={`font-semibold ${
                      transaction.type === "income" ? "text-green-600" : "text-red-600"
                    }`}>
                      {transaction.amount > 0 ? "+" : ""}${Math.abs(transaction.amount).toLocaleString()}
                    </p>
                    <p className="text-sm text-muted-foreground">{transaction.category}</p>
                  </div>
                  <DeleteTransaction onDelete={() => onDeleteTransaction(transaction.id)} />
                </div>
              </div>
            ))
          )}
        </div>
      </CardContent>
    </Card>
  );
}