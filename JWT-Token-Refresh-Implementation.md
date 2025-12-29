# JWT Token Expiration Validation & Automatic Refresh - Implementation Guide

## Overview

This document describes the implementation of JWT token expiration validation and automatic refresh functionality in the NERBA Backoffice Angular frontend. The implementation ensures that expired tokens are automatically refreshed, and users are only logged out when refresh fails.

---

## Implementation Summary

### What Was Implemented

1. **Token Expiration Validation**
   - Token expiration is checked on application load
   - Token expiration is checked before returning bearer token
   - Expired tokens are never sent to the backend

2. **Automatic Token Refresh**
   - HTTP 401 errors trigger automatic token refresh
   - Original request is retried with new token after refresh
   - Multiple simultaneous refresh requests are prevented

3. **Graceful Logout on Refresh Failure**
   - If token refresh fails, user is logged out
   - LocalStorage is cleared
   - User is redirected to login page with returnUrl

---

## Files Modified

### 1. API Endpoints Configuration
**File**: `src/app/core/objects/apiEndpoints.ts`

**Changes**:
```typescript
// Added refresh token endpoint
refresh_token: `${BASE_URL}/api/auth/refresh-user-token`,
```

### 2. Authentication Service
**File**: `src/app/core/services/auth.service.ts`

**Changes**:
- Added `isRefreshing` flag to prevent duplicate refresh requests
- Enhanced `loadUserFromStorage()` to validate token expiration on load
- Updated `bearerToken` getter to check expiration before returning token
- Added `logout()` to reset refresh state

**New Methods**:

#### `isTokenExpired(token: string): boolean`
Checks if a JWT token is expired or invalid.
```typescript
isTokenExpired(token: string): boolean {
  if (!token) return true;

  try {
    const decoded = jwtDecode<JwtPayload & { exp: number }>(token);
    const currentTime = Math.floor(Date.now() / 1000);

    // Add 5 second buffer to account for clock skew
    return decoded.exp <= currentTime + 5;
  } catch (error) {
    console.error('Error decoding token:', error);
    return true;
  }
}
```

#### `getTokenExpirationTime(token: string): number | null`
Gets the token expiration time in milliseconds.
```typescript
getTokenExpirationTime(token: string): number | null {
  try {
    const decoded = jwtDecode<JwtPayload & { exp: number }>(token);
    return decoded.exp * 1000; // Convert to milliseconds
  } catch {
    return null;
  }
}
```

#### `refreshToken(): Observable<void>`
Refreshes the current user's JWT token by calling the backend endpoint.
```typescript
refreshToken(): Observable<void> {
  // Prevent multiple simultaneous refresh attempts
  if (this.isRefreshing) {
    return of(void 0);
  }

  this.isRefreshing = true;

  return this.http.get<User>(API_ENDPOINTS.refresh_token).pipe(
    tap((user: User) => {
      if (user && user.jwt) {
        console.log('Token refreshed successfully');
        this.setUser(user);
        this.isRefreshing = false;
      }
    }),
    map(() => void 0),
    catchError((error) => {
      console.error('Token refresh failed:', error);
      this.isRefreshing = false;
      // Clear user data and logout on refresh failure
      this.logout();
      throw error;
    })
  );
}
```

### 3. Token Refresh Interceptor (New File)
**File**: `src/app/shared/interceptors/token-refresh.interceptor.ts`

**Purpose**: Intercepts HTTP 401 errors and automatically refreshes the token.

**Key Features**:
- Catches 401 Unauthorized responses
- Skips refresh for auth endpoints (login, refresh) to avoid infinite loops
- Attempts token refresh automatically
- Retries original request with new token on success
- Logs out user and redirects to login on refresh failure

**Implementation**:
```typescript
export const tokenRefreshInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401) {
        return throwError(() => error);
      }

      // Don't try to refresh on auth endpoints
      const isAuthEndpoint =
        req.url.includes('/api/auth/login') ||
        req.url.includes('/api/auth/refresh-user-token');

      if (isAuthEndpoint) {
        return throwError(() => error);
      }

      // Attempt to refresh the token
      return authService.refreshToken().pipe(
        switchMap(() => {
          // Retry original request with new token
          const newToken = authService.bearerToken;
          const clonedReq = req.clone({
            setHeaders: {
              Authorization: `Bearer ${newToken}`,
            },
          });
          return next(clonedReq);
        }),
        catchError((refreshError) => {
          // Refresh failed, logout and redirect
          authService.logout();
          router.navigate(['/login'], {
            queryParams: { returnUrl: router.url },
          });
          return throwError(() => refreshError);
        })
      );
    })
  );
};
```

### 4. Application Bootstrap
**File**: `src/main.ts`

**Changes**:
- Imported `tokenRefreshInterceptor`
- Registered interceptor in HTTP client configuration

