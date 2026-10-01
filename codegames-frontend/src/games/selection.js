const storageKey = 'codegames.gameTypes'

export const gameTypeIds = [
  'PredictOutput',
  'MultipleChoice',
  'FixBug',
  'SwipeBug',
  'FillBlanks',
  'ReorderCode',
  'TraceExecution',
  'RobotMaze',
]

const known = new Set(gameTypeIds)

export function readGameSelection() {
  try {
    const raw = localStorage.getItem(storageKey)
    if (!raw) {
      return [...gameTypeIds]
    }

    const parsed = JSON.parse(raw)
    if (!Array.isArray(parsed)) {
      return [...gameTypeIds]
    }

    return parsed.filter((id) => known.has(id))
  } catch {
    return [...gameTypeIds]
  }
}

export function writeGameSelection(ids) {
  localStorage.setItem(storageKey, JSON.stringify(ids))
}
