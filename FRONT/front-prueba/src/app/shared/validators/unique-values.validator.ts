import { AbstractControl, FormArray, ValidationErrors, ValidatorFn } from '@angular/forms';

/** Marca el FormArray con { duplicated: true } si tiene valores repetidos (sin distinguir mayúsculas). */
export function uniqueValuesValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const values = (control as FormArray).controls
      .map(c => String(c.value ?? '').trim().toUpperCase())
      .filter(v => v.length > 0);
    return new Set(values).size !== values.length ? { duplicated: true } : null;
  };
}
