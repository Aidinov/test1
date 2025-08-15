import axios from 'axios';

const http = axios.create({
  baseURL: '/api'
});

function ensureStringEnums(obj: unknown): void {
  if (obj && typeof obj === 'object') {
    for (const [key, value] of Object.entries(obj)) {
      if (typeof value === 'number') {
        throw new Error(`Numeric enum value received for ${key}`);
      }
      ensureStringEnums(value);
    }
  }
}

http.interceptors.response.use((response) => {
  ensureStringEnums(response.data);
  return response;
});

export default http;
