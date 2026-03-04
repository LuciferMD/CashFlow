import { ArrowDownRight, ArrowUpRight } from "lucide-react";
import type {Transaction} from "../model/types.ts";

interface TransactionCardProps {
  transaction: Transaction;
}

export function TransactionCard({ transaction }: TransactionCardProps) {
  return (
    <div className="flex items-center gap-4">
      <div className={`w-10 h-10 rounded-full flex items-center justify-center ${
        transaction.type === "income" 
          ? "bg-green-100 text-green-600" 
          : "bg-red-100 text-red-600"
      }`}>
        {transaction.type === "income" ? (
          <ArrowDownRight className="w-5 h-5" />
        ) : (
          <ArrowUpRight className="w-5 h-5" />
        )}
      </div>
      <div>
        <p className="font-medium">{transaction.description}</p>
        <p className="text-sm text-muted-foreground">{transaction.date}</p>
      </div>
    </div>
  );
}
