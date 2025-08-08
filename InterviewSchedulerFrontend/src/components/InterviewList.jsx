import React, { useState } from 'react';
import { format } from 'date-fns';
import { PencilIcon, TrashIcon, VideoCameraIcon } from '@heroicons/react/24/outline';
import Button from './ui/Button';

const InterviewList = ({ interviews, onEdit, onDelete, loading = false }) => {
  const [deletingId, setDeletingId] = useState(null);

  const handleDelete = async (id) => {
    setDeletingId(id);
    try {
      await onDelete(id);
    } finally {
      setDeletingId(null);
    }
  };

  const formatDateTime = (dateString) => {
    try {
      return format(new Date(dateString), 'MMM dd, yyyy - hh:mm a');
    } catch (error) {
      return 'Invalid date';
    }
  };

  if (loading) {
    return (
      <div className="bg-white shadow rounded-lg p-6">
        <div className="animate-pulse space-y-4">
          {[...Array(3)].map((_, i) => (
            <div key={i} className="border-b border-gray-200 pb-4">
              <div className="h-4 bg-gray-200 rounded w-1/4 mb-2"></div>
              <div className="h-3 bg-gray-200 rounded w-1/2 mb-2"></div>
              <div className="h-3 bg-gray-200 rounded w-1/3"></div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (!interviews || interviews.length === 0) {
    return (
      <div className="bg-white shadow rounded-lg p-6 text-center">
        <VideoCameraIcon className="mx-auto h-12 w-12 text-gray-400" />
        <h3 className="mt-2 text-sm font-medium text-gray-900">No interviews scheduled</h3>
        <p className="mt-1 text-sm text-gray-500">Get started by scheduling your first interview.</p>
      </div>
    );
  }

  return (
    <div className="bg-white shadow rounded-lg overflow-hidden">
      <div className="px-4 py-5 sm:p-6">
        <h3 className="text-lg leading-6 font-medium text-gray-900 mb-4">
          Scheduled Interviews
        </h3>
        <div className="space-y-4">
          {interviews.map((interview) => (
            <div key={interview.id} className="border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow">
              <div className="flex justify-between items-start">
                <div className="flex-1">
                  <h4 className="text-lg font-semibold text-gray-900 mb-2">
                    {interview.jobTitle}
                  </h4>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm text-gray-600">
                    <div>
                      <p><span className="font-medium">Candidate:</span> {interview.candidateName}</p>
                      <p><span className="font-medium">Email:</span> {interview.candidateEmail}</p>
                    </div>
                    <div>
                      <p><span className="font-medium">Interviewer:</span> {interview.interviewerName}</p>
                      <p><span className="font-medium">Email:</span> {interview.interviewerEmail}</p>
                    </div>
                  </div>
                  <div className="mt-3">
                    <p className="text-sm text-gray-600">
                      <span className="font-medium">Time:</span> {formatDateTime(interview.startTime)}
                    </p>
                    {interview.googleMeetLink && (
                      <a
                        href={interview.googleMeetLink}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="inline-flex items-center mt-2 text-sm text-primary-600 hover:text-primary-700"
                      >
                        <VideoCameraIcon className="h-4 w-4 mr-1" />
                        Join Google Meet
                      </a>
                    )}
                  </div>
                </div>
                <div className="flex space-x-2 ml-4">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => onEdit(interview)}
                  >
                    <PencilIcon className="h-4 w-4" />
                  </Button>
                  <Button
                    variant="danger"
                    size="sm"
                    onClick={() => handleDelete(interview.id)}
                    loading={deletingId === interview.id}
                  >
                    <TrashIcon className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

export default InterviewList;