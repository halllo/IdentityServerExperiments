import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { P404Component } from './views/404/404.component';
import { RedirectingComponent } from './views/redirecting/redirecting.component';
import { FullLayoutComponent, SimpleLayoutComponent } from './container';
import { NotAuthenticatedGuard, AuthenticatedGuard } from './services/auth-guard.service';


export const routes: Routes = [
  { path: '', redirectTo: 'welcome', pathMatch: 'full' },
  { path: 'loggedin', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'welcome',
    loadChildren: './views/welcome/welcome.module#WelcomeModule',
    canActivate: [ NotAuthenticatedGuard ]
  },
  {
    path: '',
    component: FullLayoutComponent,
    canActivate: [ AuthenticatedGuard ],
    children: [
      {
        path: 'dashboard',
        loadChildren: './views/dashboard/dashboard.module#DashboardModule',
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
    path: '**',
    redirectTo: '/404'
  }
];

@NgModule({
  imports: [ RouterModule.forRoot(routes, {useHash: true}) ],
  exports: [ RouterModule ]
})
export class AppRoutingModule {}
