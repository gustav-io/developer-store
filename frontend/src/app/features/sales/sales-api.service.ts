import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiResponse, ListSalesQuery, PaginatedResponse, Sale, SaleInput } from '../../core/models';

@Injectable({ providedIn: 'root' })
export class SalesApi {
  private readonly http = inject(HttpClient);
  private readonly base = '/api/sales';

  list(query: ListSalesQuery): Observable<PaginatedResponse<Sale>> {
    let params = new HttpParams().set('_page', query.page).set('_size', query.size);
    if (query.order) params = params.set('_order', query.order);
    if (query.customerName) params = params.set('customerName', `*${query.customerName}*`);
    if (query.status) params = params.set('status', query.status);
    return this.http.get<PaginatedResponse<Sale>>(this.base, { params });
  }

  get(id: string): Observable<ApiResponse<Sale>> {
    return this.http.get<ApiResponse<Sale>>(`${this.base}/${id}`);
  }

  create(input: SaleInput): Observable<ApiResponse<Sale>> {
    return this.http.post<ApiResponse<Sale>>(this.base, input);
  }

  cancel(id: string): Observable<ApiResponse<Sale>> {
    return this.http.patch<ApiResponse<Sale>>(`${this.base}/${id}/cancel`, null);
  }

  cancelItem(id: string, itemId: string): Observable<ApiResponse<Sale>> {
    return this.http.patch<ApiResponse<Sale>>(`${this.base}/${id}/items/${itemId}/cancel`, null);
  }
}
