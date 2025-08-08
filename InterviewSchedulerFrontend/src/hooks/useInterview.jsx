import { useState } from 'react';
import { interviewService } from '../services/interviewService';

export const useInterview = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(false);

  const scheduleInterview = async (interviewData) => {
    try {
      setLoading(true);
      setError(null);
      setSuccess(false);
      
      const result = await interviewService.createInterview(interviewData);
      setSuccess(true);
      
      // Reset success after 3 seconds
      setTimeout(() => {
        setSuccess(false);
      }, 3000);
      
      return result;
    } catch (err) {
      setError(err.message || 'Failed to schedule interview');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const updateInterview = async (id, interviewData) => {
    try {
      setLoading(true);
      setError(null);
      
      const result = await interviewService.updateInterview(id, interviewData);
      return result;
    } catch (err) {
      setError(err.message || 'Failed to update interview');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const deleteInterview = async (id) => {
    try {
      setLoading(true);
      setError(null);
      
      await interviewService.deleteInterview(id);
      return true;
    } catch (err) {
      setError(err.message || 'Failed to delete interview');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const getInterviews = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const result = await interviewService.getInterviews();
      return result;
    } catch (err) {
      setError(err.message || 'Failed to fetch interviews');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const getInterview = async (id) => {
    try {
      setLoading(true);
      setError(null);
      
      const result = await interviewService.getInterview(id);
      return result;
    } catch (err) {
      setError(err.message || 'Failed to fetch interview');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const clearError = () => {
    setError(null);
  };

  const clearSuccess = () => {
    setSuccess(false);
  };

  return {
    loading,
    error,
    success,
    scheduleInterview,
    updateInterview,
    deleteInterview,
    getInterviews,
    getInterview,
    clearError,
    clearSuccess
  };
};