import { FormGroup } from '@angular/forms';

export interface IUpsert {
  id: number;
  submitted: boolean;
  loading: boolean;
  errorMessages: string[];
  form: FormGroup;

  initializeForm(): void;

  patchFormValues(): void;

  onSubmit(): void;
}
