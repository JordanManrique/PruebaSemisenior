import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { CreateSampleRequest, PagedResponse, SampleDetail, SampleListItem } from '../models/sample.models';

@Injectable({ providedIn: 'root' })
export class SampleService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/samples`;

  getPage(page: number, pageSize: number): Observable<PagedResponse<SampleListItem>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResponse<SampleListItem>>(this.baseUrl, { params });
  }

  create(request: CreateSampleRequest): Observable<SampleDetail> {
    return this.http.post<SampleDetail>(this.baseUrl, request);
  }
}
