import { useState } from "react";
import { useNavigate } from "react-router";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "../../../components/ui/card.tsx";
import { Input } from "../../../components/ui/input.tsx";
import { Label } from "../../../components/ui/label.tsx";
import { Button } from "../../../components/ui/button.tsx";
import { AnimatedSensor } from "../../../components/AnimatedSensor.tsx";
import { login } from "../api/Login.ts";

export function LoginPage() {
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [isAnimating, setIsAnimating] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleFieldFocus = () => {
    setIsAnimating(true);
    setTimeout(() => {
      setIsAnimating(false);
    }, 600);
  };

  const handleLogin = (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (email && password) {
      login(email, password)
        .then((result) => {
          if (result) {
            navigate("/dashboard");
          } else {
            setError("Invalid email or password");
          }
        })
        .catch(() => setError("Something went wrong. Please try again"));
    }
  };

  return (
    <div className="min-h-screen bg-[radial-gradient(circle_at_top,_rgba(34,211,238,0.12),_transparent_40%),linear-gradient(to_bottom,_#f8fafc,_#ecfeff)] p-4 flex items-center justify-center">
      <Card className="w-full max-w-md border-slate-200 bg-white/95 text-slate-900 shadow-xl shadow-slate-200/60">
        <CardHeader className="space-y-4 text-center">
          <div className="mx-auto">
            <AnimatedSensor isAnimating={isAnimating} />
          </div>
          <div>
            <CardTitle className="text-3xl text-slate-900">SensorHub</CardTitle>
            <CardDescription className="text-slate-500">
              Sign in to monitor your smart home sensors
            </CardDescription>
          </div>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleLogin} className="space-y-6">
            {error && (
              <div className="text-center text-sm text-rose-600">{error}</div>
            )}
            <div className="space-y-2">
              <Label htmlFor="email">Email</Label>
              <Input
                id="email"
                type="email"
                placeholder="you@example.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                onFocus={handleFieldFocus}
                className="border-slate-200 bg-slate-50"
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="password">Password</Label>
              <Input
                id="password"
                type="password"
                placeholder="Enter your password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                onFocus={handleFieldFocus}
                className="border-slate-200 bg-slate-50"
                required
              />
            </div>
            <Button
              type="submit"
              className="mt-8 w-full bg-cyan-600 text-white hover:bg-cyan-500"
            >
              Sign In
            </Button>
            <div className="text-center text-sm">
              <span className="text-muted-foreground">Don't have an account? </span>
              <button
                type="button"
                onClick={() => navigate("/register")}
                className="text-cyan-700 hover:underline"
              >
                Sign up
              </button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
