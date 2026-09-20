import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { ApiError, ListSalesQuery, Sale } from '../../core/models';
import { SalesApi } from './sales-api.service';

/**
 * Signal-based state for the sales list. Small enough that a store service beats NgRx here.
 */
@Injectable({ providedIn: 'root' })
export class SalesStore {
  private readonly api = inject(SalesApi);

  private readonly salesSignal = signal<Sale[]>([]);
  private readonly querySignal = signal<ListSalesQuery>({ page: 1, size: 10, order: 'saleNumber desc' });
  private readonly totalPagesSignal = signal(0);
  private readonly totalCountSignal = signal(0);
  private readonly loadingSignal = signal(false);
  private readonly errorSignal = signal<string | null>(null);

  readonly sales = this.salesSignal.asReadonly();
  readonly query = this.querySignal.asReadonly();
  readonly totalPages = this.totalPagesSignal.asReadonly();
  readonly totalCount = this.totalCountSignal.asReadonly();
  readonly loading = this.loadingSignal.asReadonly();
  readonly error = this.errorSignal.asReadonly();
  readonly hasPrevious = computed(() => this.querySignal().page > 1);
  readonly hasNext = computed(() => this.querySignal().page < this.totalPagesSignal());

  load(patch: Partial<ListSalesQuery> = {}): void {
    const query = { ...this.querySignal(), ...patch };
    this.querySignal.set(query);
    this.loadingSignal.set(true);
    this.errorSignal.set(null);

    this.api.list(query).subscribe({
      next: page => {
        this.salesSignal.set(page.data);
        this.totalPagesSignal.set(page.totalPages);
        this.totalCountSignal.set(page.totalCount);
        this.loadingSignal.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.errorSignal.set(describeError(err));
        this.loadingSignal.set(false);
      },
    });
  }

  sortBy(field: string): void {
    const [current, direction] = (this.querySignal().order ?? '').split(' ');
    const next = current === field && direction === 'asc' ? 'desc' : 'asc';
    this.load({ order: `${field} ${next}`, page: 1 });
  }

  nextPage(): void {
    if (this.hasNext()) this.load({ page: this.querySignal().page + 1 });
  }

  previousPage(): void {
    if (this.hasPrevious()) this.load({ page: this.querySignal().page - 1 });
  }
}

export function describeError(err: HttpErrorResponse): string {
  const body = err.error as Partial<ApiError> | undefined;
  return body?.detail || body?.error || err.message || 'Unexpected error';
}
