import { Award, Crown, Flame, Gem, Medal, Star, Trophy } from 'lucide-react'

function levelIcon(threshold) {
  if (threshold >= 200) {
    return Star
  }

  if (threshold >= 100) {
    return Gem
  }

  if (threshold >= 50) {
    return Crown
  }

  if (threshold >= 25) {
    return Trophy
  }

  if (threshold >= 10) {
    return Award
  }

  return Medal
}

export default function AwardMark({ kind, threshold, size = 28 }) {
  const Icon = kind === 'Streak' ? Flame : levelIcon(threshold)
  return <Icon size={size} aria-hidden="true" />
}
