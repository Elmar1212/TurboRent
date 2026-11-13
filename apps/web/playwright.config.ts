import { defineConfig } from '@playwright/test'
import { fileURLToPath } from 'node:url'

if (!process.env.TURBORENT_TEST_DATABASE?.includes('Database=turborent_identity_test_')) {
  throw new Error('Use scripts/Test-IdentityE2E.ps1 with an isolated database.')
}
const certificate = fileURLToPath(new URL('../../artifacts/dev-certs/turborent.pem', import.meta.url))
const key = fileURLToPath(new URL('../../artifacts/dev-certs/turborent.key', import.meta.url))
const webPort = Number(process.env.TURBORENT_E2E_WEB_PORT ?? '5175')
if (!Number.isInteger(webPort) || webPort < 1024 || webPort > 65535) throw new Error('Invalid isolated E2E port.')
const webOrigin = 'https://localhost:' + webPort
const configuration = process.env.TURBORENT_E2E_CONFIGURATION === 'Debug' ? 'Debug' : 'Release'
export default defineConfig({
  testDir: '../../tests/e2e',
  workers: 1,
  use: { baseURL: webOrigin, ignoreHTTPSErrors: true, trace: 'off' },
  webServer: [
    {
      command: 'dotnet run -c ' + configuration + ' --no-build --no-launch-profile --project ../../src/backend/TurboRent.Api --urls https://localhost:7081',
      url: 'https://localhost:7081/api/v1/health/ready',
      ignoreHTTPSErrors: true, reuseExistingServer: false,
      env: {
        ASPNETCORE_ENVIRONMENT: 'Testing',
        ConnectionStrings__Database: process.env.TURBORENT_TEST_DATABASE!,
        ASPNETCORE_Kestrel__Certificates__Default__Path: certificate,
        ASPNETCORE_Kestrel__Certificates__Default__KeyPath: key,
        Identity__FrontendOrigin: webOrigin,
        Identity__MailboxPath: process.env.TURBORENT_E2E_MAILBOX!,
        Identity__IpPermitLimit: '1000',
      },
    },
    {
      command: 'npm run dev -- --port ' + webPort,
      url: webOrigin, ignoreHTTPSErrors: true, reuseExistingServer: false,
      env: { TURBORENT_HTTPS: 'true', TURBORENT_API_URL: 'https://localhost:7081', NODE_EXTRA_CA_CERTS: certificate },
    },
  ],
})
