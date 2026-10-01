import { useEffect, useRef, useState } from 'react'

const labels = {
  forward: 'Naprijed',
  turnLeft: 'Skreni lijevo',
  turnRight: 'Skreni desno',
  repeat: 'Ponovi',
}

const outcomes = {
  reachedGoal: 'Robot je stigao do cilja.',
  hitWall: 'Robot je udario u zid ili izašao iz mreže.',
  outOfSteps: 'Program je prekinut jer je potrošio dozvoljene korake.',
  didNotReachGoal: 'Program je završio, ali robot nije na cilju.',
}

function strip(list) {
  return list.map((item) => (
    item.type === 'repeat'
      ? { type: 'repeat', times: Number(item.times) || 1, body: strip(item.body ?? []) }
      : { type: item.type }
  ))
}

function mapTree(list, id, change) {
  return list.flatMap((item) => {
    if (item.id === id) {
      const next = change(item)
      return next ? [next] : []
    }

    if (item.type === 'repeat') {
      return [{ ...item, body: mapTree(item.body ?? [], id, change) }]
    }

    return [item]
  })
}

function findParent(list, id, parent = null) {
  for (let index = 0; index < list.length; index += 1) {
    const item = list[index]
    if (item.id === id) {
      return { list, index, parent }
    }

    if (item.type === 'repeat') {
      const nested = findParent(item.body ?? [], id, item)
      if (nested) {
        return nested
      }
    }
  }

  return null
}

