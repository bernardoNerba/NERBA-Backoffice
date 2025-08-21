# NERBA Frontend

This is the frontend portion of the NERBA Backoffice system, built with Angular 19 and TypeScript. The frontend provides a comprehensive user interface for managing training programs, people, companies, and administrative tasks within the NERBA organization.

## Architecture Overview

The NERBA Frontend is a modern Angular single-page application (SPA) that follows Angular best practices and provides:

- **Modern Angular Architecture**: Built with Angular 19 using standalone components and signals
- **Responsive Design**: Bootstrap 5-based responsive UI with custom styling
- **JWT Authentication**: Secure authentication with role-based access control
- **Component Library**: Comprehensive reusable component library using PrimeNG
- **Type Safety**: Full TypeScript implementation with strong typing
- **Lazy Loading**: Feature modules loaded on demand for optimal performance
- **Guard Protection**: Route protection with authentication and authorization guards

## Core Features

### Authentication & Authorization

- **JWT Token Management**: Secure token storage and automatic refresh
- **Role-Based Access**: Support for Admin, User, CQ (Quality Control), and FM (Financial Manager) roles
- **Route Guards**: Protected routes with automatic redirects
- **Session Management**: Automatic logout and session handling

### Training Management

- **Course Management**: Create, edit, and manage training courses
- **Module Management**: Organize training content into modules
- **Session Scheduling**: Schedule and track training sessions
- **Action Tracking**: Monitor and manage course-related actions

### People Management

- **Student Management**: Track student enrollments and progress
- **Teacher Management**: Manage instructor profiles and assignments
- **People Database**: Comprehensive contact and personal information management
- **Role Assignments**: Flexible role assignment system

### Business Management

- **Company Management**: Manage corporate clients and relationships
- **Frame Management**: Document template and branding management
- **PDF Generation**: Generate training reports and certificates
- **Global Configuration**: System-wide settings and tax management

### User Interface

- **Dashboard**: Overview of key metrics and recent activities
- **Data Tables**: Advanced data tables with sorting, filtering, and pagination
- **Forms**: Comprehensive form handling with validation
- **File Upload**: Image and document upload functionality
- **Responsive Design**: Mobile-friendly responsive layout

## Project Structure

```text
NERBABO.Frontend/
├── src/
│   ├── app/
│   │   ├── core/                     # Core functionality
│   │   │   ├── interfaces/           # TypeScript interfaces
│   │   │   ├── models/               # Data models and types
│   │   │   ├── objects/              # Static data and constants
│   │   │   └── services/             # Business logic services
│   │   ├── features/                 # Feature modules
│   │   │   ├── auth/                 # Authentication features
│   │   │   ├── dashboard/            # Dashboard and overview
│   │   │   ├── courses/              # Course management
│   │   │   ├── modules/              # Module management
│   │   │   ├── sessions/             # Session scheduling
│   │   │   ├── people/               # People management
│   │   │   ├── students/             # Student management
│   │   │   ├── teachers/             # Teacher management
│   │   │   ├── companies/            # Company management
│   │   │   ├── actions/              # Action management
│   │   │   ├── frames/               # Frame management
│   │   │   ├── global-config/        # Global settings
│   │   │   └── acc/                  # Account management
│   │   └── shared/                   # Shared components and utilities
│   │       ├── components/           # Reusable UI components
│   │       ├── guards/               # Route guards
│   │       ├── interceptors/         # HTTP interceptors
│   │       ├── pipes/                # Custom pipes
│   │       └── utils.ts              # Utility functions
│   ├── environments/                 # Environment configurations
│   └── styles.css                    # Global styles
├── public/                           # Static assets
├── angular.json                      # Angular configuration
├── package.json                      # Dependencies and scripts
└── tsconfig.json                     # TypeScript configuration
```

## Development Standards and Conventions

### Architecture Patterns

- **Standalone Components**: Angular 19 standalone components for better tree-shaking
- **Service Layer**: Business logic encapsulated in injectable services
- **Reactive Programming**: RxJS observables for state management and async operations
- **Type Safety**: Strong TypeScript typing throughout the application
- **Dependency Injection**: Angular's DI system for service management

### Component Organization

Each feature follows a consistent CRUD structure:

```text
Feature/
├── index-{feature}/              # List/table view
├── view-{feature}/               # Detail view
├── upsert-{feature}/             # Create/update form
└── delete-{feature}/             # Delete confirmation
```

### Naming Conventions

- **Components**: `{action}-{entity}` (e.g., `index-courses`, `upsert-students`)
- **Services**: `{entity}.service.ts` (e.g., `courses.service.ts`)
- **Models**: `{entity}.ts` (e.g., `course.ts`, `student.ts`)
- **Interfaces**: `I{Purpose}` (e.g., `IIndex`, `IUpsert`, `IView`)
- **Guards**: `{purpose}.guard.ts` (e.g., `auth.guard.ts`)
- **Pipes**: `{function}.pipe.ts` (e.g., `format-date-range.pipe.ts`)

