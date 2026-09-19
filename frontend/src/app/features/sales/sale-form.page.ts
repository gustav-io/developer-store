import { CurrencyPipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { SaleInput } from '../../core/models';
import { MAX_QUANTITY, previewItem } from './discount';
import { SalesApi } from './sales-api.service';
import { describeError } from './sales.store';

@Component({
  selector: 'app-sale-form-page',
  imports: [ReactiveFormsModule, RouterLink, CurrencyPipe],
  template: `
    <a routerLink="/sales">← Back to sales</a>
    <form [formGroup]="form" (ngSubmit)="submit()">
      <div class="card">
        <h2 style="margin-top: 0;">New sale</h2>
        <div class="grid">
          <div class="field"><label for="customerName">Customer name</label><input id="customerName" formControlName="customerName" /></div>
          <div class="field"><label for="customerId">Customer id</label><input id="customerId" formControlName="customerId" /></div>
          <div class="field"><label for="branchName">Branch name</label><input id="branchName" formControlName="branchName" /></div>
          <div class="field"><label for="branchId">Branch id</label><input id="branchId" formControlName="branchId" /></div>
        </div>
      </div>

      <div class="card" formArrayName="items">
        <h3 style="margin-top: 0;">Items</h3>
        @for (item of items.controls; track $index; let i = $index) {
          <div [formGroupName]="i" class="grid" style="align-items: end; border-bottom: 1px solid #e5e7eb; padding-bottom: 8px; margin-bottom: 8px;">
            <div class="field"><label>Product name</label><input formControlName="productName" /></div>
            <div class="field"><label>Product id</label><input formControlName="productId" /></div>
            <div class="field"><label>Unit price</label><input type="number" min="0.01" step="0.01" formControlName="unitPrice" /></div>
            <div class="field"><label>Quantity (1–{{ maxQuantity }})</label><input type="number" min="1" [max]="maxQuantity" formControlName="quantity" /></div>
            <div>
              @if (previews()[i]; as p) {
                <small>{{ p.percent * 100 }}% off · {{ p.total | currency: 'BRL' }}</small>
              } @else {
                <small class="error">Quantity must be 1–{{ maxQuantity }}</small>
              }
            </div>
            <button type="button" (click)="removeItem(i)" [disabled]="items.length === 1">Remove</button>
          </div>
        }
        <button type="button" (click)="addItem()">Add item</button>
      </div>

      @if (error()) {
        <p class="error">{{ error() }}</p>
      }
      <div class="card" style="display: flex; justify-content: space-between; align-items: center;">
        <strong>Total: {{ total() | currency: 'BRL' }}</strong>
        <button class="primary" type="submit" [disabled]="form.invalid || busy()">Create sale</button>
      </div>
    </form>
  `,
})
export class SaleFormPage {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(SalesApi);
  private readonly router = inject(Router);

  readonly maxQuantity = MAX_QUANTITY;
  readonly form = this.fb.nonNullable.group({
    customerId: [crypto.randomUUID(), Validators.required],
    customerName: ['', [Validators.required, Validators.maxLength(100)]],
    branchId: [crypto.randomUUID(), Validators.required],
    branchName: ['', [Validators.required, Validators.maxLength(100)]],
    items: this.fb.array([this.buildItem()]),
  });

  private readonly value = toSignal(this.form.valueChanges, { initialValue: this.form.value });
  readonly previews = computed(() => (this.value().items ?? []).map(i => previewItem(Number(i?.unitPrice), Number(i?.quantity))));
  readonly total = computed(() => this.previews().reduce((sum, p) => sum + (p?.total ?? 0), 0));
  readonly error = signal<string | null>(null);
  readonly busy = signal(false);

  get items(): FormArray<FormGroup> {
    return this.form.controls.items as FormArray<FormGroup>;
  }

  addItem(): void {
    this.items.push(this.buildItem());
  }

  removeItem(i: number): void {
    this.items.removeAt(i);
  }

  submit(): void {
    this.busy.set(true);
    this.error.set(null);
    const input = this.form.getRawValue() as SaleInput;
    this.api.create(input).subscribe({
      next: r => this.router.navigate(['/sales', r.data.id]),
      error: e => {
        this.error.set(describeError(e));
        this.busy.set(false);
      },
    });
  }

  private buildItem(): FormGroup {
    return this.fb.nonNullable.group({
      productId: [crypto.randomUUID(), Validators.required],
      productName: ['', [Validators.required, Validators.maxLength(200)]],
      unitPrice: [10, [Validators.required, Validators.min(0.01)]],
      quantity: [1, [Validators.required, Validators.min(1), Validators.max(MAX_QUANTITY)]],
    });
  }
}
