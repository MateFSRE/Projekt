import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  ArrowDownUp,
  Bot,
  Bug,
  Check,
  Footprints,
  ListChecks,
  Terminal,
  TextCursorInput,
  Wrench,
} from 'lucide-react'
import { challengeTypeLabel } from '../components/challenge/challengeTypes.js'
import { gameTypeIds, readGameSelection, writeGameSelection } from '../games/selection.js'

const icons = {
  PredictOutput: Terminal,
  MultipleChoice: ListChecks,
  FixBug: Wrench,
  SwipeBug: Bug,
  FillBlanks: TextCursorInput,
  ReorderCode: ArrowDownUp,
  TraceExecution: Footprints,
  RobotMaze: Bot,
}

export default function GamesPage() {
  const navigate = useNavigate()
  const [selected, setSelected] = useState(() => new Set(readGameSelection()))

  function toggle(id) {
    const next = new Set(selected)
    if (next.has(id)) {
      next.delete(id)
    } else {
      next.add(id)
    }

    const ids = gameTypeIds.filter((type) => next.has(type))
    setSelected(new Set(ids))
    writeGameSelection(ids)
  }

  return (
    <section className="page page-wide">
      <h1>Igre</h1>
      <p>Prvo odaberi koje vrste igara ulaze u igru. Zatim dobivaš nasumičan zadatak koji još nisi točno riješio, samo iz odabranih vrsta.</p>
      <div className="game-grid" role="group" aria-label="Vrste igara">
        {gameTypeIds.map((id) => {
          const Icon = icons[id]
          const on = selected.has(id)
          return (
            <button
              key={id}
              type="button"
              className={on ? 'game-pick game-pick-on' : 'game-pick'}
              aria-pressed={on}
              onClick={() => toggle(id)}
            >
              <span className="game-pick-row">
                <span className="game-pick-icon" aria-hidden="true">
                  <Icon size={28} />
                </span>
                <Check className={on ? 'game-pick-check' : 'game-pick-check is-off'} size={18} aria-hidden="true" />
              </span>
              <span>{challengeTypeLabel(id)}</span>
            </button>
          )
        })}
      </div>
      {selected.size === 0 ? <p className="empty">Odaberi barem jednu vrstu.</p> : null}
      <button type="button" className="button" disabled={selected.size === 0} onClick={() => navigate('/games/play')}>
        Otvori igru
      </button>
    </section>
  )
}
