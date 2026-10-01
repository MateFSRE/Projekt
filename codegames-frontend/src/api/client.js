import axios from 'axios'

// Jedini axios u aplikaciji. Komponente zovu ovaj modul, ne axios izravno.
const api = axios.create({
  baseURL: '/api',
  withCredentials: true,
})

api.interceptors.response.use(
  (response) => response,
  (error) => {
    const data = error.response?.data
    const message =
      (typeof data === 'object' && data?.message) ||
      'Zahtjev nije uspio. Pokušaj ponovno.'

    const apiError = new Error(message)
    apiError.status = error.response?.status
    return Promise.reject(apiError)
  },
)

export default api
