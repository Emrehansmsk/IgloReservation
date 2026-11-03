export type ReservationStatus = 'Pending' | 'Confirmed' | 'Cancelled' | 'Completed'

export interface UserInfo {
  id: string
  email: string | null
  userName: string | null
  roles: string[]
}

export interface AuthResponse {
  accessToken: string
  expiresAtUtc: string
  user: UserInfo
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
}

export interface IgloUnit {
  id: string
  name: string
  isActive: boolean
  createdAtUtc: string
}

export interface Reservation {
  id: string
  userId: string
  igloUnitId: string
  startAtUtc: string
  endAtUtc: string
  status: ReservationStatus
  description: string | null
  createdAtUtc: string
}

export interface CreateReservationPayload {
  userId: string
  igloUnitId: string
  startAtUtc: string
  endAtUtc: string
  status: ReservationStatus
  description?: string | null
}

export interface UpdateReservationPayload {
  igloUnitId: string
  startAtUtc: string
  endAtUtc: string
  status: ReservationStatus
  description?: string | null
}

export class ApiError extends Error {
  status: number
  body: unknown

  constructor(status: number, message: string, body?: unknown) {
    super(message)
    this.status = status
    this.body = body
  }
}
