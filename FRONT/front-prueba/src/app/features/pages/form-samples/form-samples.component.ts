import { Component, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { SampleService } from '../../../core/services/sample.service';
import { ErrorResponse, SAMPLE_TYPES } from '../../../core/models/sample.models';
import { notFutureDateValidator } from '../../../shared/validators/not-future-date.validator';
import { uniqueValuesValidator } from '../../../shared/validators/unique-values.validator';

@Component({
  selector: 'app-form-samples',
  standalone: true,
  imports: [
    ReactiveFormsModule, RouterLink,
    MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, MatIconModule,
    MatCardModule, MatProgressSpinnerModule, MatSnackBarModule
  ],
  templateUrl: './form-samples.component.html',
  styleUrl: './form-samples.component.css'
})
export class FormSamplesComponent {
  private readonly fb = inject(FormBuilder);
  private readonly sampleService = inject(SampleService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  readonly sampleTypes = SAMPLE_TYPES;
  readonly maxDate = toLocalDateTimeInput(new Date());
  readonly saving = signal(false);
  readonly serverErrors = signal<string[]>([]);

  readonly form = this.fb.nonNullable.group({
    documentNumber: ['', [Validators.required, Validators.pattern(/^\d+$/), Validators.minLength(6), Validators.maxLength(12)]],
    fullName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(150)]],
    sampleType: ['', Validators.required],
    collectedAt: ['', [Validators.required, notFutureDateValidator()]],
    testCodes: this.fb.array<FormControl<string>>([this.createTestCode()], [Validators.minLength(1), uniqueValuesValidator()])
  });

  get testCodes(): FormArray<FormControl<string>> {
    return this.form.controls.testCodes;
  }

  addTestCode(): void {
    this.testCodes.push(this.createTestCode());
  }

  removeTestCode(index: number): void {
    // Se exige mínimo un código: nunca se elimina la última fila.
    if (this.testCodes.length > 1) {
      this.testCodes.removeAt(index);
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.saving.set(true);
    this.serverErrors.set([]);

    this.sampleService.create({
      documentNumber: value.documentNumber,
      fullName: value.fullName.trim(),
      sampleType: value.sampleType,
      collectedAt: value.collectedAt,
      testCodes: value.testCodes.map(c => c.trim().toUpperCase())
    })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          this.snackBar.open('Muestra registrada correctamente.', 'Cerrar', { duration: 3000 });
          this.router.navigate(['/samples']);
        },
        error: (err: HttpErrorResponse) => this.serverErrors.set(toErrorMessages(err))
      });
  }

  private createTestCode(): FormControl<string> {
    return this.fb.nonNullable.control('', Validators.required);
  }
}

/** Formato aceptado por <input type="datetime-local">: yyyy-MM-ddTHH:mm en hora local. */
function toLocalDateTimeInput(date: Date): string {
  const pad = (n: number) => n.toString().padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

function toErrorMessages(err: HttpErrorResponse): string[] {
  const body = err.error as ErrorResponse | null;
  if (body?.errors) {
    return Object.values(body.errors).flat();
  }
  if (body?.message) {
    return [body.message];
  }
  return ['No se pudo registrar la muestra. Verifique que la API esté disponible.'];
}
