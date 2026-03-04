import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "../../../app/components/ui/card";
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from "recharts";

const incomePaymentsData = [
  { month: "Jan", income: 8000, payments: 6000 },
  { month: "Feb", income: 9500, payments: 6500 },
  { month: "Mar", income: 8200, payments: 7300 },
  { month: "Apr", income: 11000, payments: 7000 },
  { month: "May", income: 10000, payments: 8000 },
  { month: "Jun", income: 12000, payments: 7000 },
];

export function IncomePaymentsChart() {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Income vs Payments</CardTitle>
        <CardDescription>Comparison of your income and payments over time</CardDescription>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={350}>
          <BarChart data={incomePaymentsData}>
            <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
            <XAxis dataKey="month" stroke="#6b7280" />
            <YAxis stroke="#6b7280" />
            <Tooltip 
              contentStyle={{ 
                backgroundColor: 'white', 
                border: '1px solid #e5e7eb',
                borderRadius: '8px'
              }}
            />
            <Bar dataKey="income" fill="#10b981" radius={[8, 8, 0, 0]} />
            <Bar dataKey="payments" fill="#ef4444" radius={[8, 8, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
}