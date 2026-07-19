import { defineConfig } from '@playwright/test';

const demoUrl = process.env.DEMO_URL ?? 'http://localhost:5072';

export default defineConfig({
  testDir: '.',
  timeout: 30_000,
  expect: { timeout: 10_000 },
  retries: process.env.CI ? 1 : 0,
  use: {
    baseURL: demoUrl,
    actionTimeout: 5_000,
    navigationTimeout: 10_000,
    trace: 'on-first-retry',
    permissions: ['clipboard-read', 'clipboard-write'],
  },
  webServer: process.env.SKIP_WEB_SERVER
    ? undefined
    : {
        command:
          'dotnet build ../../src/TheNerdCollective.Demo/TheNerdCollective.Demo.csproj && dotnet run --project ../../src/TheNerdCollective.Demo/TheNerdCollective.Demo.csproj --launch-profile http --no-build',
        url: demoUrl,
        reuseExistingServer: !process.env.CI,
        timeout: 90_000,
      },
});
