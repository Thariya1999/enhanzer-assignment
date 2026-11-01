import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'purchase-bill' },
  {
    path: 'login',
    canActivate: [guestGuard],
    title: 'Login',
    loadComponent: () => import('./features/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'purchase-bill',
    canActivate: [authGuard],
    title: 'Purchase Bill',
    loadComponent: () =>
      import('./features/purchase-bill/purchase-bill.component').then((m) => m.PurchaseBillComponent),
  },
  { path: '**', redirectTo: 'purchase-bill' },
];
