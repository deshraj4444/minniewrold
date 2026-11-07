import { createContext, useCallback, useContext, useEffect, useState } from 'react';
import { api } from '../lib/api.js';

const StudentContext = createContext({ student: undefined, setStudent: () => {}, logout: async () => {} });

/** Who is signed in as a student. `student` is undefined while loading and null when signed out. */
export function StudentProvider({ children }) {
  const [student, setStudent] = useState(undefined);

  useEffect(() => {
    api.students.me().then(setStudent).catch(() => setStudent(null));
  }, []);

  const logout = useCallback(async () => {
    await api.students.logout();
    setStudent(null);
  }, []);

  return <StudentContext.Provider value={{ student, setStudent, logout }}>{children}</StudentContext.Provider>;
}

export const useStudent = () => useContext(StudentContext);
