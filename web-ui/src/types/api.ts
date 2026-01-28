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