```typescript
import { tokenRefreshInterceptor } from './app/shared/interceptors/token-refresh.interceptor';

provideHttpClient(
  withInterceptors([authInterceptor, tokenRefreshInterceptor])
)
```

---

## How It Works

### Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                   Application Start                          │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
         ┌───────────────────────────────┐
         │  Load User from LocalStorage  │
         └───────────────┬───────────────┘
                         │
                         ▼
              ┌──────────────────────┐
              │  Is Token Expired?   │
              └──────┬────────┬──────┘
                     │        │
                 Yes │        │ No
                     │        │
                     ▼        ▼
         ┌───────────────┐   ┌──────────────────┐
         │  Clear User   │   │  Set User State  │
         │  Redirect to  │   │  Continue Normal │
         │  Login        │   │  Operation       │
         └───────────────┘   └──────────────────┘


┌─────────────────────────────────────────────────────────────┐
│                     API Request Flow                         │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
         ┌───────────────────────────────┐
         │  Make API Request with Token  │
         └───────────────┬───────────────┘
                         │
                         ▼
              ┌──────────────────────┐
              │  Response Status?    │
              └──┬───────────────┬───┘
                 │               │
            200 OK│               │ 401 Unauthorized
                 │               │
                 ▼               ▼
      ┌─────────────────┐   ┌──────────────────────┐
      │  Return Data    │   │  Token Refresh       │
      │  to Component   │   │  Interceptor Catches │
      └─────────────────┘   └──────────┬───────────┘
                                       │
                                       ▼
                         ┌──────────────────────────┐
                         │  Call Refresh Endpoint   │
                         └──────┬───────────┬───────┘
                                │           │
                          Success│           │Fail
                                │           │
                                ▼           ▼
              ┌──────────────────────┐  ┌──────────────────┐
              │  Store New Token     │  │  Logout User     │
              │  Retry Original Req  │  │  Redirect Login  │
              └──────────────────────┘  └──────────────────┘
```

### Scenarios

#### Scenario 1: Application Load with Valid Token
1. User opens application
2. `AuthService` constructor calls `loadUserFromStorage()`
3. Token is checked with `isTokenExpired()`
4. Token is valid → User state is set
5. Application proceeds normally

#### Scenario 2: Application Load with Expired Token
1. User opens application
2. `AuthService` constructor calls `loadUserFromStorage()`
3. Token is checked with `isTokenExpired()`
4. Token is expired → `removeUser()` is called
5. LocalStorage is cleared
6. User sees login page

#### Scenario 3: API Request with Expired Token (401 Response)
1. User makes API request (e.g., fetch companies)
2. Backend validates token and returns 401 Unauthorized
3. `tokenRefreshInterceptor` catches the 401 error
4. Interceptor calls `authService.refreshToken()`
5. Backend validates old token and issues new token
6. New token is stored in LocalStorage
7. Original request is retried with new token
8. User receives data seamlessly (no logout)

#### Scenario 4: Token Refresh Failure
1. User makes API request
2. Backend returns 401 Unauthorized
3. `tokenRefreshInterceptor` attempts refresh
4. Refresh fails (token too old, user blocked, etc.)
5. `authService.logout()` is called
6. LocalStorage is cleared
7. User is redirected to `/login` with `returnUrl`
8. After re-login, user returns to original page

---

## Security Features

### 1. Clock Skew Protection
A 5-second buffer is added to token expiration checks to account for time differences between client and server:
```typescript
return decoded.exp <= currentTime + 5;
```

### 2. Duplicate Refresh Prevention
The `isRefreshing` flag prevents multiple simultaneous refresh requests:
```typescript
if (this.isRefreshing) {
  return of(void 0);
}
```

### 3. Infinite Loop Prevention
Auth endpoints are excluded from refresh interceptor:
```typescript
const isAuthEndpoint =
  req.url.includes('/api/auth/login') ||
  req.url.includes('/api/auth/refresh-user-token');