export default function RobotMazeGame({ challenge, onSubmit, result, loading }) {
  const payload = challenge.payload ?? {}
  const locked = Boolean(result) || loading
  const nextId = useRef(1)
  const [blocks, setBlocks] = useState([])
  const [selectedId, setSelectedId] = useState(null)
  const [frame, setFrame] = useState(0)
  const [replay, setReplay] = useState(0)
  const blocksRef = useRef(blocks)
  blocksRef.current = blocks

  const path = result?.details?.path ?? []
  const pose = path[frame] ?? payload.start ?? { x: 0, y: 0, dir: 'right' }
  const visited = new Set(path.slice(0, frame + 1).map((item) => `${item.x},${item.y}`))

  useEffect(() => {
    if (!path.length) {
      return undefined
    }

    const reduce = window.matchMedia('(prefers-reduced-motion: reduce)').matches
    if (reduce) {
      setFrame(path.length - 1)
      return undefined
    }

    setFrame(0)
    const timer = setInterval(() => {
      setFrame((index) => {
        if (index >= path.length - 1) {
          clearInterval(timer)
          return index
        }

        return index + 1
      })
    }, 380)

    return () => clearInterval(timer)
  }, [result, replay])

  function createBlock(type) {
    const id = `b${nextId.current}`
    nextId.current += 1
    if (type === 'repeat') {
      return { id, type, times: 2, body: [] }
    }

    return { id, type }
  }

  function placeList(parent, list) {
    if (!parent) {
      setBlocks(list)
      return
    }

    setBlocks(mapTree(blocksRef.current, parent.id, (item) => ({ ...item, body: list })))
  }

  function addBlock(type) {
    if (locked) {
      return
    }

    const block = createBlock(type)
    const selected = findParent(blocks, selectedId)
    const selectedItem = selected?.list[selected.index]
    const last = blocks[blocks.length - 1]

    if (type === 'repeat' && selectedItem && selectedItem.type !== 'repeat') {
      const repeat = { ...block, body: [selectedItem] }
      const updated = selected.list.map((item) => (item.id === selectedItem.id ? repeat : item))
      placeList(selected.parent, updated)
      setSelectedId(repeat.id)
      return
    }

    if (selectedItem?.type === 'repeat') {
      setBlocks(mapTree(blocks, selectedId, (item) => ({ ...item, body: [...(item.body ?? []), block] })))
      if (type === 'repeat') {
        setSelectedId(block.id)
      }
      return
    }

    if (type === 'repeat' && last && last.type !== 'repeat') {
      const repeat = { ...block, body: [last] }
      setBlocks([...blocks.slice(0, -1), repeat])
      setSelectedId(repeat.id)
      return
    }

    if (type !== 'repeat' && last?.type === 'repeat' && (selectedId === last.id || ((last.body ?? []).length === 0 && !selectedId))) {
      setBlocks(mapTree(blocks, last.id, (item) => ({ ...item, body: [...(item.body ?? []), block] })))
      setSelectedId(last.id)
      return
    }

    setBlocks([...blocks, block])
    if (type === 'repeat') {
      setSelectedId(block.id)
    }
  }

  function removeBlock(id) {
    setBlocks(mapTree(blocks, id, () => null))
    if (selectedId === id) {
      setSelectedId(null)
    }
  }

  function moveBlock(id, delta) {
    const tree = blocksRef.current
    const located = findParent(tree, id)
    if (!located) {
      return
    }

    const item = located.list[located.index]
    const neighbor = located.list[located.index + delta]

    if (neighbor?.type === 'repeat') {
      const without = located.list.filter((entry) => entry.id !== id)
      const body = delta > 0 ? [item, ...(neighbor.body ?? [])] : [...(neighbor.body ?? []), item]
      const updated = without.map((entry) => (entry.id === neighbor.id ? { ...entry, body } : entry))
      placeList(located.parent, updated)
      return
    }

    if ((delta < 0 && located.index === 0) || (delta > 0 && located.index === located.list.length - 1)) {
      if (!located.parent) {
        return
      }

      const parentLocation = findParent(tree, located.parent.id)
      if (!parentLocation) {
        return
      }

      const without = located.list.filter((entry) => entry.id !== id)
      const parentList = parentLocation.list.slice()
      const updatedParent = { ...located.parent, body: without }
      parentList.splice(parentLocation.index, 1, updatedParent)
      parentList.splice(delta < 0 ? parentLocation.index : parentLocation.index + 1, 0, item)
      placeList(parentLocation.parent, parentList)
      return
    }

    const target = located.index + delta
    if (target < 0 || target >= located.list.length) {
      return
    }

    const next = located.list.slice()
    const [moved] = next.splice(located.index, 1)
    next.splice(target, 0, moved)
    placeList(located.parent, next)
  }

  function moveIntoRepeat(id, repeatId) {
    const tree = blocksRef.current
    const source = findParent(tree, id)
    const repeatLocation = findParent(tree, repeatId)
    const repeat = repeatLocation?.list[repeatLocation.index]
    if (!source || !repeat || repeat.type !== 'repeat' || source.parent?.id === repeatId) {
      return
    }

    if (findParent([source.list[source.index]], repeatId)) {
      return
    }

    const item = source.list[source.index]
    const without = source.list.filter((entry) => entry.id !== id)
    const removed = source.parent
      ? mapTree(tree, source.parent.id, (parent) => ({ ...parent, body: without }))
      : without
    setBlocks(mapTree(removed, repeatId, (parent) => ({ ...parent, body: [...(parent.body ?? []), item] })))
    setSelectedId(repeatId)
  }

  function onHandlePointerDown(event, id) {
    if (locked || event.button !== 0) {
      return
    }

    event.preventDefault()
    event.stopPropagation()
    const startY = event.clientY
    let moved = false

    function move(moveEvent) {
      if (Math.abs(moveEvent.clientY - startY) < 12) {
        return
      }

      moved = true
      const under = document.elementFromPoint(moveEvent.clientX, moveEvent.clientY)
      const row = under?.closest('[data-block-id]')
      if (!row || row.dataset.blockId === id) {
        return
      }

      const source = findParent(blocksRef.current, id)
      const target = findParent(blocksRef.current, row.dataset.blockId)
      if (!source || !target || source.parent?.id !== target.parent?.id) {
        return
      }

      const delta = target.index - source.index
      if (delta !== 0) {
        moveBlock(id, delta)
      }
    }

    function up(upEvent) {
      window.removeEventListener('pointermove', move)
      window.removeEventListener('pointerup', up)
      if (!moved) {
        setSelectedId(id)
        return
      }

      const under = document.elementFromPoint(upEvent.clientX, upEvent.clientY)
      const drop = under?.closest('[data-repeat-id]')
      if (drop?.dataset.repeatId) {
        moveIntoRepeat(id, drop.dataset.repeatId)
      }
    }

    window.addEventListener('pointermove', move)
    window.addEventListener('pointerup', up)
  }

  function reset() {
    if (!result) {
      setBlocks([])
      setSelectedId(null)
      return
    }

    setReplay((value) => value + 1)
  }

  async function run(event) {
    event.preventDefault()
    if (locked) {
      return
    }

    await onSubmit(strip(blocks))
  }

  const width = payload.width ?? 1
  const height = payload.height ?? 1
  const walls = new Set((payload.walls ?? []).map((wall) => `${wall[0]},${wall[1]}`))
  const stars = new Set((payload.stars ?? []).map((star) => `${star[0]},${star[1]}`))
  const goal = payload.goal ?? { x: 0, y: 0 }
  const cells = []
  for (let y = 0; y < height; y += 1) {
    for (let x = 0; x < width; x += 1) {
      const key = `${x},${y}`
      const className = [
        'maze-cell',
        walls.has(key) ? 'maze-wall' : '',
        goal.x === x && goal.y === y ? 'maze-goal' : '',
      ].filter(Boolean).join(' ')
      cells.push(
        <div key={key} className={className}>
          {stars.has(key) && !visited.has(key) ? <span className="maze-star">★</span> : null}
          {pose.x === x && pose.y === y ? <span className={`maze-robot maze-robot-${pose.dir}`} /> : null}
        </div>,
      )
    }
  }

  const outcome = result?.details?.outcome

  return (
    <article className="maze-game">
      <p className="challenge-question">Složi blokove i pokreni robota. Cilj je zeleno polje.</p>
      <div className="maze-layout">
        <div className="maze-grid" style={{ gridTemplateColumns: `repeat(${width}, minmax(2.4rem, 1fr))` }}>
          {cells}
        </div>
        <div className="maze-program">
          <div className="maze-palette">
            {(payload.allowedBlocks ?? []).map((type) => (
              <button key={type} type="button" className="nudge" disabled={locked} onClick={() => addBlock(type)}>
                {labels[type] ?? type}
              </button>
            ))}
          </div>
          <BlockList
            items={blocks}
            parentId="root"
            selectedId={selectedId}
            locked={locked}
            onSelect={setSelectedId}
            onRemove={removeBlock}
            onMove={moveBlock}
            onTimes={(id, times) => setBlocks(mapTree(blocks, id, (item) => ({ ...item, times })))}
            onDrag={onHandlePointerDown}
          />
          <p className="trace-quiet">
            Najviše {payload.maxBlocks} blokova. Bonus ako ih je {payload.optimalBlocks} ili manje. Svaka zvjezdica uz cilj donosi dodatne bodove. Naredba uz Ponovi ulazi u petlju. Strelica je vadi van ako treba ići samo jednom.
          </p>
        </div>
      </div>
      <div className="trace-nav">
        <button type="button" className="maze-run" onClick={run} disabled={locked}>Pokreni</button>
        <button type="button" onClick={reset}>Resetiraj</button>
      </div>
      {outcome ? (
        <p className={outcome === 'reachedGoal' ? 'maze-outcome maze-outcome-ok' : 'maze-outcome'}>
          {outcomes[outcome] ?? outcome}
          {' '}
          Zvjezdice: {result.details.starsCollected}/{result.details.starsTotal}. Blokova: {result.details.blocksUsed}.
        </p>
      ) : null}
    </article>
  )
}

