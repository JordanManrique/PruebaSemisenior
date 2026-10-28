import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/** Marca el control con { futureDate: true } si la fecha es posterior al momento actual. */
export function notFutureDateValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null;
    }
    const date = new Date(control.value);
    if (isNaN(date.getTime())) {
      return { invalidDate: true };
    }
    return date.getTime() > Date.now() ? { futureDate: true } : null;
  };
}