### Code Style Standards

- **TypeScript**: Strict type checking enabled
- **Component Structure**: Consistent component lifecycle and organization
- **Reactive Forms**: Angular reactive forms for all user input
- **Error Handling**: Centralized error handling with user feedback
- **Accessibility**: ARIA labels and semantic HTML
- **Performance**: OnPush change detection where applicable

### State Management

- **Services**: State management through injectable services
- **BehaviorSubjects**: Reactive state with BehaviorSubject patterns
- **Local Storage**: Persistent storage for user sessions and preferences
- **HTTP Caching**: Strategic caching of API responses

## Dependencies

### Core Framework

- **@angular/core** (^19.2.0): Angular framework core
- **@angular/common** (^19.2.0): Common Angular functionality
- **@angular/forms** (^19.2.0): Reactive and template-driven forms
- **@angular/router** (^19.2.0): Client-side routing
- **TypeScript** (~5.7.2): Static typing for JavaScript

### UI Framework & Styling

- **bootstrap** (^5.3.6): CSS framework for responsive design
- **bootstrap-icons** (^1.13.1): Bootstrap icon library
- **ngx-bootstrap** (^19.0.2): Bootstrap components for Angular
- **primeng** (^19.1.3): Rich UI component library
- **@primeng/themes** (^19.1.3): PrimeNG theming system
- **primeicons** (^7.0.0): PrimeNG icon library

### Icons & Graphics

- **@fortawesome/angular-fontawesome** (^1.0.0): FontAwesome integration
- **@fortawesome/free-solid-svg-icons** (^6.7.1): Solid FontAwesome icons
- **@fortawesome/free-regular-svg-icons** (^6.7.1): Regular FontAwesome icons
- **@fortawesome/free-brands-svg-icons** (^6.7.1): Brand FontAwesome icons

### Authentication & Security

- **jwt-decode** (^4.0.0): JWT token decoding and validation

### Validation & Forms

- **ngx-validators** (^6.0.1): Extended form validation library

### Development Tools

- **@angular/cli** (^19.2.7): Angular command line interface
- **@angular-devkit/build-angular** (^19.2.7): Angular build tools
- **jasmine-core** (~5.6.0): Testing framework
- **karma** (~6.4.0): Test runner

### Runtime Dependencies

- **rxjs** (~7.8.0): Reactive programming library
- **zone.js** (~0.15.0): Zone-based change detection
- **tslib** (^2.3.0): TypeScript runtime library

## Getting Started

### Prerequisites

- Node.js (18.x or higher)
- npm (9.x or higher)
- Angular CLI (optional but recommended)

### Installation

1. **Navigate to the frontend directory:**

   ```bash
   cd NERBABO.Frontend
   ```

2. **Install dependencies:**

   ```bash
   npm install
   ```

3. **Install Angular CLI globally (optional):**

   ```bash
   npm install -g @angular/cli
   ```

### Development

1. **Start the development server:**

   ```bash
   npm start
   # or
   ng serve
   ```

2. **Build for development:**

   ```bash
   npm run build:dev
   # or
   ng build --configuration development
   ```

3. **Build for production:**

   ```bash
   npm run build:prod
   # or
   ng build --configuration production
   ```

4. **Run tests:**

   ```bash
   npm test
   # or
   ng test
   ```

### Development URLs

- **Frontend**: http://localhost:4200
- **API Backend**: http://localhost:8080 (configured in environment)

## Environment Configuration

The application supports multiple environments:

### Development Environment

```typescript
environment = {
  production: false,
  appUrl: "http://localhost:8080",
  userKey: "NerbaBackofficeUser",
  roles: ["Admin", "User", "CQ", "FM"],
  getApiUrl: () => string, // Dynamic API URL resolution
};
```

### Production Environment

- Optimized builds with minification
- Source maps disabled
- Cache busting enabled
- Environment-specific API endpoints

## Authentication System

The frontend implements a comprehensive authentication system:

### Features

- **JWT Token Management**: Automatic token storage and retrieval
- **Role-Based Access Control**: Different UI elements based on user roles
- **Route Protection**: Guards prevent unauthorized access
- **Session Persistence**: User sessions persist across browser restarts
- **Automatic Logout**: Session timeout and manual logout functionality

### User Roles

- **Admin**: Full system access and user management
- **User**: Standard user access to training features
- **CQ (Quality Control)**: Quality assurance and certification access
- **FM (Financial Manager)**: Financial and billing access

## Component Library

### Shared Components

#### Navigation

- **Navbar**: Main navigation with user menu and branding
- **Sidebar**: Collapsible sidebar with feature navigation
- **Breadcrumbs**: Navigation breadcrumbs for deep pages

