import FillBlanksGame from './FillBlanksGame.jsx'
import FixBugGame from './FixBugGame.jsx'
import MultipleChoiceGame from './MultipleChoiceGame.jsx'
import PredictOutputGame from './PredictOutputGame.jsx'
import ReorderCodeGame from './ReorderCodeGame.jsx'
import RobotMazeGame from './RobotMazeGame.jsx'
import TraceExecutionGame from './TraceExecutionGame.jsx'
import SwipeBugGame from './SwipeBugGame.jsx'

// Ključ je string ChallengeType koji API vrati. Nova igra: komponenta + jedan red ovdje.
const games = {
  PredictOutput: PredictOutputGame,
  MultipleChoice: MultipleChoiceGame,
  FixBug: FixBugGame,
  SwipeBug: SwipeBugGame,
  FillBlanks: FillBlanksGame,
  ReorderCode: ReorderCodeGame,
  TraceExecution: TraceExecutionGame,
  RobotMaze: RobotMazeGame,
}

export function getGameComponent(type) {
  return games[type] ?? null
}
