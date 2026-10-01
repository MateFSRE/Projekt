// Isticanje sintakse dolazi kad se odobri paket react-syntax-highlighter.
export default function CodeSnippet({ code }) {
  return (
    <pre className="challenge-code">
      <code>{code}</code>
    </pre>
  )
}
