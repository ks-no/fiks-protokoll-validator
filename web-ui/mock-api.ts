import type { Plugin, ViteDevServer, Connect } from 'vite'
import type { ServerResponse } from 'node:http'

interface TestCaseData {
  testId: string
  testName: string
  description: string
  testStep: string
  messageType: string
  operation: string
  situation: string
  expectedResult: string
  supported: boolean
  protocol: string
  payloadFileName: string
  fiksResponseTests?: {
    id: string
    payloadQuery: string
    valueType: number
    expectedValue: string
  }[]
}

interface CreateSessionRequest {
  recipientId: string
  protocol: string
  selectedTestCaseIds: string[]
}

interface StoredSession {
  id: string
  recipientId: string
  protocol: string
  selectedTestCaseIds: string[]
  createdAt: string
}

const mockTestCases: Record<string, TestCaseData[]> = {
  'no.ks.fiks.arkiv.v1': [
    {
      testId: 'HentSaksmappeN1',
      testName: 'Hent Saksmappe - Normalsituasjon 1',
      description: 'Henter en saksmappe basert på ReferanseEksternNoekkel som nøkkel',
      testStep: 'Normalsituasjon 1',
      messageType: 'no.ks.fiks.arkiv.v1.innsyn.mappe.hent',
      operation: 'HentSaksmappe',
      situation: 'N1',
      expectedResult: 'Leverer en saksmappe fra arkiv som har en referanseEksternNoekkel',
      supported: true,
      protocol: 'no.ks.fiks.arkiv.v1',
      payloadFileName: 'hent-saksmappe.xml',
      fiksResponseTests: [
        {
          id: 'test-1',
          payloadQuery: '//mappe/referanseEksternNoekkel/noekkel',
          valueType: 0,
          expectedValue: 'EksternNoekkel-1'
        }
      ]
    },
    {
      testId: 'HentJournalpostN1',
      testName: 'Hent Journalpost - Normalsituasjon 1',
      description: 'Henter en journalpost basert på journalnøkkel',
      testStep: 'Normalsituasjon 1',
      messageType: 'no.ks.fiks.arkiv.v1.innsyn.journalpost.hent',
      operation: 'HentJournalpost',
      situation: 'N1',
      expectedResult: 'Leverer en journalpost fra arkiv',
      supported: true,
      protocol: 'no.ks.fiks.arkiv.v1',
      payloadFileName: 'hent-journalpost.xml'
    },
    {
      testId: 'NyJournalpostF7',
      testName: 'Send ny Journalpost - Feilsituasjon 7',
      description: 'Tester at arkivet håndterer feil når journalpost ikke finnes',
      testStep: 'Feilsituasjon 7',
      messageType: 'no.ks.fiks.gi.arkivintegrasjon.oppdatering.basis.arkivmelding.v1',
      operation: 'NyJournalpost',
      situation: 'F7',
      expectedResult: 'FinderFault med info om at journalpost ikke finnes',
      supported: false,
      protocol: 'no.ks.fiks.arkiv.v1',
      payloadFileName: 'ny-journalpost.xml'
    }
  ],
  'no.ks.fiks.plan.v2': [
    {
      testId: 'HentPlanN1',
      testName: 'Hent Plan - Normalsituasjon 1',
      description: 'Henter en plan basert på plannøkkel',
      testStep: 'Normalsituasjon 1',
      messageType: 'no.ks.fiks.plan.v2.hent',
      operation: 'HentPlan',
      situation: 'N1',
      expectedResult: 'Leverer en plan fra planregisteret',
      supported: true,
      protocol: 'no.ks.fiks.plan.v2',
      payloadFileName: 'hent-plan.xml'
    }
  ],
  'no.ks.fiks.matrikkelfoering.v2': [
    {
      testId: 'OppdaterMatrikkelN1',
      testName: 'Oppdater Matrikkel - Normalsituasjon 1',
      description: 'Oppdaterer matrikkelinformasjon',
      testStep: 'Normalsituasjon 1',
      messageType: 'no.ks.fiks.matrikkelfoering.v2.oppdater',
      operation: 'OppdaterMatrikkel',
      situation: 'N1',
      expectedResult: 'Matrikkel oppdatert',
      supported: true,
      protocol: 'no.ks.fiks.matrikkelfoering.v2',
      payloadFileName: 'oppdater-matrikkel.xml'
    }
  ]
}

// In-memory store for created sessions
const sessions = new Map<string, StoredSession>()

function getAllTestCases(): TestCaseData[] {
  return Object.values(mockTestCases).flat()
}

