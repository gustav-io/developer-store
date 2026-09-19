import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { SalesStore } from './sales.store';
import { Sale } from '../../core/models';

const sale = (overrides: Partial<Sale> = {}): Sale => ({
  id: 'a', saleNumber: 1000, saleDate: '2026-09-20T00:00:00Z', customerId: 'c', customerName: 'Maria',
  branchId: 'b', branchName: 'Centro', status: 'Active', totalAmount: 45, createdAt: '', updatedAt: null, items: [],
  ...overrides,
});

const emptyPage = { success: true, message: '', data: [], currentPage: 1, totalPages: 0, totalCount: 0 };

describe('SalesStore', () => {
  let store: SalesStore;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    store = TestBed.inject(SalesStore);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('loads a page and exposes paging state', () => {
    store.load({ page: 2, size: 5, order: 'totalAmount desc' });

    const req = http.expectOne(r => r.url === '/api/sales');
    expect(req.request.params.get('_page')).toBe('2');
    expect(req.request.params.get('_order')).toBe('totalAmount desc');
    req.flush({ success: true, message: '', data: [sale()], currentPage: 2, totalPages: 3, totalCount: 11 });

    expect(store.sales().length).toBe(1);
    expect(store.totalPages()).toBe(3);
    expect(store.totalCount()).toBe(11);
    expect(store.loading()).toBeFalse();
  });

  it('surfaces the API error detail', () => {
    store.load({ page: 1, size: 10, order: 'password' });

    http.expectOne(r => r.url === '/api/sales')
      .flush({ type: 'ValidationError', error: 'Invalid input data', detail: 'Cannot order by password' }, { status: 400, statusText: 'Bad Request' });

    expect(store.error()).toBe('Cannot order by password');
    expect(store.loading()).toBeFalse();
  });

  it('toggles sort direction on the same field', () => {
    store.sortBy('saleDate');
    http.expectOne(r => r.url === '/api/sales').flush(emptyPage);
    expect(store.query().order).toBe('saleDate asc');

    store.sortBy('saleDate');
    http.expectOne(r => r.url === '/api/sales').flush(emptyPage);
    expect(store.query().order).toBe('saleDate desc');
  });
});
