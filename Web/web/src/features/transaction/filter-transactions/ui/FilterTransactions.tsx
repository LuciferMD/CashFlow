import { Filter } from "lucide-react";

interface FilterTransactionsProps {
  value: string;
  onChange: (value: string) => void;
}

export function FilterTransactions({ value, onChange }: FilterTransactionsProps) {
  return (
    <div className="flex items-center gap-2">
      <Filter className="w-4 h-4 text-muted-foreground" />
      <select
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className="px-3 py-2 border rounded-md bg-background"
      >
        <option value="all">All Time</option>
        <option value="this-week">This Week</option>
        <option value="this-month">This Month</option>
        <option value="last-month">Last Month</option>
      </select>
    </div>
  );
}
