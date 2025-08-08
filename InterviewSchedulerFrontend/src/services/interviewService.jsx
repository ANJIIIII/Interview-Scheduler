import api from './api';

export const interviewService = {
  getInterviews: async () => {
    const response = await api.get('/interview');
    return response.data;
  },

  getInterview: async (id) => {
    const response = await api.get(`/interview/${id}`);
    return response.data;
  },

  createInterview: async (data) => {
    const response = await api.post('/interview', data);
    return response.data;
  },

  updateInterview: async (id, data) => {
    const response = await api.put(`/interview/${id}`, data);
    return response.data;
  },

  deleteInterview: async (id) => {
    await api.delete(`/interview/${id}`);
  }
};