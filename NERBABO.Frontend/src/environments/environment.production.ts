// environment.production.ts
export const environment = {
  production: true,
  appUrl: '', // Will be set dynamically
  userKey: 'NerbaBackofficeUser',
  roles: ['Admin', 'User', 'CQ', 'FM'],

  // Helper function to get API URL dynamically
  getApiUrl: (): string => {
    // In production, use the same origin as the Angular app
    // Nginx reverse proxy will route /api requests to the backend
    // This works for both:
    // 1. Direct access via docker-compose (ports exposed)
    // 2. Access via nginx reverse proxy on port 80/443

    // Check if we're behind a reverse proxy (standard web ports)
    const port = window.location.port;
    const protocol = window.location.protocol;
    const hostname = window.location.hostname;

    // If no port or standard ports (80/443), use same origin (nginx routing)
    if (!port || port === '80' || port === '443') {
      return window.location.origin;
    }

    // If on port 4200 (direct Docker access), API is on port 5001
    if (port === '4200') {
      return `${protocol}//${hostname}:5001`;
    }

    // Default: same origin (let nginx handle it)
    return window.location.origin;
  },
};

// Set the appUrl dynamically
environment.appUrl = environment.getApiUrl();
