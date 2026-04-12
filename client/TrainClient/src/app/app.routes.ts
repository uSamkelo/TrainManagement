import { Routes } from '@angular/router';
import { authGuard, roleGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'dashboard', loadComponent: () => import('./train-position/train-position.component').then(m => m.TrainPositionComponent) },
  { path: 'login', loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./pages/register/register.component').then(m => m.RegisterComponent) },
  { path: 'tickets', canActivate: [authGuard], loadComponent: () => import('./pages/tickets/tickets.component').then(m => m.TicketsComponent) },
  { path: 'my-tickets', canActivate: [authGuard], loadComponent: () => import('./pages/my-tickets/my-tickets.component').then(m => m.MyTicketsComponent) },
  { path: 'admin/users', canActivate: [roleGuard('Admin', 'StationStaff')], loadComponent: () => import('./pages/admin-users/admin-users.component').then(m => m.AdminUsersComponent) },
  { path: '**', redirectTo: '/dashboard' }
];
