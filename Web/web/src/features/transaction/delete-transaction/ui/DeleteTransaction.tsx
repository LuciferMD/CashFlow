import { Trash2 } from "lucide-react";
import { Button } from "../../../../app/components/ui/button";

interface DeleteTransactionProps {
  onDelete: () => void;
}

export function DeleteTransaction({ onDelete }: DeleteTransactionProps) {
  return (
    <Button
      variant="ghost"
      size="sm"
      onClick={onDelete}
      className="text-red-600 hover:text-red-700 hover:bg-red-50"
    >
      <Trash2 className="w-4 h-4" />
    </Button>
  );
}