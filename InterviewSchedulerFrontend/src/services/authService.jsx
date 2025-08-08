import api from './api';

export const authService = {
  googleAuth: async (code) => {
    const response = await api.post('/auth/google', { code });
    return response.data;
  },

  getCurrentUser: async () => {
    const response = await api.get('/auth/me');
    return response.data;
  },

  logout: () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  }
};