function BlockList({ items, parentId, selectedId, locked, onSelect, onRemove, onMove, onTimes, onDrag }) {
  if (!items.length) {
    if (parentId === 'root') {
      return <p className="maze-empty">Radna površina je prazna. Klik na naredbu je dodaje.</p>
    }

    return (
      <button type="button" className="maze-empty-slot" data-repeat-id={parentId} onClick={() => onSelect(parentId)}>
        Ovdje ide naredba koja se ponavlja.
      </button>
    )
  }

  return (
    <ol className="maze-blocks">
      {items.map((item) => (
        <li
          key={item.id}
          data-block-id={item.id}
          data-parent={parentId}
          data-repeat-id={item.type === 'repeat' ? item.id : undefined}
          className={selectedId === item.id ? 'maze-block maze-block-selected' : 'maze-block'}
          onClick={(event) => {
            event.stopPropagation()
            onSelect(item.id)
          }}
        >
          <div className="maze-block-row">
            <button type="button" className="maze-handle nudge" disabled={locked} onPointerDown={(event) => onDrag(event, item.id)}>
              {labels[item.type]}
            </button>
            {item.type === 'repeat' ? (
              <input
                className="maze-times"
                type="number"
                min="1"
                max="10"
                value={item.times}
                disabled={locked}
                aria-label="Broj ponavljanja"
                onChange={(event) => onTimes(item.id, Math.max(1, Math.min(10, Number(event.target.value) || 1)))}
              />
            ) : null}
            <button type="button" className="nudge" disabled={locked} onClick={() => onMove(item.id, -1)} aria-label="Gore">↑</button>
            <button type="button" className="nudge" disabled={locked} onClick={() => onMove(item.id, 1)} aria-label="Dolje">↓</button>
            <button type="button" className="nudge" disabled={locked} onClick={() => onRemove(item.id)} aria-label="Ukloni">×</button>
          </div>
          {item.type === 'repeat' ? (
            <div className="maze-loop" data-repeat-id={item.id}>
              <BlockList
                items={item.body ?? []}
                parentId={item.id}
                selectedId={selectedId}
                locked={locked}
                onSelect={onSelect}
                onRemove={onRemove}
                onMove={onMove}
                onTimes={onTimes}
                onDrag={onDrag}
              />
            </div>
          ) : null}
        </li>
      ))}
    </ol>
  )
}