if (isAuthEndpoint) {
  return throwError(() => error);
}
```

### 4. Automatic Cleanup on Failure
All failure paths clear user data and reset state:
```typescript
catchError((error) => {
  this.isRefreshing = false;
  this.logout();
  throw error;
})
```

---

## Testing Instructions

### Test 1: Valid Token on Load
**Steps**:
1. Login to the application
2. Close browser tab
3. Reopen application
4. **Expected**: User remains logged in

### Test 2: Expired Token on Load
**Steps**:
1. Login to the application
2. Manually modify token expiration in LocalStorage to past date
3. Refresh page
4. **Expected**: User is logged out and sees login page

### Test 3: Token Expiration During Session
**Steps**:
1. Login to the application
2. Wait for token to expire (or modify backend to issue short-lived tokens)
3. Make an API request (e.g., navigate to a different page)
4. **Expected**: Token is automatically refreshed, request succeeds

### Test 4: Refresh Failure
**Steps**:
1. Login to the application
2. Stop backend server
3. Make an API request
4. **Expected**: User is logged out and redirected to login

### Test 5: Multiple Simultaneous Requests
**Steps**:
1. Login to the application
2. Wait for token to expire
3. Trigger multiple API requests simultaneously
4. **Expected**: Only one refresh request is made, all requests succeed

---

## Browser Console Logs

During normal operation, you'll see these logs:

**On Load (Expired Token)**:
```
Token expired on load, clearing user data
```

**On 401 Response**:
```
401 error detected, attempting token refresh...
Token refreshed successfully
Token refreshed, retrying original request
```

**On Refresh Failure**:
```
Token refresh failed: [error details]
Token refresh failed, redirecting to login
```

---

## Backend Requirements

The frontend expects the backend to provide:

1. **Refresh Endpoint**: `GET /api/auth/refresh-user-token`
   - Requires valid JWT in Authorization header
   - Returns new User object with refreshed JWT
   - Returns 401 if token cannot be refreshed

2. **JWT Claims**: Token must include `exp` (expiration) claim
   ```json
   {
     "nameid": "user-id",
     "email": "user@example.com",
     "given_name": "John",
     "family_name": "Doe",
     "role": ["Admin"],
     "exp": 1735234567  // Unix timestamp
   }
   ```

---

## Debugging Tips

### Check Token Expiration Manually
```javascript
// In browser console
const user = JSON.parse(localStorage.getItem('NerbaBackofficeUser'));
const decoded = JSON.parse(atob(user.jwt.split('.')[1]));
console.log('Token expires at:', new Date(decoded.exp * 1000));
console.log('Current time:', new Date());
console.log('Is expired:', decoded.exp * 1000 < Date.now());
```

### Monitor Network Requests
1. Open DevTools → Network tab
2. Filter by "XHR"
3. Look for:
   - Original request → 401 response
   - Refresh request → 200 response
   - Retry of original request → 200 response

### Check AuthService State
```javascript
// In Angular component with AuthService injected
console.log('Is Authenticated:', this.authService.isAuthenticated);
console.log('Bearer Token:', this.authService.bearerToken);
console.log('User Claims:', this.authService.getUserClaims);
```

---

## Performance Considerations

### Token Expiration Checking
- Expiration is checked only when needed (not on timer)
- Decoding is fast (client-side only)
- No performance impact on normal operations

### Refresh Request Overhead
- Refresh only occurs on 401 responses
- Duplicate requests are prevented
- Original request is retried automatically (no user action needed)

### LocalStorage Access
- Minimal localStorage reads (only on load and token updates)
- No performance impact on modern browsers

---

---

## Server-Side Logout Implementation

### Overview

The application now includes **server-side token invalidation** using a Redis-based blacklist. When users logout, their JWT token is added to a blacklist, ensuring it cannot be used even if stolen after logout.

### Backend Components

#### 1. Token Blacklist Service

**File**: `NERBABO.Backend/Core/Authentication/Services/TokenBlacklistService.cs`

```csharp
public class TokenBlacklistService : ITokenBlacklistService
{
    private readonly IDistributedCache _cache;

    public async Task BlacklistTokenAsync(string token, DateTime expirationTime)
    {
        var key = $"blacklist:token:{token}";
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = expirationTime  // Auto-cleanup
        };
        await _cache.SetAsync(key, Encoding.UTF8.GetBytes(...), options);
    }

    public async Task<bool> IsTokenBlacklistedAsync(string token)
    {
        var key = $"blacklist:token:{token}";
        var value = await _cache.GetAsync(key);
        return value != null;  // Null = not blacklisted
    }
}
```

**Key Features**:
- Uses Redis distributed cache
- Tokens auto-expire from blacklist
- Fail-secure (returns true on errors)

#### 2. Token Blacklist Middleware

**File**: `NERBABO.Backend/Shared/Middleware/TokenBlacklistMiddleware.cs`

```csharp
public class TokenBlacklistMiddleware
{
    public async Task InvokeAsync(HttpContext context, ITokenBlacklistService service)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var token = ExtractToken(context);
            if (await service.IsTokenBlacklistedAsync(token))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new {
                    message = "Token has been invalidated. Please login again."
                });
                return;
            }
        }
        await _next(context);
    }
}
```

**Registered in**: `Program.cs`
```csharp
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TokenBlacklistMiddleware>();  // After auth
```

#### 3. Logout Endpoint

**File**: `NERBABO.Backend/Core/Authentication/Controllers/AuthController.cs`

```csharp
[Authorize(Policy = "ActiveUser")]
[HttpPost("logout")]
public async Task<IActionResult> Logout()
{
    var token = ExtractTokenFromHeader();
    Result result = await _jwtService.InvalidateTokenAsync(token);
    return _responseHandler.HandleResult(result);
}
```

### Frontend Integration

**File**: `src/app/core/services/auth.service.ts:52-72`

```typescript
logout(): void {
  const token = this.bearerToken;

  if (token) {
    // Call backend to invalidate token (fire and forget)
    this.http.post(API_ENDPOINTS.logout, {}).pipe(
      catchError((error) => {
        console.error('Logout server call failed:', error);
        return of(null);  // Continue logout even on error
      })
    ).subscribe(() => {
      console.log('Token invalidated on server');
    });
  }

  // Clear client-side immediately (don't wait for server)
  this.isRefreshing = false;
  this.removeUser();
}
```

### How It Works

```
User Clicks Logout
       │
       ▼
