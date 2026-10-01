export const configTemplates = {
  SwipeBug: JSON.stringify({
    language: 'Python',
    code: 'broj = 4\nif broj = 4:\n    print("da")\nelse:\n    print("ne")',
    hasBug: true,
    bugLine: 2,
  }, null, 2),
  FillBlanks: JSON.stringify({
    language: 'Python',
    template: 'brojevi = [1, 2, 3]\n{{1}} broj {{2}} brojevi:\n    print(broj)',
    blanks: [
      { id: '1', answer: 'for' },
      { id: '2', answer: 'in' },
    ],
    wordBank: ['for', 'in', 'while', 'if'],
  }, null, 2),
  ReorderCode: JSON.stringify({
    language: 'Python',
    goal: 'Ispiši zbroj dva broja.',
    lines: [
      { id: 'a', text: 'a = 2', indent: 0 },
      { id: 'b', text: 'b = 3', indent: 0 },
      { id: 'c', text: 'print(a + b)', indent: 0 },
    ],
    correctOrders: [['a', 'b', 'c']],
  }, null, 2),
  TraceExecution: JSON.stringify({
    language: 'Python',
    code: 'x = 1\nprint(x)',
    variables: ['x'],
    steps: [
      { line: 1, expected: { x: '1' } },
      { line: 2, expected: { x: '1' } },
    ],
    askAt: [0, 1],
  }, null, 2),
  RobotMaze: JSON.stringify({
    width: 4,
    height: 1,
    walls: [],
    start: { x: 0, y: 0, dir: 'right' },
    goal: { x: 3, y: 0 },
    stars: [[1, 0]],
    allowedBlocks: ['forward'],
    maxBlocks: 4,
    maxSteps: 8,
    optimalBlocks: 3,
  }, null, 2),
}

export function usesOptions(type) {
  return type === 'MultipleChoice' || type === 'FixBug'
}

export function usesConfig(type) {
  return Boolean(configTemplates[type])
}
