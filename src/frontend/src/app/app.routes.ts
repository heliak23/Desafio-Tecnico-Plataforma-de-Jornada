import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login';
import { PontoComponent } from './pages/ponto/ponto';
import { EspelhoComponent } from './pages/espelho/espelho';
import { AjustesComponent } from './pages/ajustes/ajustes';
import { GestaoComponent } from './pages/gestao/gestao';
import { authGuard, managerOrHrGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'ponto', component: PontoComponent, canActivate: [authGuard] },
  { path: 'espelho', component: EspelhoComponent, canActivate: [authGuard] },
  { path: 'ajustes', component: AjustesComponent, canActivate: [authGuard] },
  { path: 'gestao', component: GestaoComponent, canActivate: [authGuard, managerOrHrGuard] },
  { path: '', redirectTo: 'ponto', pathMatch: 'full' },
  { path: '**', redirectTo: 'ponto' }
];
