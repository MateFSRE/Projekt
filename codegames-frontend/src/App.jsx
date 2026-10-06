import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { AuthProvider } from './auth/AuthContext.jsx'
import AdminRoute from './components/AdminRoute.jsx'
import Layout from './components/Layout.jsx'
import ProtectedRoute from './components/ProtectedRoute.jsx'
import AdminPage from './pages/AdminPage.jsx'
import DailyPage from './pages/DailyPage.jsx'
import DemoPage from './pages/DemoPage.jsx'
import GamesPage from './pages/GamesPage.jsx'
import PlayPage from './pages/PlayPage.jsx'
import HomePage from './pages/HomePage.jsx'
import LeaderboardPage from './pages/LeaderboardPage.jsx'
import LoginPage from './pages/LoginPage.jsx'
import ProfilePage from './pages/ProfilePage.jsx'
import RegisterPage from './pages/RegisterPage.jsx'

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route element={<Layout />}>
            <Route path="/" element={<HomePage />} />
            <Route path="/demo" element={<DemoPage />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route path="/games" element={<ProtectedRoute><GamesPage /></ProtectedRoute>} />
            <Route path="/games/play" element={<ProtectedRoute><PlayPage /></ProtectedRoute>} />
            <Route path="/daily" element={<ProtectedRoute><DailyPage /></ProtectedRoute>} />
            <Route path="/leaderboard" element={<LeaderboardPage />} />
            <Route path="/profile" element={<ProtectedRoute><ProfilePage /></ProtectedRoute>} />
            <Route path="/admin" element={<AdminRoute><AdminPage /></AdminRoute>} />
          </Route>
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}
