import { defineConfig, devices } from '@playwright/test'

export default defineConfig({
  testDir: './e2e',
  timeout: 60_000,
  expect: { timeout: 15_000 },
  use: {
    baseURL: 'http://localhost:5173',
    ...devices['Desktop Chrome'],
  },
  webServer: [
    {
      command: 'dotnet run --project ../CodeGames/CodeGames.csproj --launch-profile http',
      url: 'http://localhost:5080/swagger/index.html',
      reuseExistingServer: true,
      timeout: 120_000,
    },
    {
      command: 'npm run dev',
      url: 'http://localhost:5173',
      reuseExistingServer: true,
      timeout: 60_000,
    },
  ],
})
