import { Search } from "lucide-react";
import { Input } from "../../../../app/components/ui/input";

interface SearchTransactionsProps {
  value: string;
  onChange: (value: string) => void;
}

export function SearchTransactions({ value, onChange }: SearchTransactionsProps) {
  return (
    <div className="relative flex-1">
      <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-muted-foreground" />
      <Input
        placeholder="Search transactions..."
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className="pl-9"
      />
    </div>
  );
}