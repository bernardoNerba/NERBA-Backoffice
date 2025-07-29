export const environment = {
  production: false,
  appUrl: 'http://localhost:8080',
  userKey: 'NerbaBackofficeUser',
  roles: ['Admin', 'User', 'CQ', 'FM'],

  // Helper function to get API URL dynamically
  getApiUrl: (): string => {
    // In development, try to use the same host as the Angular app
    const currentHost = window.location.hostname;

    // If accessing via IP, use the same IP for API
    if (currentHost !== 'localhost' && currentHost !== '127.0.0.1') {
      return `http://${currentHost}:8080`;
    }

    // Default to localhost
    return 'http://localhost:8080';
  },
};

// Set the appUrl dynamically
environment.appUrl = environment.getApiUrl();
