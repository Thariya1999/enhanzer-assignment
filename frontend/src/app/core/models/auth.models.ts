export interface LoginRequest {
  email: string;
  password: string;
}

export interface Location {
  locationCode: string;
  locationName: string;
}

export interface AuthSession {
  token: string;
  expiresAt: string;
  email: string;
  locations: Location[];
}
