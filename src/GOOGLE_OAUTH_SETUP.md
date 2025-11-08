# Google OAuth Setup Guide

This guide explains how to set up Google OAuth authentication for RoomEnglish.

## Prerequisites

- A Google account
- Access to [Google Cloud Console](https://console.cloud.google.com)

## Step-by-Step Setup

### 1. Create a Google Cloud Project

1. Go to [Google Cloud Console](https://console.cloud.google.com)
2. Click on the project dropdown at the top of the page
3. Click "New Project"
4. Enter a project name (e.g., "RoomEnglish")
5. Click "Create"

### 2. Enable Google+ API

1. In your project, navigate to "APIs & Services" > "Library"
2. Search for "Google+ API"
3. Click on it and click "Enable"

### 3. Configure OAuth Consent Screen

1. Go to "APIs & Services" > "OAuth consent screen"
2. Select "External" user type (unless you have a Google Workspace)
3. Click "Create"
4. Fill in the required fields:
   - **App name**: RoomEnglish
   - **User support email**: Your email
   - **Developer contact information**: Your email
5. Click "Save and Continue"
6. On the "Scopes" page, click "Add or Remove Scopes"
7. Add these scopes:
   - `userinfo.email`
   - `userinfo.profile`
8. Click "Save and Continue"
9. Add test users if needed (in development mode)
10. Click "Save and Continue" and then "Back to Dashboard"

### 4. Create OAuth 2.0 Credentials

1. Go to "APIs & Services" > "Credentials"
2. Click "Create Credentials" > "OAuth client ID"
3. Select "Web application" as the application type
4. Enter a name (e.g., "RoomEnglish Web Client")
5. Add Authorized JavaScript origins:
   - `http://localhost:5173` (for Vite dev server)
   - `https://yourdomain.com` (for production)
6. Add Authorized redirect URIs:
   - `http://localhost:5173/signin-google` (for development)
   - `https://yourdomain.com/signin-google` (for production)
   - `http://localhost:5296/signin-google` (for ASP.NET backend dev)
   - `https://yourdomain.com/signin-google` (for production backend)
7. Click "Create"
8. Copy the **Client ID** and **Client Secret**

### 5. Update Configuration Files

#### appsettings.json

Update `src/Web/appsettings.json`:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_CLIENT_ID_HERE",
      "ClientSecret": "YOUR_CLIENT_SECRET_HERE"
    }
  }
}
```

#### appsettings.Development.json (Optional)

For local development, you can also update `src/Web/appsettings.Development.json`:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_DEV_CLIENT_ID",
      "ClientSecret": "YOUR_DEV_CLIENT_SECRET"
    }
  }
}
```

### 6. Security Best Practices

⚠️ **IMPORTANT**: Never commit credentials to version control!

1. Add your `appsettings.json` with real credentials to `.gitignore`
2. Use environment variables for production:
   ```bash
   export Authentication__Google__ClientId="your-client-id"
   export Authentication__Google__ClientSecret="your-client-secret"
   ```
3. Use Azure Key Vault or similar services for production secrets

## Testing

1. Start the backend server:
   ```bash
   cd src/Web
   dotnet run
   ```

2. Start the frontend:
   ```bash
   cd src/Web/ClientApp
   npm run dev
   ```

3. Navigate to http://localhost:5173/login
4. Click "Sign in with Google"
5. Complete the Google authentication flow
6. You should be redirected back and logged in

## Troubleshooting

### Redirect URI mismatch

- Make sure the redirect URI in Google Cloud Console exactly matches your application URL
- The callback path is `/signin-google` (configured in `DependencyInjection.cs`)

### 401 Unauthorized

- Check that Client ID and Client Secret are correct
- Verify that the Google+ API is enabled

### Email not provided

- Make sure you've added the `userinfo.email` scope in the OAuth consent screen
- Check that the user's Google account has an email address

### Account creation fails

- Check your database connection
- Verify that the AspNetUsers table exists
- Check application logs for detailed error messages

## How It Works

1. User clicks "Sign in with Google" on the login page
2. Frontend redirects to `/api/google-auth/login-google`
3. Backend redirects to Google's OAuth consent screen
4. User authorizes the application
5. Google redirects back to `/api/google-auth/google-callback`
6. Backend:
   - Retrieves user info from Google
   - Creates or links user account
   - Generates a Bearer token
7. Backend redirects to frontend with token in URL
8. Frontend extracts token and stores it in localStorage
9. User is redirected to the dashboard

## Database Schema

Google OAuth uses ASP.NET Core Identity's external login tables:

- `AspNetUserLogins`: Stores the link between users and external providers
  - `LoginProvider`: "Google"
  - `ProviderKey`: Google user ID
  - `UserId`: Local user ID

## API Endpoints

- `GET /api/google-auth/login-google` - Initiates OAuth flow
- `GET /api/google-auth/google-callback` - Handles OAuth callback

## References

- [Google OAuth 2.0 Documentation](https://developers.google.com/identity/protocols/oauth2)
- [ASP.NET Core External Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/)
- [Google Authentication in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins)
