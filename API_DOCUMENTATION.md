# IgnaCheck API Documentation

## Overview

The IgnaCheck API is a RESTful API that provides compliance framework management capabilities. This document describes how to use the API, including versioning and authentication.

## API Versioning

The API uses URL-based versioning with support for multiple version selection methods:

### Version Format

All API endpoints follow this pattern:
```
https://api.example.com/api/v{version}/{controller}/{action}
```

For example:
```
https://api.example.com/api/v1/Authentication/login
https://api.example.com/api/v1/Workspace
https://api.example.com/api/v1/Profile
```

### Version Selection Methods

The API supports three ways to specify the API version:

1. **URL Segment (Recommended)**: Include the version in the URL
   ```
   GET /api/v1/Profile
   ```

2. **Header**: Use the `X-Api-Version` header
   ```
   GET /api/Profile
   X-Api-Version: 1.0
   ```

3. **Media Type**: Include version in the `Accept` header
   ```
   GET /api/Profile
   Accept: application/json;version=1.0
   ```

### Current Version

- **Current Version**: v1.0
- **Default Version**: v1.0 (used when no version is specified)

## Interactive API Documentation (Swagger)

The API provides interactive documentation via Swagger UI, which allows you to:
- Browse all available endpoints
- View request/response schemas
- Test API endpoints directly from your browser
- See authentication requirements

### Accessing Swagger UI

Navigate to `/api` in your browser when running the application:
```
http://localhost:5000/api
```

The Swagger specification JSON is available at:
```
http://localhost:5000/api/specification.json
```

## Authentication

Most endpoints require authentication using JWT (JSON Web Token).

### Obtaining a Token

1. Register a new account:
   ```
   POST /api/v1/Authentication/register
   {
     "email": "user@example.com",
     "password": "SecurePassword123!",
     "firstName": "John",
     "lastName": "Doe"
   }
   ```

2. Verify your email (check your email for verification link)

3. Login to get a JWT token:
   ```
   POST /api/v1/Authentication/login
   {
     "email": "user@example.com",
     "password": "SecurePassword123!"
   }
   ```

   Response:
   ```json
   {
     "succeeded": true,
     "data": {
       "accessToken": "eyJhbGciOiJIUzI1NiIs...",
       "userId": "user-id-here",
       "email": "user@example.com"
     }
   }
   ```

### Using the Token

Include the token in the `Authorization` header for all authenticated requests:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### Testing with Swagger UI

1. Navigate to `/api`
2. Click the "Authorize" button at the top
3. Enter: `Bearer {your-token-here}`
4. Click "Authorize"
5. Now you can test authenticated endpoints

## API Response Format

All API responses follow a consistent format:

### Success Response
```json
{
  "succeeded": true,
  "data": {
    // Response data here
  }
}
```

### Error Response
```json
{
  "succeeded": false,
  "errors": [
    "Error message 1",
    "Error message 2"
  ]
}
```

### Validation Error Response
```json
{
  "succeeded": false,
  "errors": {
    "fieldName": ["Validation error for this field"],
    "anotherField": ["Another validation error"]
  }
}
```

## Available Endpoints

### Authentication (`/api/v1/Authentication`)
- `POST /register` - Register a new user
- `POST /login` - Login and get JWT token
- `POST /verify-email` - Verify email address
- `POST /request-password-reset` - Request password reset
- `POST /reset-password` - Reset password with token

### Workspace Management (`/api/v1/Workspace`)
- `GET /` - Get all workspaces for current user
- `POST /` - Create a new workspace
- `POST /switch` - Switch to a different workspace
- `GET /settings` - Get workspace settings
- `PUT /settings` - Update workspace settings

### User Management (`/api/v1/Users`)
- `GET /workspace/members` - Get workspace members
- `POST /invite` - Invite a user to workspace
- `GET /workspace/invitations` - Get workspace invitations
- `GET /my-invitations` - Get current user's invitations
- `POST /invitations/accept` - Accept an invitation
- `POST /invitations/decline` - Decline an invitation
- `POST /invitations/revoke` - Revoke an invitation
- `PUT /workspace/members/role` - Update member role
- `DELETE /workspace/members` - Remove a member

### Profile Management (`/api/v1/Profile`)
- `GET /` - Get current user's profile
- `PUT /` - Update current user's profile
- `PUT /avatar` - Update user avatar
- `GET /notification-preferences` - Get notification preferences
- `PUT /notification-preferences` - Update notification preferences

## Rate Limiting

Currently, there are no rate limits enforced. This may change in future versions.

## HTTP Status Codes

The API uses standard HTTP status codes:

- `200 OK` - Request succeeded
- `400 Bad Request` - Invalid request data
- `401 Unauthorized` - Authentication required or failed
- `403 Forbidden` - Authenticated but not authorized
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

## Support and Feedback

For issues, questions, or feedback, please contact the development team or create an issue in the repository.
