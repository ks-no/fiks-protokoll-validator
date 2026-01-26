// Mock API data for development
// This file is only used in development mode and not included in production builds

export const mockTestCases = {
  'no.ks.fiks.arkiv.v1': [
    {
      id: 'HentSaksmappeN1',
      testName: 'Hent Saksmappe - Normalsituasjon 1',
      description: 'Henter en saksmappe basert på ReferanseEksternNoekkel som nøkkel',
      testStep: 'Normalsituasjon 1',
      messageType: 'no.ks.fiks.arkiv.v1.innsyn.mappe.hent',
      operation: 'HentSaksmappe',
      situation: 'N1',
      expectedResult: 'Leverer en saksmappe fra arkiv som har en referanseEksternNoekkel',
      supported: true,
      protocol: 'no.ks.fiks.arkiv.v1'
    },
    {
      id: 'HentJournalpostN1',
      testName: 'Hent Journalpost - Normalsituasjon 1',
      description: 'Henter en journalpost basert på journalnøkkel',
      testStep: 'Normalsituasjon 1',
      messageType: 'no.ks.fiks.arkiv.v1.innsyn.journalpost.hent',
      operation: 'HentJournalpost',
      situation: 'N1',
      expectedResult: 'Leverer en journalpost fra arkiv',
      supported: true,
      protocol: 'no.ks.fiks.arkiv.v1'
    },
    {
      id: 'NyJournalpostF7',
      testName: 'Send ny Journalpost - Feilsituasjon 7',
      description: 'Tester at arkivet håndterer feil når journalpost ikke finnes',
      testStep: 'Feilsituasjon 7',
      messageType: 'no.ks.fiks.gi.arkivintegrasjon.oppdatering.basis.arkivmelding.v1',
      operation: 'NyJournalpost',
      situation: 'F7',
      expectedResult: 'FinderFault med info om at journalpost ikke finnes',
      supported: false,
      protocol: 'no.ks.fiks.arkiv.v1'
    }
  ],
  'no.ks.fiks.plan.v2': [
    {
      id: 'HentPlanN1',
      testName: 'Hent Plan - Normalsituasjon 1',
      description: 'Henter en plan basert på plannøkkel',
      testStep: 'Normalsituasjon 1',
      messageType: 'no.ks.fiks.plan.v2.hent',
      operation: 'HentPlan',
      situation: 'N1',
      expectedResult: 'Leverer en plan fra planregisteret',
      supported: true,
      protocol: 'no.ks.fiks.plan.v2'
    }
  ],
  'no.ks.fiks.matrikkelfoering.v2': [
    {
      id: 'OppdaterMatrikkelN1',
      testName: 'Oppdater Matrikkel - Normalsituasjon 1',
      description: 'Oppdaterer matrikkelinformasjon',
      testStep: 'Normalsituasjon 1',
      messageType: 'no.ks.fiks.matrikkelfoering.v2.oppdater',
      operation: 'OppdaterMatrikkel',
      situation: 'N1',
      expectedResult: 'Matrikkel oppdatert',
      supported: true,
      protocol: 'no.ks.fiks.matrikkelfoering.v2'
    }
  ]
};

export function createMockApiPlugin() {
  return {
    name: 'mock-api',
    configureServer(server) {
      server.middlewares.use((req, res, next) => {
        if (!req.url.startsWith('/api')) {
          return next()
        }

        console.log('[Mock API]', req.method, req.url)

        // Mock TestCases list (all protocols)
        if (req.url === '/api/TestCases') {
          res.setHeader('Content-Type', 'application/json')
          res.end(JSON.stringify([
            { protocol: 'no.ks.fiks.arkiv.v1', name: 'Fiks Arkiv', count: 3 },
            { protocol: 'no.ks.fiks.plan.v2', name: 'Fiks Plan', count: 1 },
            { protocol: 'no.ks.fiks.matrikkelfoering.v2', name: 'Matrikkelføring', count: 1 }
          ]))
          return
        }

        // Mock TestCases by Protocol
        if (req.url.startsWith('/api/TestCases/Protocol/')) {
          const protocol = decodeURIComponent(req.url.split('/').pop())
          const testCases = mockTestCases[protocol] || []
          res.setHeader('Content-Type', 'application/json')
          res.end(JSON.stringify(testCases))
          return
        }

        // Mock TestSessions POST - create new test session
        if (req.url === '/api/TestSessions' && req.method === 'POST') {
          let body = ''
          req.on('data', chunk => { body += chunk })
          req.on('end', () => {
            try {
              const data = JSON.parse(body)
              const sessionId = 'mock-session-' + Date.now()
              
              res.statusCode = 201
              res.setHeader('Content-Type', 'application/json')
              res.end(JSON.stringify({
                id: sessionId,
                recipientId: data.recipientId,
                protocol: data.protocol,
                status: 'created',
                testCaseIds: data.selectedTestCaseIds,
                createdAt: new Date().toISOString()
              }))
            } catch (e) {
              res.statusCode = 400
              res.end(JSON.stringify({ error: 'Invalid request' }))
            }
          })
          return
        }

        // Mock TestSession GET - get test session results
        if (req.url.startsWith('/api/TestSessions/')) {
          const sessionId = req.url.split('/').pop()
          res.setHeader('Content-Type', 'application/json')
          res.end(JSON.stringify({
            id: sessionId,
            status: 'completed',
            protocol: 'no.ks.fiks.arkiv.v1',
            recipientId: 'mock-recipient',
            testResults: [
              {
                testCaseId: 'HentSaksmappeN1',
                testName: 'Hent Saksmappe - Normalsituasjon 1',
                status: 'passed',
                message: 'Test completed successfully',
                timestamp: new Date().toISOString()
              }
            ],
            createdAt: new Date().toISOString(),
            completedAt: new Date().toISOString()
          }))
          return
        }

        // Default 404
        res.statusCode = 404
        res.end(JSON.stringify({ error: 'Not Found' }))
      })
    }
  }
}
