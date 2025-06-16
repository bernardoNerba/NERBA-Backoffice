export type Alert = {
  type: string;
  message: string;
  dismissible: boolean;
  timeout?: number;
};
