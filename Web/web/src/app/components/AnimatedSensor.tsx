import { motion } from "motion/react";
import { Radio, Wifi } from "lucide-react";

interface AnimatedSensorProps {
  isAnimating: boolean;
}

export function AnimatedSensor({ isAnimating }: AnimatedSensorProps) {
  return (
    <div className="relative flex h-24 w-24 items-center justify-center">
      {[0, 1, 2].map((index) => (
        <motion.span
          key={index}
          className="absolute rounded-full border border-cyan-400/40"
          initial={{ width: 48, height: 48, opacity: 0.5 }}
          animate={
            isAnimating
              ? {
                  width: [48, 96],
                  height: [48, 96],
                  opacity: [0.5, 0],
                }
              : { width: 48, height: 48, opacity: 0.25 }
          }
          transition={{
            duration: 1.8,
            repeat: isAnimating ? Infinity : 0,
            delay: index * 0.35,
            ease: "easeOut",
          }}
        />
      ))}

      <div className="relative z-10 flex h-16 w-16 items-center justify-center rounded-2xl bg-gradient-to-br from-cyan-400 to-emerald-500 shadow-lg shadow-cyan-500/25">
        <Radio className="h-7 w-7 text-white" />
      </div>

      <Wifi className="absolute bottom-1 right-1 h-4 w-4 text-emerald-600" />
    </div>
  );
}
