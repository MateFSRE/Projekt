const typeLabels = {
  PredictOutput: 'Predviđanje ispisa',
  MultipleChoice: 'Odabir točnog odgovora',
  FixBug: 'Ispravljanje greške',
  SwipeBug: 'Ima li bug?',
  FillBlanks: 'Dopuni rupe',
  ReorderCode: 'Složi kod',
  TraceExecution: 'Prati izvršavanje',
  RobotMaze: 'Robot u labirintu',
}

export function challengeTypeLabel(type) {
  return typeLabels[type] ?? type
}

export function usesOptions(type) {
  return type === 'MultipleChoice' || type === 'FixBug'
}
