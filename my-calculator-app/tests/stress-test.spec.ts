const { test } = require('@playwright/test');

for (let i = 0; i < 20; i++) {
  test(`click stress and result buttons - Run ${i + 1}`, async ({ page }) => {
    // Navigate to localhost:4200
    await page.goto('http://localhost:4200/');

    // Click the button with id 'stress'
    await page.click('#stress');

    // Click the button with id 'result'
    await page.click('#result');

    // Wait for the div with id 'input' to contain any text, with a maximum timeout of 90 seconds
    await page.waitForSelector('#input:not(:empty)', { timeout: 150000 });
  });
}
