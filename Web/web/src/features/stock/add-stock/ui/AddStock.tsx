import { useState } from "react";
import { Plus } from "lucide-react";
import { Button } from "../../../../app/components/ui/button";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger } from "../../../../app/components/ui/dialog";
import { Input } from "../../../../app/components/ui/input";
import { Label } from "../../../../app/components/ui/label";

interface AddStockProps {
  onAdd: (stock: {
    symbol: string;
    name: string;
    shares: string;
    purchasePrice: string;
    currentPrice: string;
  }) => void;
}

export function AddStock({ onAdd }: AddStockProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [formData, setFormData] = useState({
    symbol: "",
    name: "",
    shares: "",
    purchasePrice: "",
    currentPrice: "",
  });

  const handleSubmit = () => {
    if (formData.symbol && formData.name && formData.shares && formData.purchasePrice && formData.currentPrice) {
      onAdd(formData);
      setFormData({
        symbol: "",
        name: "",
        shares: "",
        purchasePrice: "",
        currentPrice: "",
      });
      setIsOpen(false);
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={setIsOpen}>
      <DialogTrigger asChild>
        <Button>
          <Plus className="w-4 h-4 mr-2" />
          Add Stock
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Add New Stock</DialogTitle>
          <DialogDescription>Enter the details of the stock you want to add to your portfolio.</DialogDescription>
        </DialogHeader>
        <div className="space-y-4 py-4">
          <div className="space-y-2">
            <Label htmlFor="symbol">Stock Symbol</Label>
            <Input
              id="symbol"
              placeholder="e.g., AAPL"
              value={formData.symbol}
              onChange={(e) => setFormData({ ...formData, symbol: e.target.value })}
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="name">Company Name</Label>
            <Input
              id="name"
              placeholder="e.g., Apple Inc."
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="shares">Number of Shares</Label>
            <Input
              id="shares"
              type="number"
              placeholder="e.g., 10"
              value={formData.shares}
              onChange={(e) => setFormData({ ...formData, shares: e.target.value })}
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="purchasePrice">Purchase Price</Label>
            <Input
              id="purchasePrice"
              type="number"
              placeholder="e.g., 150.00"
              value={formData.purchasePrice}
              onChange={(e) => setFormData({ ...formData, purchasePrice: e.target.value })}
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="currentPrice">Current Price</Label>
            <Input
              id="currentPrice"
              type="number"
              placeholder="e.g., 162.00"
              value={formData.currentPrice}
              onChange={(e) => setFormData({ ...formData, currentPrice: e.target.value })}
            />
          </div>
        </div>
        <div className="flex justify-end gap-3">
          <Button variant="outline" onClick={() => setIsOpen(false)}>
            Cancel
          </Button>
          <Button onClick={handleSubmit}>
            Add Stock
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}