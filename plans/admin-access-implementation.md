# Admin Access & Invite System Implementation

## Overview

This implementation adds a comprehensive admin access control system with invite-based user registration. The system includes:

1. **User Roles**: Admin and User roles with different permissions
2. **Invite System**: Admins can create invite links for new users
3. **Profile Completion**: New users must complete their profile on first login
4. **User Management**: Admins can block/unblock users and change roles

## Environment Configuration

### Backend Environment Variables

The admin user is created automatically on startup using environment variables. Configure these before starting the application:

```bash
# Admin credentials (required for production)
ADMIN_EMAIL=admin@yourdomain.com
ADMIN_PASSWORD=YourSecurePassword123!

# Immich API key (if using Immich provider)
IMMICH_API_KEY=your-immich-api-key
```

You can set these in:
- Environment variables
- `VideoManager/VManBackend/appsettings.json` (for development only):
  ```json
  {
    "Admin": {
      "Email": "admin@vman.local",
      "Password": "AdminPass123!"
    }
  }
  ```

### Frontend Environment Variables

Configure the frontend API URL in `.env.local`:

```bash
NEXT_PUBLIC_API_URL=http://localhost:5001
```

## Features Implemented

### Backend

1. **User Model Updates**
   - Added `Role` enum (User, Admin)
   - Added `IsBlocked` flag
   - Added `IsProfileComplete` flag
   - Made FirstName and LastName optional (completed on first login)

2. **UserInvite Model**
   - Unique token per invite
   - Email-specific invites
   - Expiration (7 days)
   - Single-use tokens
   - Creator tracking

3. **Authentication Endpoints**
   - `POST /api/auth/accept-invite` - Accept an invite and create account
   - `POST /api/auth/complete-profile` - Complete profile on first login
   - `POST /api/auth/login` - Updated to return profile completion status
   - `POST /api/auth/register` - Disabled (returns 403)

4. **Admin Endpoints** (all require Admin role)
   - `POST /api/admin/invites` - Create a new invite
   - `GET /api/admin/invites` - List all invites
   - `GET /api/admin/users` - List all users
   - `POST /api/admin/users/{id}/block` - Block a user
   - `POST /api/admin/users/{id}/unblock` - Unblock a user
   - `PUT /api/admin/users/{id}/role` - Change user role

5. **Authorization**
   - Added "AdminOnly" authorization policy
   - Role claims in JWT tokens
   - Blocked user check on login

### Frontend

1. **Auth Flow Updates**
   - `/accept-invite?token=xxx` - Accept invite page
   - `/complete-profile` - Profile completion page
   - Updated login to handle profile completion redirect
   - Removed public registration link

2. **Admin Dashboard**
   - `/admin/users` - User management page
     - View all users
     - Block/unblock users
     - Change user roles
   - `/admin/invites` - Invite management page
     - Create new invites
     - View invite history
     - Copy invite links

3. **Navigation**
   - Admin menu items visible only to admin users
   - Role-based navigation display

## Usage Flow

### First Time Setup

1. **Start the application** with ADMIN_EMAIL and ADMIN_PASSWORD environment variables
2. **Admin user is created** automatically with incomplete profile
3. **Admin logs in** and completes their profile
4. **Admin can now manage** users and invites

### Inviting New Users

1. **Admin creates invite** via `/admin/invites`
   - Enter user's email
   - Invite link is generated and copied to clipboard
2. **Admin shares link** with the new user (via email, chat, etc.)
3. **User clicks link** and sets their password
4. **User completes profile** by entering their name
5. **User can now access** the application

### User Management

Admins can:
- **Block users**: Prevents login
- **Unblock users**: Re-enables login
- **Change roles**: Promote users to Admin or demote to User
- **View user activity**: See creation date and last login

## Security Considerations

1. **Invite Tokens**
   - 256-bit random tokens (URL-safe base64)
   - Single-use only
   - 7-day expiration
   - Email-specific (can't be used for different email)

2. **Admin Protection**
   - Admins cannot block themselves
   - Admins cannot change their own role
   - At least one admin should always exist

3. **Password Requirements**
   - Minimum 8 characters
   - Maximum 100 characters
   - BCrypt hashing with work factor 4 (development) or 10+ (production)

4. **Authorization**
   - JWT role claims
   - Server-side role verification on all admin endpoints
   - Blocked users cannot login

## Database Migration

The migration `AddUserRolesAndInvites` includes:
- User table updates (Role, IsBlocked, IsProfileComplete)
- New UserInvites table
- Indexes for performance

Apply the migration:
```bash
cd VideoManager/VManBackend
dotnet ef database update
```

## Testing

### Manual Testing Steps

1. **Test Admin Creation**
   ```bash
   # Set environment variables
   export ADMIN_EMAIL="admin@test.local"
   export ADMIN_PASSWORD="TestAdmin123!"
   
   # Start application - admin should be created
   # Check logs for "✅ Admin user created"
   ```

2. **Test Invite Flow**
   - Login as admin
   - Navigate to Admin > Invites
   - Create invite for test@example.com
   - Copy invite link
   - Open link in incognito/private window
   - Set password
   - Complete profile
   - Verify login works

3. **Test User Management**
   - Login as admin
   - Navigate to Admin > Users
   - Block a user
   - Try to login as blocked user (should fail)
   - Unblock user
   - Login should work again

4. **Test Role Changes**
   - Login as admin
   - Promote a user to Admin
   - Login as that user
   - Verify admin menu is visible
   - Demote user back to User
   - Admin menu should disappear

## Known Limitations

1. **Email Sending**: System does not send emails automatically. Admins must manually share invite links.
2. **Password Reset**: Not implemented in this version.
3. **2FA**: Not implemented in this version.
4. **Audit Logging**: User actions are not logged (only basic created/login timestamps).

## Future Enhancements

- Email integration for automatic invite sending
- Password reset functionality
- Two-factor authentication
- Audit log for user actions
- Bulk invite creation
- Custom invite expiration periods
- User profile editing
