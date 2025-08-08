import React, { useEffect, useRef } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import { authService } from '../../services/authService';
import { GOOGLE_CLIENT_ID } from '../../utils/constant';
import Button from '../ui/Button';

const GoogleAuth = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { login } = useAuth();
  const [loading, setLoading] = React.useState(false);
  const [error, setError] = React.useState(null);
  const hasProcessedCode = useRef(false); // Prevent double execution

  useEffect(() => {
    const code = searchParams.get('code');
    if (code && !hasProcessedCode.current) {
      hasProcessedCode.current = true; // Mark as processed
      handleGoogleCallback(code);
    }
  }, [searchParams]);

  const handleGoogleCallback = async (code) => {
    setLoading(true);
    setError(null);
    
    try {
      console.log('Processing authorization code:', code.substring(0, 20) + '...');
      const response = await authService.googleAuth(code);
      login(response.user, response.token);
      navigate('/dashboard');
    } catch (err) {
      console.error('Authentication error:', err);
      setError('Authentication failed. Please try again.');
      // Reset the flag on error so user can retry
      hasProcessedCode.current = false;
    } finally {
      setLoading(false);
    }
  };

  const handleGoogleLogin = () => {
    console.log('Google Client ID:', GOOGLE_CLIENT_ID);
    const googleAuthUrl = `https://accounts.google.com/o/oauth2/v2/auth?` +
      `client_id=${GOOGLE_CLIENT_ID}&` +
      `redirect_uri=${encodeURIComponent('http://localhost:5173/auth/callback')}&` +
      `response_type=code&` +
      `scope=${encodeURIComponent('openid email profile https://www.googleapis.com/auth/calendar')}&` +
      `access_type=offline&` +
      `prompt=consent`;
    
    window.location.href = googleAuthUrl;
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Authenticating...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full space-y-8">
        <div>
          <h2 className="mt-6 text-center text-3xl font-extrabold text-blue-500">
            Interview Scheduler
          </h2>
          <p className="mt-2 text-center text-sm text-gray-600">
            Sign in to schedule and manage interviews
          </p>
        </div>
        
        <div className="mt-8 space-y-6">
          {error && (
            <div className="bg-red-50 border border-red-200 rounded-md p-4">
              <p className="text-sm text-red-600">{error}</p>
              <button 
                onClick={() => {
                  setError(null);
                  hasProcessedCode.current = false;
                }}
                className="mt-2 text-sm text-red-700 underline"
              >
                Try again
              </button>
            </div>
          )}
          
          <div>
            <Button
              onClick={handleGoogleLogin}
              className="group relative w-full flex justify-center py-3 px-4 border border-transparent text-sm font-medium rounded-md text-black bg-blue-500 hover:bg-primary-700"
            >
              <svg className="w-5 h-5 mr-2" viewBox="0 0 24 24">
                <path fill="currentColor" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/>
                <path fill="currentColor" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/>
                <path fill="currentColor" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"/>
                <path fill="currentColor" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/>
              </svg>
              Sign in with Google
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default GoogleAuth;