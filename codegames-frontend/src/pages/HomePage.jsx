import { Link } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext.jsx'

export default function HomePage() {
  const { user } = useAuth()

  return (
    <section className="page">
      <h1>Uči programiranje kroz kratke igre</h1>
      <p>
        CodeGames je web aplikacija za učenje programiranja na primjerima u
        Pythonu i JavaScriptu. Svaka igra je jedan mali zadatak. Nakon odgovora
        odmah vidiš je li točan, zašto, koliko bodova imaš, koju si razinu
        dosegao i gdje si na rang listi.
      </p>
      <section className="panel">
        <h2>Vrste zadataka</h2>
        <ul className="type-list">
          <li>Predviđanje ispisa, odabir odgovora i ispravak greške.</li>
          <li>Ima li bug, dopuni rupe, složi kod i prati izvršavanje.</li>
          <li>Robot u labirintu — program od blokova na mreži.</li>
        </ul>
      </section>
      {user ? (
        <p>Prijavljen si kao {user.username}.</p>
      ) : (
        <div className="actions">
          <Link className="button" to="/login">Prijava</Link>
          <Link className="button button-secondary" to="/register">Registracija</Link>
        </div>
      )}
    </section>
  )
}
