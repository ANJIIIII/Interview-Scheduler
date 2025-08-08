import React, { useState, useEffect } from 'react';
// Remove PlusIcon import if it's causing issues
import Layout from '../components/Layout/layout';
import Button from '../components/ui/Button';
import InterviewForm from '../components/forms/InterviewForm';
import InterviewList from '../components/InterviewList';
import { interviewService } from '../services/interviewService';
import { useAuth } from '../hooks/useAuth';

const Dashboard = () => {
  const { user } = useAuth(); // Get user info for personalization
  const [interviews, setInterviews] = useState([]);
  const [loading, setLoading] = useState(true);
  const [formLoading, setFormLoading] = useState(false);
  const [showForm, setShowForm] = useState(false);
  const [editingInterview, setEditingInterview] = useState(null);
  const [error, setError] = useState(null);

  useEffect(() => {
    loadInterviews();
  }, []);

  const loadInterviews = async () => {
    try {
      setLoading(true);
      setError(null); // Clear previous errors
      const data = await interviewService.getInterviews();
      setInterviews(data);
    } catch (err) {
      console.error('Failed to load interviews:', err);
      setError('Failed to load interviews. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleCreateInterview = async (data) => {
    try {
      setFormLoading(true);
      setError(null);
      const newInterview = await interviewService.createInterview(data);
      setInterviews(prev => [newInterview, ...prev]);
      setShowForm(false);
      // Show success message (optional)
      console.log('Interview created successfully');
    } catch (err) {
      console.error('Failed to create interview:', err);
      setError('Failed to create interview. Please check your details and try again.');
    } finally {
      setFormLoading(false);
    }
  };

  const handleUpdateInterview = async (data) => {
    try {
      setFormLoading(true);
      setError(null);
      const updatedInterview = await interviewService.updateInterview(editingInterview.id, data);
      setInterviews(prev => prev.map(i => i.id === editingInterview.id ? updatedInterview : i));
      setEditingInterview(null);
      setShowForm(false);
      console.log('Interview updated successfully');
    } catch (err) {
      console.error('Failed to update interview:', err);
      setError('Failed to update interview. Please try again.');
    } finally {
      setFormLoading(false);
    }
  };

  const handleDeleteInterview = async (id) => {
    // Add confirmation dialog
    if (!window.confirm('Are you sure you want to delete this interview? This action cannot be undone.')) {
      return;
    }

    try {
      setError(null);
      await interviewService.deleteInterview(id);
      setInterviews(prev => prev.filter(i => i.id !== id));
      console.log('Interview deleted successfully');
    } catch (err) {
      console.error('Failed to delete interview:', err);
      setError('Failed to delete interview. Please try again.');
    }
  };

  const handleEditInterview = (interview) => {
    setEditingInterview(interview);
    setShowForm(true);
  };

  const handleCloseForm = () => {
    setShowForm(false);
    setEditingInterview(null);
    setError(null); // Clear any form-related errors
  };

  // Calculate statistics
  const now = new Date();
  const upcomingInterviews = interviews.filter(interview => 
    new Date(interview.scheduledAt) > now
  ).length;
  const completedInterviews = interviews.filter(interview => 
    new Date(interview.scheduledAt) < now
  ).length;

  return (
    <Layout>
      <div className="space-y-6">
        {/* Header Section */}
        <div className="flex justify-between items-center">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Interview Dashboard</h1>
            <p className="mt-1 text-sm text-gray-500">
              Welcome back, {user?.name || user?.email}! Manage and schedule your interviews
            </p>
          </div>
            {/* <button >
           Click Me
          </button> */}
          {/* <Button 
            onClick={() => setShowForm(true)}
            disabled={loading}
            className="inline-flex items-center"
          >
           
            Schedule Interview
          </Button> */}
            <button
        onClick={() => setShowForm(true)}
        className="px-4 py-2 rounded-md font-semibold text-white bg-blue-600 hover:bg-blue-700 inline-flex items-center"
      >
        Schedule Interview
      </button>
        </div>

        {/* Statistics Cards */}
        <div className="grid grid-cols-1 gap-5 sm:grid-cols-3">
          <div className="bg-white overflow-hidden shadow rounded-lg">
            <div className="p-5">
              <div className="flex items-center">
                <div className="flex-shrink-0">
                  <div className="w-8 h-8 bg-blue-500 rounded-full flex items-center justify-center">
                    <span className="text-white text-sm font-bold">
                      {interviews.length}
                    </span>
                  </div>
                </div>
                <div className="ml-5 w-0 flex-1">
                  <dl>
                    <dt className="text-sm font-medium text-gray-500 truncate">
                      Total Interviews
                    </dt>
                    <dd className="text-lg font-medium text-gray-900">
                      {interviews.length}
                    </dd>
                  </dl>
                </div>
              </div>
            </div>
          </div>

          <div className="bg-white overflow-hidden shadow rounded-lg">
            <div className="p-5">
              <div className="flex items-center">
                <div className="flex-shrink-0">
                  <div className="w-8 h-8 bg-green-500 rounded-full flex items-center justify-center">
                    <span className="text-white text-sm font-bold">
                      {upcomingInterviews}
                    </span>
                  </div>
                </div>
                <div className="ml-5 w-0 flex-1">
                  <dl>
                    <dt className="text-sm font-medium text-gray-500 truncate">
                      Upcoming
                    </dt>
                    <dd className="text-lg font-medium text-gray-900">
                      {upcomingInterviews}
                    </dd>
                  </dl>
                </div>
              </div>
            </div>
          </div>

          <div className="bg-white overflow-hidden shadow rounded-lg">
            <div className="p-5">
              <div className="flex items-center">
                <div className="flex-shrink-0">
                  <div className="w-8 h-8 bg-gray-500 rounded-full flex items-center justify-center">
                    <span className="text-white text-sm font-bold">
                      {completedInterviews}
                    </span>
                  </div>
                </div>
                <div className="ml-5 w-0 flex-1">
                  <dl>
                    <dt className="text-sm font-medium text-gray-500 truncate">
                      Completed
                    </dt>
                    <dd className="text-lg font-medium text-gray-900">
                      {completedInterviews}
                    </dd>
                  </dl>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Error Alert */}
        {error && (
          <div className="bg-red-50 border border-red-200 rounded-md p-4">
            <div className="flex">
              <div className="ml-3 flex-1">
                <h3 className="text-sm font-medium text-red-800">Error</h3>
                <div className="mt-2 text-sm text-red-700">
                  <p>{error}</p>
                </div>
                <div className="mt-4">
                  <button
                    onClick={() => setError(null)}
                    className="bg-red-50 text-red-800 rounded-md px-2 py-1 hover:bg-red-100 text-sm border border-red-200"
                  >
                    Dismiss
                  </button>
                  {error.includes('load') && (
                    <button
                      onClick={loadInterviews}
                      className="ml-2 bg-red-100 text-red-800 rounded-md px-2 py-1 hover:bg-red-200 text-sm border border-red-300"
                    >
                      Retry
                    </button>
                  )}
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Success Message (you can add this state if needed) */}
        {/* You might want to add a success state for better UX */}

        {/* Interview List */}
        <div className="bg-white shadow rounded-lg">
          <div className="px-4 py-5 sm:p-6">
            <h3 className="text-lg leading-6 font-medium text-gray-900 mb-4">
              Your Interviews
            </h3>
            <InterviewList
              interviews={interviews}
              onEdit={handleEditInterview}
              onDelete={handleDeleteInterview}
              loading={loading}
            />
          </div>
        </div>

        {/* Interview Form Modal */}
        <InterviewForm
          isOpen={showForm}
          onClose={handleCloseForm}
          onSubmit={editingInterview ? handleUpdateInterview : handleCreateInterview}
          initialData={editingInterview}
          loading={formLoading}
        />
      </div>
    </Layout>
  );
};

export default Dashboard;