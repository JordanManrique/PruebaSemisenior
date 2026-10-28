import { Component, OnInit, PLATFORM_ID, inject, signal } from '@angular/core';
import { DatePipe, isPlatformBrowser } from '@angular/common';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';

import { SampleService } from '../../../core/services/sample.service';
import { SampleListItem } from '../../../core/models/sample.models';

@Component({
  selector: 'app-samples',
  standalone: true,
  imports: [DatePipe, RouterLink, MatTableModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatCardModule],
  templateUrl: './samples.component.html',
  styleUrl: './samples.component.css'
})
export class SamplesComponent implements OnInit {
  private readonly sampleService = inject(SampleService);
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  readonly displayedColumns = ['documentNumber', 'patientName', 'sampleType', 'collectedAt', 'status', 'testCount'];
  readonly pageSize = 10;

  readonly samples = signal<SampleListItem[]>([]);
  readonly page = signal(1);
  readonly totalPages = signal(0);
  readonly totalCount = signal(0);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
   
    if (this.isBrowser) {
      this.load(1);
    }
  }

  previous(): void {
    if (this.page() > 1) {
      this.load(this.page() - 1);
    }
  }

  next(): void {
    if (this.page() < this.totalPages()) {
      this.load(this.page() + 1);
    }
  }

  load(page: number): void {
    this.loading.set(true);
    this.error.set(null);
    this.sampleService.getPage(page, this.pageSize)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: res => {
          this.samples.set(res.items);
          this.page.set(res.page);
          this.totalPages.set(res.totalPages);
          this.totalCount.set(res.totalCount);
        },
        error: () => this.error.set('No se pudieron cargar las muestras. Verifique que la API esté disponible.')
      });
  }
}