function buildFiksRequest(testCase: TestCaseData, index: number) {
  const sentAt = new Date(Date.now() - (index + 1) * 60000).toISOString()

  // Vary the mock results: first = valid, second = invalid, rest = not validated
  const isValidated = index < 2
  const hasErrors = index === 1
  const validationErrors = hasErrors
    ? ['Forventet verdi "EksternNoekkel-1" men fant "feil-verdi"']
    : []

  const fiksResponses = isValidated
    ? [
        {
          id: `response-${testCase.testId}-1`,
          receivedAt: new Date(Date.now() - index * 60000 + 5000).toISOString(),
          type: testCase.messageType + '.resultat',
          fiksPayloads: [
            {
              id: `payload-${testCase.testId}-1`,
              filename: 'resultat.xml',
              payload: btoa('<?xml version="1.0"?>\n<resultat>\n  <status>OK</status>\n</resultat>')
            }
          ]
        }
      ]
    : []

  return {
    messageGuid: `msg-${testCase.testId}-${Date.now()}`,
    sentAt,
    testCase,
    fiksResponses,
    isFiksResponseValidated: isValidated,
    fiksResponseValidationErrors: validationErrors
  }
}

export function createMockApiPlugin(): Plugin {
  return {
    name: 'mock-api',
    configureServer(server: ViteDevServer) {
      server.middlewares.use((req: Connect.IncomingMessage, res: ServerResponse, next: Connect.NextFunction) => {
        const url = req.url ?? ''

        if (!url.startsWith('/api')) {
          return next()
        }

        if (url === '/api/TestCases') {
          res.setHeader('Content-Type', 'application/json')
          res.end(JSON.stringify([
            { protocol: 'no.ks.fiks.arkiv.v1', name: 'Fiks Arkiv', count: 3 },
            { protocol: 'no.ks.fiks.plan.v2', name: 'Fiks Plan', count: 1 },
            { protocol: 'no.ks.fiks.matrikkelfoering.v2', name: 'Matrikkelføring', count: 1 }
          ]))
          return
        }

        if (url.startsWith('/api/TestCases/Protocol/')) {
          const protocol = decodeURIComponent(url.split('/').pop() ?? '')
          const testCases = mockTestCases[protocol] ?? []
          res.setHeader('Content-Type', 'application/json')
          res.end(JSON.stringify(testCases))
          return
        }

        if (url === '/api/TestSessions' && req.method === 'POST') {
          let body = ''
          req.on('data', (chunk: Buffer) => { body += chunk.toString() })
          req.on('end', () => {
            try {
              const data = JSON.parse(body) as CreateSessionRequest
              const sessionId = 'mock-session-' + Date.now()

              sessions.set(sessionId, {
                id: sessionId,
                recipientId: data.recipientId,
                protocol: data.protocol,
                selectedTestCaseIds: data.selectedTestCaseIds,
                createdAt: new Date().toISOString()
              })

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
            } catch {
              res.statusCode = 400
              res.end(JSON.stringify({ error: 'Invalid request' }))
            }
          })
          return
        }

        if (url.startsWith('/api/TestCasePayloadFiles/')) {
          res.setHeader('Content-Type', 'text/plain')
          res.end('<?xml version="1.0" encoding="UTF-8"?>\n<mock>\n  <payload>Mock payload innhold for testing</payload>\n</mock>')
          return
        }

        if (url.startsWith('/api/TestSessions/')) {
          const sessionId = url.replace('/api/TestSessions/', '')
          const session = sessions.get(sessionId)

          // Look up the selected test cases, or fall back to defaults
          const allTestCases = getAllTestCases()
          const selectedIds = session?.selectedTestCaseIds
          const matchedTestCases = selectedIds
            ? allTestCases.filter(tc => selectedIds.includes(tc.testId))
            : allTestCases.slice(0, 2)

          const fiksRequests = matchedTestCases.map((tc, i) => buildFiksRequest(tc, i))

          res.setHeader('Content-Type', 'application/json')
          res.end(JSON.stringify({
            id: sessionId,
            recipientId: session?.recipientId ?? 'mock-recipient',
            protocol: session?.protocol ?? 'no.ks.fiks.arkiv.v1',
            status: 'completed',
            createdAt: session?.createdAt ?? new Date().toISOString(),
            completedAt: new Date().toISOString(),
            fiksRequests
          }))
          return
        }

        res.statusCode = 404
        res.end(JSON.stringify({ error: 'Not Found' }))
      })
    }
  }
}
