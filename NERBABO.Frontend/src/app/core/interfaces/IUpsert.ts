export interface IUpsert {
  id: number;
  submitted: boolean;
  loading: boolean;
  isUpdate: boolean;
  errorMessages: string[];
  form: any;
  initializeForm(): void;

  patchFormValues(): void;

  onSubmit(): void;
}
