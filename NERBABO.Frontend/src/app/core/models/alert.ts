// core/models/alert.ts
export interface Alert {
  type:
    | 'success'
    | 'info'
    | 'warning'
    | 'danger'
    | 'error'
    | 'primary'
    | 'secondary'
    | 'light'
    | 'dark';
  message: string;
  title?: string; // Optional title for better toast display
  dismissible?: boolean; // Optional, defaults to true
  timeout?: number; // Optional timeout in milliseconds
  sticky?: boolean; // Optional, if true, toast won't auto-hide
}
