import { UserRole } from '../../models/enums/user-role.enum';

export interface User {
  id: string;
  email: string;
  displayName: string;
  role: UserRole;
  organizationId: string;
  organizationName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken?: string;
  expiresIn: number;
  user: User;
}

export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
}