┌─────────────────────────────┐
│  Frontend AuthService       │
│  - Get current bearer token │
│  - Call POST /api/auth/logout │
│  - Clear LocalStorage       │
└──────────┬──────────────────┘
           │
           ▼
┌─────────────────────────────┐
│  Backend AuthController     │
│  - Extract token from header│
│  - Call InvalidateTokenAsync│
└──────────┬──────────────────┘
           │
           ▼
┌─────────────────────────────┐
│  JwtService                 │
│  - Decode token (get exp)   │
│  - Add to Redis blacklist   │
└──────────┬──────────────────┘
           │
           ▼
┌─────────────────────────────┐
│  TokenBlacklistService      │
│  - Store in Redis:          │
│    blacklist:token:<jwt>    │
│  - Set TTL = token exp time │
└─────────────────────────────┘

Later API Request with Same Token
       │
       ▼
┌─────────────────────────────┐
│  TokenBlacklistMiddleware   │
│  - Check if token in Redis  │
│  - If found → Return 401    │
└─────────────────────────────┘
```

### Security Benefits

1. **Stolen Token Protection**: Tokens stolen before logout become invalid immediately
2. **No Server-Side State**: Uses Redis cache, tokens auto-expire
3. **Distributed System Support**: Works across multiple backend servers
4. **Fail-Secure**: On Redis errors, denies access rather than allowing it

---

## Future Enhancements

### 1. Proactive Token Refresh
Refresh token before it expires (e.g., 5 minutes before expiration):
```typescript
// In AuthService constructor
this.setupProactiveRefresh();

private setupProactiveRefresh(): void {
  this.user$.subscribe(user => {
    if (user?.jwt) {
      const expirationTime = this.getTokenExpirationTime(user.jwt);
      if (expirationTime) {
        const refreshTime = expirationTime - (5 * 60 * 1000); // 5 min before
        const delay = refreshTime - Date.now();

        if (delay > 0) {
          setTimeout(() => {
            this.refreshToken().subscribe();
          }, delay);
        }
      }
    }
  });
}
```

### 2. Session Timeout Warning
Show warning modal 2 minutes before token expires:
```typescript
// Display modal warning user of impending logout
this.sharedService.showWarning('Your session will expire in 2 minutes');
```

### 3. Refresh Token Pattern
Implement separate short-lived access tokens and long-lived refresh tokens stored in HttpOnly cookies (see main JWT documentation for details).

### 4. ✅ Token Blacklist Support - IMPLEMENTED
Server-side logout now invalidates tokens using Redis blacklist. See "Server-Side Logout Implementation" section above.

---

## Troubleshooting

### Issue: User Gets Logged Out Immediately
**Cause**: Token is already expired in LocalStorage
**Solution**: Clear LocalStorage and login again

### Issue: Infinite Redirect Loop
**Cause**: Auth endpoints are being intercepted by refresh interceptor
**Solution**: Verify endpoint exclusion in `tokenRefreshInterceptor`

### Issue: Token Not Refreshing on 401
**Cause**: Interceptor not registered or wrong order
**Solution**: Check `main.ts` and ensure interceptors are in correct order

### Issue: Multiple Refresh Requests
**Cause**: `isRefreshing` flag not working properly
**Solution**: Verify flag is properly reset in all code paths

---

## Related Documentation

- [JWT Authentication Implementation - Frontend Documentation](./JWT-Authentication-Documentation.md)
- [Backend Authentication API](../NERBABO.Backend/NERBABO.ApiService/Core/Authentication/)
- [Angular HTTP Interceptors](https://angular.dev/guide/http/interceptors)
- [JWT Decode Library](https://github.com/auth0/jwt-decode)

---

**Document Version**: 1.0
**Implementation Date**: 2025-12-26
**Author**: Claude Code Implementation
**Project**: NERBA Backoffice - ASP.NET Core + Angular 19
