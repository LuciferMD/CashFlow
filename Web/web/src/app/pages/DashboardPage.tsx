import { useState } from "react";
import { useNavigate } from "react-router";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "../components/ui/tabs";
import { DashboardHeader } from "../../widgets/dashboard-header";
import { BalanceCards } from "../../widgets/balance-cards";
import { BalanceChart } from "../../widgets/balance-chart";
import { IncomePaymentsChart } from "../../widgets/income-payments-chart";
import { TransactionList } from "../../widgets/transaction-list";
import { StockPortfolio } from "../../widgets/stock-portfolio";
import { calculatePortfolioValue, calculatePortfolioChangePercent } from "../../widgets/stock-portfolio/lib/calculations";
import type {Stock} from "../../entities/stock";
import type {Transaction} from "../../entities/transaction";
import {mockStocks} from "../../entities/stock";
import {mockTransactions} from "../../entities/transaction";

export function DashboardPage() {
  const navigate = useNavigate();
  const [selectedTab, setSelectedTab] = useState("overview");
  const [stocks, setStocks] = useState<Stock[]>(mockStocks);
  const [transactions, setTransactions] = useState<Transaction[]>(mockTransactions);

  const handleLogout = () => {
    navigate("/");
  };

  const handleAddStock = (stockData: {
    symbol: string;
    name: string;
    shares: string;
    purchasePrice: string;
    currentPrice: string;
  }) => {
    const stock: Stock = {
      id: stocks.length + 1,
      symbol: stockData.symbol.toUpperCase(),
      name: stockData.name,
      shares: parseFloat(stockData.shares),
      purchasePrice: parseFloat(stockData.purchasePrice),
      currentPrice: parseFloat(stockData.currentPrice),
    };
    setStocks([...stocks, stock]);
  };

  const handleDeleteStock = (id: number) => {
    setStocks(stocks.filter((stock) => stock.id !== id));
  };

  const handleAddTransaction = (transactionData: {
    type: string;
    description: string;
    amount: string;
    category: string;
  }) => {
    const transaction: Transaction = {
      id: transactions.length + 1,
      type: transactionData.type as "income" | "payment",
      description: transactionData.description,
      amount: transactionData.type === "income" 
        ? parseFloat(transactionData.amount) 
        : -parseFloat(transactionData.amount),
      date: new Date().toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' }),
      category: transactionData.category,
    };
    setTransactions([transaction, ...transactions]);
  };

  const handleDeleteTransaction = (id: number) => {
    setTransactions(transactions.filter((transaction) => transaction.id !== id));
  };

  // Calculate metrics
  const currentBalance = 22000;
  const totalIncome = 12000;
  const totalPayments = 7000;
  const totalPortfolioValue = calculatePortfolioValue(stocks);
  const portfolioChange = calculatePortfolioChangePercent(stocks);

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100">
      <DashboardHeader onLogout={handleLogout} />

      <main className="container mx-auto px-4 py-8">
        <div className="mb-8">
          <BalanceCards
            currentBalance={currentBalance}
            totalIncome={totalIncome}
            totalPayments={totalPayments}
            totalPortfolioValue={totalPortfolioValue}
            portfolioChange={portfolioChange}
          />
        </div>

        <Tabs value={selectedTab} onValueChange={setSelectedTab} className="space-y-6">
          <TabsList className="grid w-full grid-cols-3 max-w-2xl">
            <TabsTrigger value="overview">Balance Overview</TabsTrigger>
            <TabsTrigger value="income">Income & Payments</TabsTrigger>
            <TabsTrigger value="stocks">Stock Prices</TabsTrigger>
          </TabsList>

          <TabsContent value="overview" className="space-y-6">
            <BalanceChart />
          </TabsContent>

          <TabsContent value="income" className="space-y-6">
            <IncomePaymentsChart />
            <TransactionList
              transactions={transactions}
              onAddTransaction={handleAddTransaction}
              onDeleteTransaction={handleDeleteTransaction}
            />
          </TabsContent>

          <TabsContent value="stocks" className="space-y-6">
            <StockPortfolio
              stocks={stocks}
              onAddStock={handleAddStock}
              onDeleteStock={handleDeleteStock}
            />
          </TabsContent>
        </Tabs>
      </main>
    </div>
  );
}
