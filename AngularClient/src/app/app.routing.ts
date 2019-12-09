import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { P404Component } from './views/404/404.component';
import { RedirectingComponent } from './views/redirecting/redirecting.component';
import { FullLayoutComponent, SimpleLayoutComponent } from './container';
import { NotAuthenticatedGuard, AuthenticatedGuard } from './services/auth-guard.service';
import { P403Component } from './views/403/403.component';


export const routes: Routes = [
  { path: '', redirectTo: 'welcome', pathMatch: 'full' },
  { path: 'loggedin', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'welcome',
    loadChildren: () => import('./views/welcome/welcome.module').then(m => m.WelcomeModule),
    canActivate: [ NotAuthenticatedGuard ]
  },
  {
    path: '',
    component: FullLayoutComponent,
    canActivate: [ AuthenticatedGuard ],
    data: { requireScope: 'openid' },
    children: [
      {
        path: 'dashboard',
        loadChildren: () => import('./views/dashboard/dashboard.module').then(m => m.DashboardModule),
      }
    ]
  },
  {
    path: 'id_token',
    component: RedirectingComponent
  },
  {
    path: '404',
    component: P404Component
  },
  {
    path: '403',
    component: P403Component
  },
  {
    path: '**',
    redirectTo: '/404'
  }
];

@NgModule({
  imports: [ RouterModule.forRoot(routes, {useHash: true}) ],
  exports: [ RouterModule ]
})
export class AppRoutingModule {}
