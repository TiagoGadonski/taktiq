import { test, expect } from '@playwright/test';

test.describe('Authentication', () => {
  test('should display login page', async ({ page }) => {
    await page.goto('/login');
    await expect(page.getByRole('heading', { name: /gymhero/i })).toBeVisible();
    await expect(page.getByPlaceholder(/email/i)).toBeVisible();
    await expect(page.getByPlaceholder(/senha/i)).toBeVisible();
  });

  test('should show validation errors for invalid login', async ({ page }) => {
    await page.goto('/login');

    await page.getByRole('button', { name: /entrar/i }).click();

    await expect(page.getByText(/email inválido/i)).toBeVisible();
  });

  test('should navigate to signup page', async ({ page }) => {
    await page.goto('/login');

    await page.getByRole('link', { name: /criar conta/i }).click();

    await expect(page.url()).toContain('/signup');
    await expect(page.getByRole('heading', { name: /criar conta/i })).toBeVisible();
  });

  test('should show validation errors for invalid signup', async ({ page }) => {
    await page.goto('/signup');

    // Enter invalid data
    await page.getByPlaceholder(/nome/i).fill('J');
    await page.getByPlaceholder(/email/i).fill('invalid-email');
    await page.getByPlaceholder(/senha/i).fill('weak');

    await page.getByRole('button', { name: /criar conta/i }).click();

    // Should show validation errors
    await expect(page.getByText(/nome deve ter/i)).toBeVisible();
  });
});

test.describe('Protected Routes', () => {
  test('should redirect to login when accessing protected route without auth', async ({ page }) => {
    await page.goto('/dashboard');

    await expect(page.url()).toContain('/login');
  });
});
