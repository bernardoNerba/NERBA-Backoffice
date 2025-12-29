# JWT Authentication Implementation - Frontend Documentation

## Table of Contents
1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Authentication Flow](#authentication-flow)
4. [Core Components](#core-components)
5. [Security Analysis](#security-analysis)
6. [Improvement Recommendations](#improvement-recommendations)
7. [Usage Examples](#usage-examples)

---

## Overview

The NERBA Backoffice application implements JWT (JSON Web Token) based authentication for securing its Angular frontend and ASP.NET Core backend communication. This document focuses on the **frontend implementation** of JWT authentication, including token handling, verification, and route protection.

### Technology Stack
- **Frontend**: Angular 19 (Standalone Components)
- **JWT Library**: `jwt-decode` (for client-side token parsing)
- **Storage**: Browser LocalStorage
- **State Management**: RxJS BehaviorSubject

---

## Architecture

### High-Level Authentication Flow

```
┌─────────────┐         ┌──────────────┐         ┌─────────────┐
│   User      │         │   Angular    │         │   ASP.NET   │
│  (Browser)  │         │   Frontend   │         │   Backend   │
└─────────────┘         └──────────────┘         └─────────────┘
      │                        │                        │
      │  1. Login Request      │                        │
      ├───────────────────────>│                        │
      │                        │  2. POST /api/auth/login
      │                        ├───────────────────────>│
      │                        │                        │
      │                        │  3. JWT Token Response │
      │                        │<───────────────────────┤
      │                        │                        │
      │  4. Store JWT          │                        │
      │    (LocalStorage)      │                        │
      │<───────────────────────┤                        │
      │                        │                        │
      │  5. API Request        │                        │
      │    (with Bearer Token) │                        │
      │                        ├───────────────────────>│
      │                        │  6. Verify JWT         │
      │                        │     & Process          │
      │                        │<───────────────────────┤
```

### File Structure

```
NERBABO.Frontend/src/app/
├── core/
│   ├── models/
│   │   ├── user.ts                    # User model (firstName, lastName, jwt)
│   │   ├── login.ts                   # Login request model
│   │   └── jwtPayload.ts              # JWT claims structure
│   ├── services/
│   │   └── auth.service.ts            # Main authentication service
│   └── objects/
│       └── apiEndpoints.ts            # API endpoint definitions
├── shared/
│   ├── guards/
│   │   ├── auth.guard.ts              # Protected route guard
│   │   └── unauth-only.guard.ts       # Login-only route guard
│   └── interceptors/
│       └── auth.interceptor.ts        # HTTP request interceptor
└── features/
    └── auth/
        ├── login/
        │   └── login.component.ts     # Login page component
        └── logout/
            └── logout.component.ts    # Logout component
```

---

## Authentication Flow

### 1. Login Process

**Location**: `src/app/features/auth/login/login.component.ts:77-104`

```typescript
// User submits credentials
login() {
  this.authService.login(this.loginForm.value as Login).subscribe({
    next: () => {
      // Navigate to dashboard or returnUrl
      this.router.navigateByUrl(this.returnUrl || '/dashboard');
    },
    error: (error) => {
      // Display error message
      this.sharedService.handleErrorResponse(error);
    }
  });
}
```

**Process**:
1. User enters `usernameOrEmail` and `password` in login form
2. Form validation checks (both fields required)
3. POST request to `/api/auth/login/`
4. Backend validates credentials and returns User object with JWT
5. Response structure:
   ```json
   {
     "firstName": "Admin",
     "lastName": "Admin",
     "jwt": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
   }
   ```
6. User object stored in LocalStorage
7. BehaviorSubject emits new user state
8. Redirect to dashboard or original requested URL

### 2. Token Storage

**Location**: `src/app/core/services/auth.service.ts:67-71`

```typescript
private setUser(user: User): void {
  // Sets a user to the local storage
  localStorage.setItem(environment.userKey, JSON.stringify(user));
  this.userSubject.next(user);
}
```

**Storage Details**:
- **Key**: `NerbaBackofficeUser` (configurable in `environment.ts`)
- **Value**: Stringified JSON of User object
- **Persistence**: Survives browser refresh
- **Scope**: Per domain/origin

### 3. Token Retrieval & Injection

**Location**: `src/app/shared/interceptors/auth.interceptor.ts:5-17`

The HTTP interceptor automatically attaches the JWT to all outgoing requests:

```typescript
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.bearerToken;

  if (token) {
    const cloned = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
    return next(cloned);
  }
  return next(req);
};
```

**Registered in**: `src/main.ts:36`
```typescript
provideHttpClient(withInterceptors([authInterceptor]))
```

### 4. Route Protection

**Location**: `src/app/shared/guards/auth.guard.ts:12-44`

Protected routes check authentication status before allowing access:

```typescript
export const authGuard: CanActivateFn = (route, state): Observable<boolean | UrlTree> => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.user$.pipe(
    map((user: User | null) => {
      if (user) {
        return true; // Allow access
      }
      // Redirect to login with returnUrl
      return router.createUrlTree(['login'], {
        queryParams: { returnUrl: state.url }
      });
    })
  );
};
```

**Usage in Routes**: `src/app/app.routes.ts`
```typescript
{
  path: 'dashboard',
  loadComponent: () => import('./features/dashboard/dashboard.component'),
  canActivate: [authGuard] // Protected route
}
```

### 5. JWT Decoding & Claims

**Location**: `src/app/core/services/auth.service.ts:79-90`

```typescript
get getUserClaims(): any {
  const token = this.userSubject.getValue()?.jwt;
  if (!token) return null;

  try {
    const decoded = jwtDecode<JwtPayload>(token);
    return decoded;
  } catch (err) {
    console.error('Invalid token', err);
    return null;
  }
}
```

**JWT Payload Structure**: `src/app/core/models/jwtPayload.ts:1-7`
```typescript
export interface JwtPayload {
  nameid: string;        // User ID
  email: string;         // User email
  given_name: string;    // First name
  family_name: string;   // Last name
  role: Array<string>;   // User roles (e.g., ['Admin', 'User'])
}
```

### 6. Role-Based Authorization

**Location**: `src/app/core/services/auth.service.ts:92-106`

```typescript
get userRoles(): string[] {
  const claims = this.getUserClaims;
  let roles = [];
  if (!Array.isArray(claims?.role)) {
    roles.push(claims?.role);
  } else {
    roles = claims?.role;
  }
  return roles;
}

get isUserAdmin(): boolean {
  const claims = this.getUserClaims;
  return Array.isArray(claims?.role) && claims.role.includes('Admin');
}
```

**Available Roles**: `src/environments/environment.development.ts:5`
- `Admin` - Full system access
- `User` - Basic user access
- `CQ` - Quality coordinator
- `FM` - Finance manager

### 7. Logout Process

**Location**: `src/app/features/auth/logout/logout.component.ts:22-26`

```typescript
private logout(): void {
  this.authService.logout();
  this.sharedService.showSuccess('Logout efetuado com sucesso.');
  this.router.navigateByUrl('/login');
}
```

**Auth Service Logout**: `src/app/core/services/auth.service.ts:52-72`
```typescript
logout(): void {
  // Call backend to invalidate token
  const token = this.bearerToken;

  if (token) {
    // Make server call to invalidate token (fire and forget)
    this.http.post(API_ENDPOINTS.logout, {}).pipe(
      catchError((error) => {
        console.error('Logout server call failed:', error);
        // Continue with client-side logout even if server call fails
        return of(null);
      })
    ).subscribe(() => {
      console.log('Token invalidated on server');
    });
  }

  // Clear user data and reset refresh state immediately
  this.isRefreshing = false;
  this.removeUser();
}

private removeUser(): void {
  localStorage.removeItem(environment.userKey);
  this.userSubject.next(null);
}
```

**Backend Logout Endpoint**: `/api/auth/logout`

The logout process now includes:
1. **Client-Side**: Removes user from LocalStorage and clears state
2. **Server-Side**: Adds token to Redis blacklist (invalidates it)
3. **Token Blacklist**: Tokens are stored in Redis until their natural expiration
4. **Middleware Check**: All authenticated requests check if token is blacklisted

---

## Core Components

### 1. AuthService

**File**: `src/app/core/services/auth.service.ts`

**Responsibilities**:
- User authentication (login)
- Token storage management
- User state management via RxJS
- JWT decoding and claim extraction
- Role verification
- Logout handling

**Key Properties**:
```typescript
private userSubject = new BehaviorSubject<User | null>(null);
user$ = this.userSubject.asObservable(); // Observable for reactive updates
```

**Key Methods**:
- `login(model: Login): Observable<void>` - Authenticate user
- `logout(): void` - Clear user session
- `get isAuthenticated: boolean` - Check auth status
- `get bearerToken: string | undefined` - Get current JWT
- `get getUserClaims: JwtPayload | null` - Decode JWT
- `get userRoles: string[]` - Get user roles
- `get isUserAdmin: boolean` - Check admin status

### 2. Auth Interceptor

**File**: `src/app/shared/interceptors/auth.interceptor.ts`

**Purpose**: Automatically inject Bearer token into all HTTP requests

**Behavior**:
- Intercepts ALL outgoing HTTP requests
- Checks for valid JWT token
- Adds `Authorization: Bearer <token>` header if token exists
- Passes request through unchanged if no token

### 3. Auth Guard

**File**: `src/app/shared/guards/auth.guard.ts`

**Purpose**: Protect routes that require authentication

**Behavior**:
- Checks `user$` observable for authenticated user
- Allows access if user exists
- Redirects to `/login` with `returnUrl` query param if not authenticated
- Shows error toast: "Área Restrita"

### 4. Unauth-Only Guard

**File**: `src/app/shared/guards/unauth-only.guard.ts`

**Purpose**: Prevent authenticated users from accessing login page

**Behavior**:
- Checks `user$` observable
- Allows access to login page if NO user
- Redirects to `/dashboard` if already authenticated

### 5. Models

**User Model** (`src/app/core/models/user.ts`):
```typescript
export type User = {
  firstName: string;
  lastName: string;
  jwt: string;
};
```

**Login Model** (`src/app/core/models/login.ts`):
```typescript
export type Login = {
  usernameOrEmail: string;
  password: string;
};
```

**JWT Payload** (`src/app/core/models/jwtPayload.ts`):
```typescript
export interface JwtPayload {
  nameid: string;
  email: string;
  given_name: string;
  family_name: string;
  role: Array<string>;
}
```

---

## Security Analysis

### Current Security Measures

#### ✅ Strengths

1. **HTTPS Support**: Application supports HTTPS in production
2. **JWT-Based Stateless Auth**: Scalable authentication mechanism
3. **HTTP-Only Interceptor**: Automatic token injection prevents manual errors
4. **Route Guards**: Prevent unauthorized access to protected routes
5. **Role-Based Access Control**: Extracts and validates user roles from JWT
6. **Return URL Tracking**: Preserves user navigation intent after login
7. **Error Handling**: Graceful error handling for invalid tokens

### Security Concerns & Vulnerabilities

#### ⚠️ Critical Issues

1. **LocalStorage Vulnerability (XSS)**
   - **Issue**: JWT stored in LocalStorage is vulnerable to XSS attacks
   - **Risk**: Malicious scripts can steal tokens
   - **Location**: `src/app/core/services/auth.service.ts:87`
   - **Impact**: HIGH - Complete account takeover possible
   - **Recommendation**: Consider HttpOnly cookies (see recommendations below)

2. **No CSRF Protection Mentioned**
   - **Issue**: No visible CSRF token implementation
   - **Risk**: Cross-Site Request Forgery attacks
   - **Impact**: MEDIUM - Depends on backend implementation

#### ✅ Resolved Issues

The following issues have been addressed in the current implementation:

1. **✅ Token Expiration Checking** - IMPLEMENTED
   - Token expiration is validated on application load
   - Expired tokens are not returned by `bearerToken` getter
   - Location: `src/app/core/services/auth.service.ts:102-115`

2. **✅ Token Refresh Mechanism** - IMPLEMENTED
   - Automatic token refresh on 401 responses
   - Refresh endpoint: `GET /api/auth/refresh-user-token`
   - Location: `src/app/core/services/auth.service.ts:135-160`

3. **✅ Server-Side Logout** - IMPLEMENTED
   - Logout endpoint: `POST /api/auth/logout`
   - Tokens added to Redis blacklist on logout
   - Middleware checks blacklist on all authenticated requests
   - Backend Location: `NERBABO.Backend/Core/Authentication/Services/TokenBlacklistService.cs`
   - Frontend Location: `src/app/core/services/auth.service.ts:52-72`

#### ⚠️ Medium Issues

3. **Console Logging in Production**
   - **Issue**: Debug console logs in guards expose auth state
   - **Location**: `src/app/shared/guards/auth.guard.ts:21-31`
   - **Impact**: LOW - Information leakage

4. **No Token Validation Format**
   - **Issue**: No validation of JWT structure before storage
   - **Risk**: Malformed tokens could cause runtime errors
   - **Impact**: LOW - Application stability

5. **Hardcoded Role Names**
   - **Issue**: Roles defined as magic strings
   - **Location**: `src/environments/environment.development.ts:5`
   - **Impact**: LOW - Maintenance difficulty

---

---

## Backend Implementation Details

### Token Blacklist Service

**File**: `NERBABO.Backend/Core/Authentication/Services/TokenBlacklistService.cs`

The backend uses Redis distributed cache to store blacklisted tokens:

```csharp
public async Task BlacklistTokenAsync(string token, DateTime expirationTime)
{
    var key = $"blacklist:token:{token}";
    var options = new DistributedCacheEntryOptions
    {
        AbsoluteExpiration = expirationTime
    };
    await _cache.SetAsync(key, value, options);
}

public async Task<bool> IsTokenBlacklistedAsync(string token)
{
    var key = $"blacklist:token:{token}";
    var value = await _cache.GetAsync(key);
    return value != null;
}
```

**Key Features**:
- Tokens automatically removed from blacklist after expiration
- Uses Redis for distributed caching (scales across multiple servers)
- Fail-secure: Returns true (deny access) on errors

### Token Blacklist Middleware

**File**: `NERBABO.Backend/Shared/Middleware/TokenBlacklistMiddleware.cs`

Middleware runs after authentication to check if tokens are blacklisted:

```csharp
public async Task InvokeAsync(HttpContext context, ITokenBlacklistService tokenBlacklistService)
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var token = ExtractTokenFromHeader(context);
        var isBlacklisted = await tokenBlacklistService.IsTokenBlacklistedAsync(token);

        if (isBlacklisted)
        {
            context.Response.StatusCode = 401;
            return; // Stop processing
        }
    }
    await _next(context);
}
```

### Logout Endpoint

**File**: `NERBABO.Backend/Core/Authentication/Controllers/AuthController.cs`

```csharp
[Authorize(Policy = "ActiveUser")]
[HttpPost("logout")]
public async Task<IActionResult> Logout()
{
    var token = ExtractTokenFromAuthHeader();
    Result result = await _jwtService.InvalidateTokenAsync(token);
    return _responseHandler.HandleResult(result);
}
```

The `InvalidateTokenAsync` method:
1. Decodes token to get expiration time
2. Adds token to Redis blacklist with expiration
3. Returns success/failure result

---

## Improvement Recommendations

### High Priority

#### 1. Implement HttpOnly Cookies for Token Storage

**Problem**: LocalStorage is vulnerable to XSS attacks

**Solution**: Store JWT in HttpOnly cookies instead

**Implementation**:
```typescript
// Backend: Set HttpOnly cookie on login
Response.Cookies.Append("auth-token", jwt, new CookieOptions {
    HttpOnly = true,
    Secure = true,
    SameSite = SameSiteMode.Strict,
    MaxAge = TimeSpan.FromHours(24)
});

// Frontend: Interceptor remains same (cookies sent automatically)
// Remove localStorage usage from AuthService
```

**Benefits**:
- XSS attacks cannot access HttpOnly cookies
- Browser handles cookie security
- Automatic CSRF protection with SameSite attribute
- **Status**: Not yet implemented (LocalStorage still in use)

#### 2. Implement Refresh Token Pattern

**Current**: Single JWT token with 7-day expiration

**Recommendation**: Use short-lived access tokens (15-30 min) + long-lived refresh tokens (7 days)

**Benefits**:
- Reduced risk if access token is stolen
- Refresh tokens can be stored in HttpOnly cookies
- Better security posture overall

**Status**: Not yet implemented (single token pattern in use)

### Medium Priority

#### 3. Remove Debug Console Logs

**Problem**: Sensitive information logged in production

**Solution**: Conditional logging based on environment

**Implementation**:
```typescript
// src/app/shared/guards/auth.guard.ts
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.user$.pipe(
    tap((user) => {
      // Only log in development
      if (!environment.production) {
        console.log('Auth Guard User:', user);
      }
    }),
    // ... rest of implementation
  );
};
```

#### 4. Implement Role-Based Route Guards

**Problem**: Only basic authentication check exists

**Solution**: Create role-specific guards

**Implementation**:
```typescript
// src/app/shared/guards/role.guard.ts
export function roleGuard(allowedRoles: string[]): CanActivateFn {
  return (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);
    const sharedService = inject(SharedService);

    const userRoles = authService.userRoles;
    const hasRole = allowedRoles.some(role => userRoles.includes(role));

    if (!hasRole) {
      sharedService.showError('Acesso negado. Permissões insuficientes.');
      return router.createUrlTree(['dashboard']);
    }

    return true;
  };
}

// Usage in routes
{
  path: 'admin/users',
  loadComponent: () => import('./features/admin/users.component'),
  canActivate: [authGuard, roleGuard(['Admin'])]
}
```

#### 5. Centralize Role Constants

**Problem**: Role names as magic strings

**Solution**: Create role enum/constants

**Implementation**:
```typescript
// src/app/core/enums/user-roles.enum.ts
export enum UserRoles {
  ADMIN = 'Admin',
  USER = 'User',
  CQ = 'CQ',
  FM = 'FM'
}

// Usage
import { UserRoles } from '@core/enums/user-roles.enum';

get isUserAdmin(): boolean {
  return this.userRoles.includes(UserRoles.ADMIN);
}
```

#### 6. Add Token Format Validation

**Problem**: No validation before storing tokens

**Solution**: Validate JWT format

**Implementation**:
```typescript
private isValidJWT(token: string): boolean {
  const parts = token.split('.');
  if (parts.length !== 3) {
    return false;
  }

  try {
    jwtDecode(token);
    return true;
  } catch {
    return false;
  }
}

private setUser(user: User): void {
  if (!this.isValidJWT(user.jwt)) {
    console.error('Invalid JWT format');
    throw new Error('Invalid authentication token');
  }

  localStorage.setItem(environment.userKey, JSON.stringify(user));
  this.userSubject.next(user);
}
```

### Low Priority

#### 7. Implement Session Timeout Warning

**Problem**: Silent session expiration

**Solution**: Warn users before token expires

**Implementation**:
```typescript
// Show warning 2 minutes before expiration
private setupExpirationWarning(): void {
  const token = this.bearerToken;
  if (!token) return;

  const decoded = jwtDecode<JwtPayload & { exp: number }>(token);
  const expiresAt = decoded.exp * 1000;
  const warningTime = expiresAt - (2 * 60 * 1000); // 2 min before
  const timeUntilWarning = warningTime - Date.now();

  if (timeUntilWarning > 0) {
    setTimeout(() => {
      this.sharedService.showWarning('Sua sessão expirará em 2 minutos');
    }, timeUntilWarning);
  }
}
```

#### 8. Add Security Headers

**Backend recommendation**: Implement security headers

```csharp
// Program.cs or Startup.cs
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add(
        "Content-Security-Policy",
        "default-src 'self'; script-src 'self' 'unsafe-inline'"
    );
    await next();
});
```

---

## Usage Examples

### Example 1: Accessing Current User

```typescript
import { Component, OnInit } from '@angular/core';
import { AuthService } from '@core/services/auth.service';

@Component({
  selector: 'app-profile',
  template: `
    <div *ngIf="user$ | async as user">
      <h2>Welcome, {{ user.firstName }} {{ user.lastName }}</h2>
    </div>
  `
})
export class ProfileComponent implements OnInit {
  user$ = this.authService.user$;

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    // Subscribe to user changes
    this.user$.subscribe(user => {
      if (user) {
        console.log('Current user:', user);
      }
    });
  }
}
```

### Example 2: Checking User Roles

```typescript
import { Component } from '@angular/core';
import { AuthService } from '@core/services/auth.service';

@Component({
  selector: 'app-admin-panel',
  template: `
    <div *ngIf="isAdmin">
      <h2>Admin Panel</h2>
      <button (click)="assignRole()">Assign Role</button>
    </div>
  `
})
export class AdminPanelComponent {
  get isAdmin(): boolean {
    return this.authService.isUserAdmin;
  }

  constructor(private authService: AuthService) {}

  assignRole(): void {
    const model = { userId: '123', role: 'Admin' };
    this.authService.assignRole(model).subscribe({
      next: (response) => console.log('Role assigned', response),
      error: (error) => console.error('Error assigning role', error)
    });
  }
}
```

### Example 3: Conditional UI Based on Authentication

```typescript
import { Component } from '@angular/core';
import { AuthService } from '@core/services/auth.service';

@Component({
  selector: 'app-navbar',
  template: `
    <nav>
      <a routerLink="/">Home</a>

      <ng-container *ngIf="isAuthenticated; else loginLink">
        <a routerLink="/dashboard">Dashboard</a>
        <a routerLink="/logout">Logout</a>
      </ng-container>

      <ng-template #loginLink>
        <a routerLink="/login">Login</a>
      </ng-template>
    </nav>
  `
})
export class NavbarComponent {
  get isAuthenticated(): boolean {
    return this.authService.isAuthenticated;
  }

  constructor(private authService: AuthService) {}
}
```

### Example 4: Manual Token Inspection

```typescript
import { Component, OnInit } from '@angular/core';
import { AuthService } from '@core/services/auth.service';

@Component({
  selector: 'app-debug',
  template: `<pre>{{ tokenInfo | json }}</pre>`
})
export class DebugComponent implements OnInit {
  tokenInfo: any;

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    const claims = this.authService.getUserClaims;
    const roles = this.authService.userRoles;

    this.tokenInfo = {
      claims: claims,
      roles: roles,
      isAdmin: this.authService.isUserAdmin,
      token: this.authService.bearerToken?.substring(0, 20) + '...'
    };
  }
}
```

### Example 5: Protected API Call

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '@core/objects/apiEndpoints';

@Injectable({ providedIn: 'root' })
export class CompanyService {
  constructor(private http: HttpClient) {}

  // Token automatically added by authInterceptor
  getAllCompanies(): Observable<Company[]> {
    return this.http.get<Company[]>(API_ENDPOINTS.companies);
  }

  // Works even on protected endpoints
  getCompanyById(id: number): Observable<Company> {
    return this.http.get<Company>(`${API_ENDPOINTS.companies}${id}`);
  }
}
```

---

## Testing Authentication

### Manual Testing Checklist

- [ ] **Login Flow**
  - [ ] Valid credentials → successful login → redirect to dashboard
  - [ ] Invalid credentials → error message displayed
  - [ ] After login, token stored in LocalStorage
  - [ ] After login, user state updated in BehaviorSubject

- [ ] **Protected Routes**
  - [ ] Accessing `/dashboard` without login → redirect to `/login`
  - [ ] After redirect, returnUrl query param contains original URL
  - [ ] After login, redirect to original URL

- [ ] **Logout Flow**
  - [ ] Click logout → token removed from LocalStorage
  - [ ] After logout → redirect to login page
  - [ ] After logout → cannot access protected routes

- [ ] **Token Injection**
  - [ ] API calls include `Authorization: Bearer <token>` header
  - [ ] Backend receives and validates token correctly
  - [ ] 401 responses trigger appropriate error handling

- [ ] **Role-Based Access**
  - [ ] Admin users can access admin-only features
  - [ ] Non-admin users cannot access admin features
  - [ ] Role checks work correctly in components

### Browser DevTools Inspection

1. **Check LocalStorage**:
   ```javascript
   // In browser console
   localStorage.getItem('NerbaBackofficeUser');
   ```

2. **Inspect JWT**:
   ```javascript
   const user = JSON.parse(localStorage.getItem('NerbaBackofficeUser'));
   console.log(user.jwt);

   // Decode at jwt.io
   ```

3. **Monitor Network Requests**:
   - Open DevTools → Network tab
   - Trigger API call
   - Check Request Headers for `Authorization: Bearer ...`

4. **Check User State**:
   ```javascript
   // In Angular component with AuthService injected
   console.log(this.authService.isAuthenticated);
   console.log(this.authService.userRoles);
   console.log(this.authService.getUserClaims);
   ```

---

## Conclusion

The NERBA Backoffice JWT authentication implementation provides a functional foundation for securing the Angular application. The architecture follows Angular best practices with services, guards, and interceptors working together seamlessly.

However, several **critical security improvements** are strongly recommended:
1. **Move to HttpOnly cookies** to prevent XSS attacks
2. **Implement server-side token invalidation** for secure logout
3. **Add token expiration validation** for better UX
4. **Implement automatic token refresh** to maintain sessions

These improvements will significantly enhance the security posture of the application while maintaining the current user experience.

---

## Additional Resources

- [Angular Security Guide](https://angular.dev/best-practices/security)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
- [jwt-decode Library Documentation](https://github.com/auth0/jwt-decode)

---

**Document Version**: 1.0
**Last Updated**: 2025-12-26
**Author**: Claude Code Analysis
**Project**: NERBA Backoffice - ASP.NET Core + Angular 19
