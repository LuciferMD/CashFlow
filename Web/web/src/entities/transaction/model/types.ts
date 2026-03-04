export interface Transaction {
  id: number;
  type: "income" | "payment";
  description: string;
  amount: number;
  date: string;
  category: string;
}
