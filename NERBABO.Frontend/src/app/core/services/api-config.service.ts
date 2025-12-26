// api-config.service.ts
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class ApiConfigService {
  private _baseUrl: string;

  constructor() {
    this._baseUrl = this.determineApiUrl();
  }

  get baseUrl(): string {
    return this._baseUrl;
  }

  private determineApiUrl(): string {
    // In development, use the configured URL or detect dynamically
    if (!environment.production) {
      // Check if we have a dynamic getter
      if (typeof environment.getApiUrl === 'function') {
        return environment.getApiUrl();
      }
      return environment.appUrl || 'http://localhost:8080';
    }

    // In production, determine the API URL dynamically
    const protocol = window.location.protocol;
    const hostname = window.location.hostname;
    const port = window.location.port;

    // First, try to use environment configuration if available
    if (environment.appUrl && environment.appUrl !== '') {
      return environment.appUrl;
    }

    // Check if we have a dynamic getter function
    if (typeof environment.getApiUrl === 'function') {
      return environment.getApiUrl();
    }

    // Fallback: Detect if behind reverse proxy or direct access
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
  }

  // Method to test API connectivity
  async testConnectivity(): Promise<boolean> {
    try {
      const response = await fetch(`${this._baseUrl}/health`, {
        method: 'GET',
        mode: 'cors',
      });
      return response.ok;
    } catch (error) {
      console.warn('API connectivity test failed:', error);
      return false;
    }
  }

  // Method to update base URL if needed (for reconnection scenarios)
  updateBaseUrl(newUrl: string): void {
    this._baseUrl = newUrl;
  }
}
