export const SAMPLE_TYPES = ['Sangre', 'Orina', 'Heces', 'Hisopado'] as const;
export type SampleType = typeof SAMPLE_TYPES[number];

export type SampleStatus = 'Registrada' | 'Recibida' | 'Procesada' | 'Rechazada';

export interface SampleListItem {
  id: number;
  documentNumber: string;
  patientName: string;
  sampleType: SampleType;
  collectedAt: string;
  status: SampleStatus;
  testCount: number;
}

export interface TestItem {
  code: string;
  name: string;
}

export interface SampleDetail {
  id: number;
  documentNumber: string;
  patientName: string;
  sampleType: SampleType;
  collectedAt: string;
  status: SampleStatus;
  rejectionReason: string | null;
  createdAt: string;
  tests: TestItem[];
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateSampleRequest {
  documentNumber: string;
  fullName: string;
  sampleType: string;
  collectedAt: string;
  testCodes: string[];
}

export interface ErrorResponse {
  status: number;
  message: string;
  errors?: Record<string, string[]> | null;
}

export interface updateSampleStatusRequest {
  status: SampleStatus;
  rejectionReason?: string | null;
}