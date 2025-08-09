import React, { useState, useEffect } from 'react';
import Button from '../ui/Button';
import Input from '../ui/Input';
import Modal from '../ui/modal';

const InterviewForm = ({ isOpen, onClose, onSubmit, initialData = null, loading = false }) => {
  const [formData, setFormData] = useState({
    jobTitle: '',
    candidateName: '',
    candidateEmail: '',
    interviewerName: '',
    interviewerEmail: '',
    startTime: '',
    endTime: ''
  });
  
  const [errors, setErrors] = useState({});

  useEffect(() => {
    if (initialData) {
      setFormData({
        jobTitle: initialData.jobTitle || '',
        candidateName: initialData.candidateName || '',
        candidateEmail: initialData.candidateEmail || '',
        interviewerName: initialData.interviewerName || '',
        interviewerEmail: initialData.interviewerEmail || '',
        startTime: initialData.startTime ? formatDateTimeLocal(initialData.startTime) : '',
        endTime: initialData.endTime ? formatDateTimeLocal(initialData.endTime) : ''
      });
    } else {
      setFormData({
        jobTitle: '',
        candidateName: '',
        candidateEmail: '',
        interviewerName: '',
        interviewerEmail: '',
        startTime: '',
        endTime: ''
      });
    }
  }, [initialData, isOpen]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
    
    // Clear error when user starts typing
    if (errors[name]) {
      setErrors(prev => ({
        ...prev,
        [name]: ''
      }));
    }
  };

  const validateForm = () => {
    const newErrors = {};
    
    if (!formData.jobTitle.trim()) {
      newErrors.jobTitle = 'Job title is required';
    }
    
    if (!formData.candidateName.trim()) {
      newErrors.candidateName = 'Candidate name is required';
    }
    
    if (!formData.candidateEmail.trim()) {
      newErrors.candidateEmail = 'Candidate email is required';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.candidateEmail)) {
      newErrors.candidateEmail = 'Please enter a valid email address';
    }
    
    if (!formData.interviewerName.trim()) {
      newErrors.interviewerName = 'Interviewer name is required';
    }
    
    if (!formData.interviewerEmail.trim()) {
      newErrors.interviewerEmail = 'Interviewer email is required';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.interviewerEmail)) {
      newErrors.interviewerEmail = 'Please enter a valid email address';
    }
    
    if (!formData.startTime) {
      newErrors.startTime = 'Start time is required';
    }
    
    if (!formData.endTime) {
      newErrors.endTime = 'End time is required';
    }
    
    if (formData.startTime && formData.endTime) {
      const startDate = new Date(formData.startTime);
      const endDate = new Date(formData.endTime);
      
      if (startDate >= endDate) {
        newErrors.endTime = 'End time must be after start time';
      }
      
      if (startDate < new Date()) {
        newErrors.startTime = 'Start time cannot be in the past';
      }
    }
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }
    
    try {
      // Convert local datetime to ISO string for API
      const submitData = {
        ...formData,
        startTime: new Date(formData.startTime).toISOString(),
        endTime: new Date(formData.endTime).toISOString()
      };
      
      await onSubmit(submitData);
      
      // Reset form on success
      if (!initialData) {
        setFormData({
          jobTitle: '',
          candidateName: '',
          candidateEmail: '',
          interviewerName: '',
          interviewerEmail: '',
          startTime: '',
          endTime: ''
        });
      }
      
      onClose();
    } catch (err) {
      console.error('Failed to submit interview:', err);
    }
  };

  const formatDateTimeLocal = (dateString) => {
    const date = new Date(dateString);
    const offset = date.getTimezoneOffset() * 60000;
    const localISOTime = new Date(date.getTime() - offset).toISOString().slice(0, 16);
    return localISOTime;
  };

  const getMinDateTime = () => {
    const now = new Date();
    const offset = now.getTimezoneOffset() * 60000;
    const localISOTime = new Date(now.getTime() - offset).toISOString().slice(0, 16);
    return localISOTime;
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={initialData ? 'Edit Interview' : 'Schedule Interview'} size="lg">
      <div className="space-y-4">
        <div className="grid grid-cols-1 gap-4">
          <Input
            label="Job Title"
            type="text"
            name="jobTitle"
            value={formData.jobTitle}
            onChange={handleChange}
            error={errors.jobTitle}
            placeholder="e.g. Senior Software Engineer"
            required
          />
          
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <Input
              label="Candidate's Name"
              type="text"
              name="candidateName"
              value={formData.candidateName}
              onChange={handleChange}
              error={errors.candidateName}
              placeholder="Enter candidate's full name"
              required
            />
            
            <Input
              label="Candidate's Email"
              type="email"
              name="candidateEmail"
              value={formData.candidateEmail}
              onChange={handleChange}
              error={errors.candidateEmail}
              placeholder="candidate@example.com"
              required
            />
          </div>
          
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <Input
              label="Interviewer's Name"
              type="text"
              name="interviewerName"
              value={formData.interviewerName}
              onChange={handleChange}
              error={errors.interviewerName}
              placeholder="Enter interviewer's full name"
              required
            />
            
            <Input
              label="Interviewer's Email"
              type="email"
              name="interviewerEmail"
              value={formData.interviewerEmail}
              onChange={handleChange}
              error={errors.interviewerEmail}
              placeholder="interviewer@company.com"
              required
            />
          </div>
          
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <Input
              label="Start Time"
              type="datetime-local"
              name="startTime"
              value={formData.startTime}
              onChange={handleChange}
              error={errors.startTime}
              min={getMinDateTime()}
              required
            />
            
            <Input
              label="End Time"
              type="datetime-local"
              name="endTime"
              value={formData.endTime}
              onChange={handleChange}
              error={errors.endTime}
              min={formData.startTime || getMinDateTime()}
              required
            />
          </div>
        </div>
        
        <div className="flex justify-end space-x-3 pt-4">
          <Button
            type="button"
            variant="secondary"
            onClick={onClose}
            disabled={loading}
          >
            Cancel
          </Button>
         <button
  type="button"
  onClick={handleSubmit}
  disabled={loading}
  className={`px-5 py-2.5 rounded-md font-semibold text-white transition-colors duration-300
    ${loading ? 'bg-gray-400 cursor-not-allowed' : 'bg-blue-600 hover:bg-blue-700'}
  `}
>
  {loading ? 'Saving...' : (initialData ? 'Update Interview' : 'Schedule Interview')}
</button>
        </div>
      </div>
    </Modal>
  );
};

export default InterviewForm;