import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./features/auth/login.page').then(m => m.LoginPage) },
  {
    path: 'sales',
    canActivate: [authGuard],
    children: [
      { path: '', loadComponent: () => import('./features/sales/sales-list.page').then(m => m.SalesListPage) },
      { path: 'new', loadComponent: () => import('./features/sales/sale-form.page').then(m => m.SaleFormPage) },
      { path: ':id', loadComponent: () => import('./features/sales/sale-detail.page').then(m => m.SaleDetailPage) },
    ],
  },
  { path: '', pathMatch: 'full', redirectTo: 'sales' },
  { path: '**', redirectTo: 'sales' },
];
