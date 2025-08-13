import { create } from 'zustand';

interface Thread {
  id: string;
  text: string;
  status: string;
}

interface State {
  threads: Thread[];
  addThread: (t: Thread) => void;
}

export const useStore = create<State>((set) => ({
  threads: [],
  addThread: (t) => set((s) => ({ threads: [...s.threads, t] }))
}));
