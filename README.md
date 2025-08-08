# Interview Scheduler

A full-stack web application for scheduling and managing interviews with Google Calendar integration and email notifications.

## ✨ Features

- **Google OAuth Authentication** - Secure login with Google accounts
- **Interview Scheduling** - Create, edit, and manage interviews
- **Google Calendar Integration** - Automatic calendar event creation
- **Email Notifications** - Automated interview reminders
- **Responsive UI** - Modern React interface
- **Real-time Updates** - Live interview status updates

## 🛠️ Tech Stack

### Backend
- **ASP.NET Core 8.0** - REST API
- **Entity Framework Core** - Database ORM
- **PostgreSQL** - Database
- **Google Calendar API** - Calendar integration
- **JWT Authentication** - Secure auth tokens

### Frontend
- **React 18** - User interface
- **Vite** - Build tool and dev server
- **Tailwind CSS** - Styling
- **Axios** - HTTP client
- **React Router** - Navigation
- **Google OAuth** - Authentication

## 📋 Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- [Google Cloud Console Account](https://console.cloud.google.com/)

## 🚀 Quick Start

### 1. Clone Repository
```bash
git clone https://github.com/yourusername/interview-scheduler.git
cd interview-scheduler
```

### 2. Google Cloud Setup
1. Create project in [Google Cloud Console](https://console.cloud.google.com/)
2. Enable APIs:
   - Google Calendar API
   - Gmail API
   - Google+ API
3. Create OAuth 2.0 credentials
4. Create Service Account for Calendar API
5. Download service account JSON key

### 3. Backend Setup
```bash
cd backend/InterviewScheduler.API

# Install dependencies
dotnet restore

# Update appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=InterviewScheduler;Username=postgres;Password=yourpassword"
  },
  "GoogleAuth": {
    "ClientId": "your-google-client-id",
    "ClientSecret": "your-google-client-secret"
  },
  "GoogleCalendar": {
    "ServiceAccountKeyPath": "path-to-service-account.json"
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}

# Run migrations
dotnet ef database update

# Start API
dotnet run
```

### 4. Frontend Setup
```bash
cd frontend

# Install dependencies
npm install

# Create .env.local
VITE_API_BASE_URL=http://localhost:5000/api
VITE_GOOGLE_CLIENT_ID=your-google-client-id

# Start development server
npm run dev
```

## 📁 Project Structure

```
interview-scheduler/
├── backend/InterviewScheduler.API/
│   ├── Controllers/          # API endpoints
│   ├── Models/              # Data models
│   ├── Services/            # Business logic
│   ├── Data/               # Database context
│   ├── DTOs/               # Data transfer objects
│   └── Program.cs          # Application entry point
├── frontend/src/
│   ├── components/         # React components
│   ├── services/          # API services
│   ├── hooks/             # Custom React hooks
│   ├── pages/             # Page components
│   └── utils/             # Utility functions
└── README.md
```

## 🔧 Configuration

### Environment Variables

#### Backend (.NET)
```json
{
  "ConnectionStrings__DefaultConnection": "database-connection-string",
  "GoogleAuth__ClientId": "google-oauth-client-id",
  "GoogleAuth__ClientSecret": "google-oauth-client-secret",
  "GoogleCalendar__ServiceAccountKey": "base64-encoded-service-account-json",
  "Email__Username": "smtp-username",
  "Email__Password": "smtp-password"
}
```

#### Frontend (Vite)
```env
VITE_API_BASE_URL=http://localhost:5000/api
VITE_GOOGLE_CLIENT_ID=your-google-client-id
```

### Google OAuth Setup
1. Add redirect URIs in Google Cloud Console:
   - `http://localhost:5173` (Vite development)
   - Production domain (when deployed)

## 🚀 Deployment

### Backend - Railway (Free)
1. Push code to GitHub
2. Connect Railway to repository
3. Add environment variables
4. Deploy automatically

### Frontend - Vercel (Free)
1. Push code to GitHub
2. Connect Vercel to repository
3. Add environment variables
4. Deploy automatically

### Detailed Deployment Guide
See [Deployment Guide](docs/DEPLOYMENT.md) for complete instructions.

## 📊 API Endpoints

### Authentication
- `POST /api/auth/login` - Google OAuth login
- `POST /api/auth/logout` - User logout
- `GET /api/auth/me` - Get current user

### Interviews
- `GET /api/interviews` - List all interviews
- `POST /api/interviews` - Create interview
- `GET /api/interviews/{id}` - Get interview details
- `PUT /api/interviews/{id}` - Update interview
- `DELETE /api/interviews/{id}` - Delete interview

## 🧪 Testing

### Backend Tests
```bash
cd backend/InterviewScheduler.Tests
dotnet test
```

### Frontend Tests
```bash
cd frontend
npm run test
```

## 🤝 Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open Pull Request


**Interview Scheduler - Streamlining Interview Management**
**Made with ❤️ by Anjali**
