import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../objects/apiEndpoints';

@Injectable({
  providedIn: 'root',
})
export class PdfService {
  private readonly baseUrl = API_ENDPOINTS.getBaseUrl();

  constructor(private http: HttpClient) {}

  /**
   * Generates a PDF report with all sessions for a specific action
   * @param actionId The action ID to generate the report for
   * @returns Observable of PDF blob
   */
  generateSessionsReport(actionId: number): Observable<Blob> {
    return this.http.get(
      `${this.baseUrl}/api/pdf/action/${actionId}/sessions-report`,
      {
        responseType: 'blob',
      }
    );
  }

  /**
   * Generates a PDF cover page for a specific action
   * @param actionId The action ID to generate the cover for
   * @returns Observable of PDF blob
   */
  generateCoverReport(actionId: number): Observable<Blob> {
    return this.http.get(
      `${this.baseUrl}/api/pdf/action/${actionId}/cover-report`,
      {
        responseType: 'blob',
      }
    );
  }

  /**
   * Downloads a PDF blob with the specified filename
   * @param blob The PDF blob to download
   * @param filename The filename to use for download
   */
  downloadPdf(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  }

  /**
   * Opens a PDF blob in a new browser tab
   * @param blob The PDF blob to view
   */
  viewPdf(blob: Blob): void {
    const url = window.URL.createObjectURL(blob);
    window.open(url, '_blank');
  }

  /**
   * Prints a PDF blob using the browser's print function
   * @param blob The PDF blob to print
   */
  printPdf(blob: Blob): void {
    const url = window.URL.createObjectURL(blob);
    const iframe = document.createElement('iframe');
    iframe.style.display = 'none';
    iframe.src = url;
    document.body.appendChild(iframe);

    iframe.onload = () => {
      iframe.contentWindow?.print();
      setTimeout(() => {
        document.body.removeChild(iframe);
        window.URL.revokeObjectURL(url);
      }, 1000);
    };
  }
}
