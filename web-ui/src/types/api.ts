import type { TestCase, TestSession } from './testCase'

export interface ApiError {
  status: number
  message: string
  title?: string
}

export interface CreateTestSessionRequest {
  recipientId: string
  selectedTestCaseIds: string[]
  protocol: string
}

export interface CreateTestSessionResponse {
  id: string
  recipientId: string
  protocol: string
  status: string
  testCaseIds: string[]
  createdAt: string
}

export interface ProtocolSummary {
  protocol: string
  name: string
  count: number
}

export type GetTestCasesResponse = TestCase[]

export type GetTestSessionResponse = TestSession

export type GetProtocolsResponse = ProtocolSummary[]
