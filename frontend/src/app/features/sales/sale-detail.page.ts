import { CurrencyPipe, DatePipe, PercentPipe } from '@angular/common';
import { Component, OnInit, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { ApiResponse, Sale } from '../../core/models';
import { SalesApi } from './sales-api.service';
import { describeError } from './sales.store';

@Component({
  selector: 'app-sale-detail-page',
  imports: [RouterLink, CurrencyPipe, DatePipe, PercentPipe],
  template: `
    <a routerLink="/sales">← Back to sales</a>
    @if (error()) {
      <p class="error">{{ error() }}</p>
    }
    @if (sale(); as s) {
      <div class="card">
        <div style="display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 8px;">
          <h2 style="margin: 0;">Sale #{{ s.saleNumber }} <span class="badge {{ s.status }}">{{ s.status }}</span></h2>
          <button class="danger" type="button" (click)="cancelSale()" [disabled]="s.status === 'Cancelled' || busy()">Cancel sale</button>
        </div>
        <div class="grid" style="margin-top: 12px;">
          <div><strong>Date</strong><br />{{ s.saleDate | date: 'medium' }}</div>
          <div><strong>Customer</strong><br />{{ s.customerName }}</div>
          <div><strong>Branch</strong><br />{{ s.branchName }}</div>
          <div><strong>Total</strong><br />{{ s.totalAmount | currency: 'BRL' }}</div>
        </div>
      </div>
      <div class="card">
        <table>
          <thead>
            <tr><th>Product</th><th>Unit price</th><th>Qty</th><th>Discount</th><th>Total</th><th></th></tr>
          </thead>
          <tbody>
            @for (item of s.items; track item.id) {
              <tr [style.opacity]="item.isCancelled ? 0.5 : 1">
                <td data-label="Product">{{ item.productName }}</td>
                <td data-label="Unit price">{{ item.unitPrice | currency: 'BRL' }}</td>
                <td data-label="Qty">{{ item.quantity }}</td>
                <td data-label="Discount">{{ item.discountPercent | percent }} ({{ item.discountAmount | currency: 'BRL' }})</td>
                <td data-label="Total">{{ item.totalAmount | currency: 'BRL' }}</td>
                <td>
                  @if (item.isCancelled) {
                    <span class="badge Cancelled">Cancelled</span>
                  } @else {
                    <button class="danger" type="button" (click)="cancelItem(item.id)" [disabled]="s.status === 'Cancelled' || busy()">Cancel item</button>
                  }
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    }
  `,
})
export class SaleDetailPage implements OnInit {
  readonly id = input.required<string>();
  private readonly api = inject(SalesApi);

  readonly sale = signal<Sale | null>(null);
  readonly error = signal<string | null>(null);
  readonly busy = signal(false);

  ngOnInit(): void {
    this.api.get(this.id()).subscribe({
      next: r => this.sale.set(r.data),
      error: e => this.error.set(describeError(e)),
    });
  }

  cancelSale(): void {
    if (!confirm('Cancel this sale?')) return;
    this.run(this.api.cancel(this.id()));
  }

  cancelItem(itemId: string): void {
    this.run(this.api.cancelItem(this.id(), itemId));
  }

  private run(call: Observable<ApiResponse<Sale>>): void {
    this.busy.set(true);
    this.error.set(null);
    call.subscribe({
      next: r => {
        this.sale.set(r.data);
        this.busy.set(false);
      },
      error: e => {
        this.error.set(describeError(e));
        this.busy.set(false);
      },
    });
  }
}
