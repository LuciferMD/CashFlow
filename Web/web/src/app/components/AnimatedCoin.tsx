import { motion } from "motion/react";
import { DollarSign } from "lucide-react";

interface AnimatedCoinProps {
  isAnimating: boolean;
}

export function AnimatedCoin({ isAnimating }: AnimatedCoinProps) {
  return (
    <motion.div
      className="w-16 h-16 bg-gradient-to-br from-blue-600 to-indigo-600 rounded-full flex items-center justify-center relative overflow-hidden"
      animate={{
        rotateY: isAnimating ? 360 : 0,
        y: isAnimating ? [-10, 0] : 0,
      }}
      transition={{
        rotateY: {
          duration: 0.6,
          ease: "easeInOut",
        },
        y: {
          duration: 0.3,
          ease: "easeOut",
        },
      }}
    >
      {/* Shine effect */}
      <motion.div
        className="absolute inset-0 bg-gradient-to-r from-transparent via-white to-transparent opacity-20"
        animate={{
          x: isAnimating ? ["0%", "200%"] : "0%",
        }}
        transition={{
          duration: 0.6,
          ease: "linear",
        }}
      />
      
      {/* Dollar sign */}
      <motion.div
        animate={{
          scale: isAnimating ? [1, 1.15, 1] : 1,
        }}
        transition={{
          duration: 0.6,
          ease: "easeInOut",
        }}
      >
        <DollarSign className="w-10 h-10 text-white relative z-10" />
      </motion.div>

      {/* Sparkle effects - always rendered but with opacity */}
      <motion.div
        className="absolute w-2 h-2 bg-yellow-300 rounded-full"
        style={{ top: "50%", left: "50%", marginTop: -4, marginLeft: -4 }}
        animate={{
          opacity: isAnimating ? [0, 1, 0] : 0,
          scale: isAnimating ? [0, 1, 0] : 0,
          x: isAnimating ? [0, 30] : 0,
          y: isAnimating ? [0, -30] : 0,
        }}
        transition={{
          duration: 0.6,
          ease: "easeOut",
        }}
      />
      <motion.div
        className="absolute w-2 h-2 bg-yellow-300 rounded-full"
        style={{ top: "50%", left: "50%", marginTop: -4, marginLeft: -4 }}
        animate={{
          opacity: isAnimating ? [0, 1, 0] : 0,
          scale: isAnimating ? [0, 1, 0] : 0,
          x: isAnimating ? [0, -30] : 0,
          y: isAnimating ? [0, -30] : 0,
        }}
        transition={{
          duration: 0.6,
          ease: "easeOut",
        }}
      />
      <motion.div
        className="absolute w-2 h-2 bg-yellow-300 rounded-full"
        style={{ top: "50%", left: "50%", marginTop: -4, marginLeft: -4 }}
        animate={{
          opacity: isAnimating ? [0, 1, 0] : 0,
          scale: isAnimating ? [0, 1, 0] : 0,
          x: isAnimating ? [0, 20] : 0,
          y: isAnimating ? [0, 30] : 0,
        }}
        transition={{
          duration: 0.6,
          ease: "easeOut",
        }}
      />
    </motion.div>
  );
}