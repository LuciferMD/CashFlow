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
import { register } from "../api/Register.ts";

export function RegisterPage() {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    name: "",
    email: "",
    password: "",
    confirmPassword: "",
  });
  const [isAnimating, setIsAnimating] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleFieldFocus = () => {
    setIsAnimating(true);
    setTimeout(() => {
      setIsAnimating(false);
    }, 600);
  };

  const handleRegister = (e: React.FormEvent) => {
    e.preventDefault();

    if (formData.password !== formData.confirmPassword) {
      setError("Passwords do not match");
      return;
    }

    register(formData.name, formData.email, formData.password).then((result) => {
      if (result) {
        navigate("/dashboard");
      } else {
        setError("Something went wrong. Please try again");
      }
    });
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
              Create your monitoring account
            </CardDescription>
          </div>
        </CardHeader>
        <CardContent>
          <form onSubmit={(e) => handleRegister(e)} className="space-y-5">
            {error && <div className="text-center text-sm text-rose-600">{error}</div>}
            <div className="space-y-2">
              <Label htmlFor="name">Full Name</Label>
              <Input
                id="name"
                type="text"
                placeholder="John Doe"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                onFocus={handleFieldFocus}
                className="border-slate-200 bg-slate-50"
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="email">Email</Label>
              <Input
                id="email"
                type="email"
                placeholder="you@example.com"
                value={formData.email}
                onChange={(e) => setFormData({ ...formData, email: e.target.value })}
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
                placeholder="Create a password"
                value={formData.password}
                onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                onFocus={handleFieldFocus}
                className="border-slate-200 bg-slate-50"
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="confirmPassword">Confirm Password</Label>
              <Input
                id="confirmPassword"
                type="password"
                placeholder="Confirm your password"
                value={formData.confirmPassword}
                onChange={(e) =>
                  setFormData({ ...formData, confirmPassword: e.target.value })
                }
                onFocus={handleFieldFocus}
                className="border-slate-200 bg-slate-50"
                required
              />
            </div>
            <Button
              type="submit"
              className="mt-8 w-full bg-cyan-600 text-white hover:bg-cyan-500"
            >
              Create Account
            </Button>
            <div className="text-center text-sm">
              <span className="text-muted-foreground">Already have an account? </span>
              <button
                type="button"
                onClick={() => navigate("/")}
                className="text-cyan-700 hover:underline"
              >
                Sign in
              </button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
