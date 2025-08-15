import { createContext, useContext, ReactNode } from 'react';
import { UserRole } from '../types';

const UserRoleContext = createContext<UserRole>('Author');

export function UserRoleProvider({ role, children }: { role: UserRole; children: ReactNode }) {
  return <UserRoleContext.Provider value={role}>{children}</UserRoleContext.Provider>;
}

export function useUserRole() {
  return useContext(UserRoleContext);
}
