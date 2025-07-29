// environment.production.ts
export const environment = {
  production: true,
  appUrl: '', // Will be set dynamically
  userKey: 'NerbaBackofficeUser',
  roles: ['Admin', 'User', 'CQ', 'FM'],

  // Helper function to get API URL dynamically
  getApiUrl: (): string => {
    // In production, use the same host as the Angular app
    const protocol = window.location.protocol;
    const hostname = window.location.hostname;

    // Assume API on port 5001
    return `${protocol}//${hostname}:5001`;
  },
};

// Set the appUrl dynamically
environment.appUrl = environment.getApiUrl();
