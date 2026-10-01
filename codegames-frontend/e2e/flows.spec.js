import { expect, test } from '@playwright/test'

test('registracija prijavi novog korisnika', async ({ page }) => {
  const stamp = Date.now().toString(36)
  const username = `e2e${stamp}`

  await page.goto('/register')
  await page.getByLabel('E-mail').fill(`${username}@example.com`)
  await page.getByLabel('Korisničko ime').fill(username)
  await page.getByLabel('Lozinka').fill('lozinka123')
  await page.getByRole('button', { name: 'Registriraj se' }).click()

  await expect(page.getByText(`Prijavljen si kao ${username}`)).toBeVisible()
})

test('gost riješi demo zadatak', async ({ page }) => {
  await page.goto('/demo')
  await page.getByRole('button', { name: 'Ima bug' }).click()

  await expect(page.getByText(/Točno\.|Nije točno\./)).toBeVisible()
  await expect(page.getByRole('link', {
    name: 'Registriraj se da sačuvaš bodove i uđeš na rang listu',
  })).toBeVisible()
})

test('admin doda i obriše zadatak', async ({ page }) => {
  const password = process.env.PLAYWRIGHT_ADMIN_PASSWORD
  test.skip(!password, 'Postavi PLAYWRIGHT_ADMIN_PASSWORD na lozinku iz Seed:AdminPassword.')

  await page.goto('/login')
  await page.getByLabel('E-mail').fill('admin@codegames.local')
  await page.getByLabel('Lozinka').fill(password)
  await page.getByRole('button', { name: 'Prijavi se' }).click()
  await expect(page.getByRole('link', { name: 'Admin' })).toBeVisible()
  await page.goto('/admin')
  await page.getByRole('tab', { name: 'Zadaci' }).click()
  await page.getByRole('button', { name: 'Novi zadatak' }).click()

  const title = `E2E ${Date.now()}`
  await page.getByLabel('Naslov').fill(title)
  await page.getByLabel('Objašnjenje').fill('Objašnjenje za E2E.')
  await page.getByLabel('Isječak koda').fill('print(1)')
  await page.getByLabel('Pitanje').fill('Što program ispisuje?')
  await page.getByLabel('Točan ispis').fill('1')
  await page.getByRole('button', { name: 'Spremi' }).click()
  await expect(page.getByText('Zadatak je dodan.')).toBeVisible()

  let row = page.locator('.admin-row', { hasText: title })
  for (let step = 0; step < 12 && (await row.count()) === 0; step += 1) {
    const next = page.getByRole('button', { name: 'Sljedeća' })
    await expect(next).toBeEnabled()
    await next.click()
    row = page.locator('.admin-row', { hasText: title })
  }

  await row.getByRole('button', { name: 'Obriši' }).click()
  await page.getByRole('dialog').getByRole('button', { name: 'Obriši' }).click()
  await expect(page.getByText(title)).toHaveCount(0)
})