#### Data Display

- **Tables**: Advanced data tables with sorting, filtering, and pagination
- **Cards**: Content cards with consistent styling
- **Badges**: Status indicators and labels
- **Icons**: Consistent icon usage across the application

#### Forms & Input

- **Form Controls**: Custom form controls with validation
- **File Upload**: Drag-and-drop file upload component
- **Date Pickers**: Custom date selection components
- **Validators**: Extended validation functions

#### Feedback

- **Alerts**: Success, warning, and error notifications
- **Messages**: User feedback and status messages
- **Spinners**: Loading indicators and progress bars
- **Modals**: Confirmation dialogs and forms

## Contributing Guidelines

### Code Quality Standards

1. **TypeScript**: Use strict typing and avoid `any` types
2. **Component Structure**: Follow Angular style guide conventions
3. **Template Syntax**: Use Angular template best practices
4. **Reactive Forms**: Use reactive forms for all user input
5. **Error Handling**: Implement proper error handling and user feedback

### Component Development

1. **Standalone Components**: Use Angular 19 standalone components
2. **Change Detection**: Use OnPush change detection for performance
3. **Lifecycle Hooks**: Implement proper component lifecycle management
4. **Accessibility**: Include ARIA labels and semantic HTML
5. **Responsive Design**: Ensure mobile-friendly layouts

### Service Development

1. **Injectable Services**: Use dependency injection for all services
2. **HTTP Handling**: Implement proper HTTP error handling
3. **State Management**: Use BehaviorSubjects for reactive state
4. **Caching**: Implement strategic caching for performance
5. **Type Safety**: Strong typing for all service methods

### Testing Requirements

1. **Unit Tests**: Write unit tests for components and services
2. **Integration Tests**: Test component interactions
3. **E2E Tests**: End-to-end testing for critical user flows
4. **Accessibility Testing**: Ensure WCAG compliance
5. **Performance Testing**: Monitor bundle size and runtime performance

### UI/UX Guidelines

1. **Consistency**: Follow established design patterns
2. **Responsive Design**: Mobile-first approach
3. **Accessibility**: WCAG 2.1 AA compliance
4. **Performance**: Optimize for fast loading and smooth interactions
5. **User Feedback**: Provide clear feedback for all user actions

## Performance Optimization

### Build Optimization

- **Lazy Loading**: Feature modules loaded on demand
- **Tree Shaking**: Unused code elimination
- **Bundle Analysis**: Regular bundle size monitoring
- **Code Splitting**: Strategic code splitting for optimal loading

### Runtime Optimization

- **OnPush Change Detection**: Optimized change detection strategy
- **TrackBy Functions**: Efficient list rendering
- **Async Pipes**: Memory leak prevention
- **Subscription Management**: Proper observable cleanup

## Security Considerations

### Authentication Security

- **JWT Storage**: Secure token storage in localStorage
- **Token Validation**: Client-side token validation
- **Route Protection**: Comprehensive route guarding
- **Session Management**: Secure session handling

### Input Validation

- **Client-Side Validation**: Comprehensive form validation
- **XSS Prevention**: Sanitized user input
- **CSRF Protection**: Cross-site request forgery prevention
- **Secure Communication**: HTTPS enforcement

## Testing Strategy

### Unit Testing

- **Component Testing**: Test component logic and templates
- **Service Testing**: Test business logic and HTTP interactions
- **Pipe Testing**: Test custom pipe functionality
- **Guard Testing**: Test route guard behavior

### Integration Testing

- **Component Integration**: Test component interactions
- **Service Integration**: Test service dependencies
- **Router Testing**: Test navigation and guards
- **HTTP Testing**: Test API communication

### E2E Testing

- **User Workflows**: Test complete user scenarios
- **Authentication Flows**: Test login and logout processes
- **Data Management**: Test CRUD operations
- **Error Scenarios**: Test error handling and recovery

## Deployment

### Production Build

```bash
npm run build:prod
```

### Docker Deployment

The application includes Dockerfile for containerized deployment:

- **Multi-stage build**: Optimized production image
- **Nginx server**: Efficient static file serving
- **Environment configuration**: Runtime environment setup

### CI/CD Integration

- **Automated builds**: Continuous integration pipeline
- **Quality gates**: Code quality and test coverage checks
- **Deployment automation**: Automated deployment to staging and production

## Browser Support

### Supported Browsers

- **Chrome**: Latest 2 versions
- **Firefox**: Latest 2 versions
- **Safari**: Latest 2 versions
- **Edge**: Latest 2 versions

### Progressive Enhancement

- **Core Functionality**: Works without JavaScript for critical features
- **Enhanced Experience**: Full SPA experience with JavaScript enabled
- **Offline Support**: Service worker for offline capabilities (future enhancement)

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](../LICENSE.txt) file for details.
