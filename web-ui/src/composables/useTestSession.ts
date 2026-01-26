import { ref, type Ref } from 'vue'
import { useApi } from './useApi'
import type { TestSession, TestCase, FiksRequest } from '@/types'

interface CreateTestSessionParams {
  recipientId: string
  selectedTestCaseIds: string[]
  protocol: string
}

interface CreateTestSessionResponse {
  id: string
  recipientId: string
  protocol: string
  status: string
  testCaseIds: string[]
  createdAt: string
}

interface UseTestSessionReturn {
  testSession: Ref<TestSession | null>
  loading: Ref<boolean>
  error: Ref<{ status: number; message: string; data?: unknown } | null>
  fetchTestSession: (sessionId: string) => Promise<TestSession>
  createTestSession: (params: CreateTestSessionParams) => Promise<CreateTestSessionResponse>
  sortRequests: (requests: FiksRequest[]) => FiksRequest[]
}

export function useTestSession(): UseTestSessionReturn {
  const api = useApi<TestSession>()
  const testSession = ref<TestSession | null>(null)

  async function fetchTestSession(sessionId: string): Promise<TestSession> {
    const data = await api.get(`/api/TestSessions/${sessionId}`)
    testSession.value = {
      ...data,
      fiksRequests: sortRequests(data.fiksRequests)
    }
    return testSession.value
  }

  async function createTestSession(params: CreateTestSessionParams): Promise<CreateTestSessionResponse> {
    const createApi = useApi<CreateTestSessionResponse>()
    return createApi.post('/api/TestSessions', params)
  }

  function sortRequests(requests: FiksRequest[]): FiksRequest[] {
    if (!requests) return []
    return [...requests].sort((a, b) =>
      new Date(a.sentAt).getTime() - new Date(b.sentAt).getTime()
    )
  }

  return {
    testSession,
    loading: api.loading,
    error: api.error,
    fetchTestSession,
    createTestSession,
    sortRequests
  }
}

interface UseTestCasesReturn {
  testCases: Ref<TestCase[]>
  loading: Ref<boolean>
  error: Ref<{ status: number; message: string; data?: unknown } | null>
  fetchTestCases: () => Promise<TestCase[]>
  fetchTestCasesByProtocol: (protocol: string) => Promise<TestCase[]>
  filterSupported: (cases: TestCase[]) => TestCase[]
}

export function useTestCases(): UseTestCasesReturn {
  const api = useApi<TestCase[]>()
  const testCases = ref<TestCase[]>([])

  async function fetchTestCases(): Promise<TestCase[]> {
    testCases.value = await api.get('/api/TestCases')
    return testCases.value
  }

  async function fetchTestCasesByProtocol(protocol: string): Promise<TestCase[]> {
    testCases.value = await api.get(`/api/TestCases/Protocol/${protocol}`)
    return testCases.value
  }

  function filterSupported(cases: TestCase[]): TestCase[] {
    return cases.filter(tc => tc.supported)
  }

  return {
    testCases,
    loading: api.loading,
    error: api.error,
    fetchTestCases,
    fetchTestCasesByProtocol,
    filterSupported
  }
}
