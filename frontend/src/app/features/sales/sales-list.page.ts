import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { SalesStore } from './sales.store';

@Component({
  selector: 'app-sales-list-page',
  imports: [RouterLink, CurrencyPipe, DatePipe, FormsModule],
  template: `
    <div class="card">
      <div style="display: flex; gap: 12px; flex-wrap: wrap; align-items: end;">
        <div class="field" style="flex: 1; min-width: 200px; margin: 0;">
          <label for="customer">Customer</label>
          <input id="customer" [ngModel]="store.query().customerName" (ngModelChange)="store.load({ customerName: $event, page: 1 })" placeholder="Partial name" />
        </div>
        <div class="field" style="margin: 0;">
          <label for="status">Status</label>
          <select id="status" [ngModel]="store.query().status ?? ''" (ngModelChange)="store.load({ status: $event, page: 1 })">
            <option value="">All</option>
            <option value="Active">Active</option>
            <option value="Cancelled">Cancelled</option>
          </select>
        </div>
        <a routerLink="/sales/new"><button class="primary" type="button">New sale</button></a>
      </div>
    </div>

    @if (store.error()) {
      <p class="error">{{ store.error() }}</p>
    }

    <div class="card">
      <table>
        <thead>
          <tr>
            <th class="sortable" (click)="store.sortBy('saleNumber')">#</th>
            <th class="sortable" (click)="store.sortBy('saleDate')">Date</th>
            <th class="sortable" (click)="store.sortBy('customerName')">Customer</th>
            <th class="sortable" (click)="store.sortBy('branchName')">Branch</th>
            <th class="sortable" (click)="store.sortBy('totalAmount')">Total</th>
            <th class="sortable" (click)="store.sortBy('status')">Status</th>
          </tr>
        </thead>
        <tbody>
          @for (sale of store.sales(); track sale.id) {
            <tr>
              <td data-label="#"><a [routerLink]="['/sales', sale.id]">{{ sale.saleNumber }}</a></td>
              <td data-label="Date">{{ sale.saleDate | date: 'short' }}</td>
              <td data-label="Customer">{{ sale.customerName }}</td>
              <td data-label="Branch">{{ sale.branchName }}</td>
              <td data-label="Total">{{ sale.totalAmount | currency: 'BRL' }}</td>
              <td data-label="Status"><span class="badge {{ sale.status }}">{{ sale.status }}</span></td>
            </tr>
          } @empty {
            <tr><td colspan="6">{{ store.loading() ? 'Loading…' : 'No sales found' }}</td></tr>
          }
        </tbody>
      </table>
      <div style="display: flex; justify-content: space-between; align-items: center; margin-top: 12px;">
        <span>{{ store.totalCount() }} sales · page {{ store.query().page }} of {{ store.totalPages() }}</span>
        <span>
          <button type="button" (click)="store.previousPage()" [disabled]="!store.hasPrevious()">Previous</button>
          <button type="button" (click)="store.nextPage()" [disabled]="!store.hasNext()">Next</button>
        </span>
      </div>
    </div>
  `,
})
export class SalesListPage {
  protected readonly store = inject(SalesStore);

  constructor() {
    this.store.load();
  }
}
