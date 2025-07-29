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

    // First, try to use environment configuration if available
    if (environment.appUrl && environment.appUrl !== '') {
      return environment.appUrl;
    }

    // Fallback: construct URL based on current location
    // Assume API is running on port 5001 (as per docker-compose)
    return `${protocol}//${hostname}:5001`;
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
