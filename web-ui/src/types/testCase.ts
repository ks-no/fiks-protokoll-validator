export type ValidationState = 'valid' | 'invalid' | 'notValidated'

export interface TestCase {
  testId: string
  testName: string
  messageType: string
  description: string
  testStep: string
  operation: string
  situation: string
  expectedResult: string
  payloadFileName?: string
  payloadAttachmentFileNames?: string
  supported: boolean
  protocol: string
  fiksResponseTests?: FiksResponseTest[]
}

export interface FiksResponseTest {
  id: string
  payloadQuery: string
  valueType: number
  expectedValue: string
}

export interface FiksPayload {
  id: string
  filename: string
  payload?: string
}

export interface FiksResponse {
  id: string
  receivedAt: string
  type: string
  fiksPayloads: FiksPayload[]
  payloadContent?: string
}

export interface FiksRequest {
  messageGuid: string
  sentAt: string
  testCase: TestCase
  fiksResponses: FiksResponse[]
  customPayloadFile?: {
    filename: string
    content?: string
  }
  isFiksResponseValidated: boolean
  fiksResponseValidationErrors: string[]
}

export interface TestSession {
  id: string
  recipientId: string
  protocol: string
  status: string
  createdAt: string
  completedAt?: string
  fiksRequests: FiksRequest[]
}

export interface ProtocolOption {
  value: string
  text: string
}
