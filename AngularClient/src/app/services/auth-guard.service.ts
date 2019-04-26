import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot } from '@angular/router';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

@Injectable()
export class AuthenticatedGuard implements CanActivate {
  constructor(private router: Router, private auth: AuthService) { }

  async canActivate(route: ActivatedRouteSnapshot) : Promise<boolean> {
    const loggedIn = await this.auth.currentlyAuthenticated;
    if (loggedIn) {
      const accessScope: string = route.data.requireScope;
      if (this.auth.hasScope(accessScope)) {
        if (!environment.production) {
          console.info(`logged in. can activate '${route.routeConfig.path}'`);
        }
        return true;
      } else {
        if (!environment.production) {
          console.info(`logged in but no '${accessScope}' scope! redirect to '403'`);
        }
        this.router.navigate(['403']);
        return false;
      }
    } else {
      if (!environment.production) {
        console.info(`not logged in but '${route.routeConfig.path}' can only activate when logged in! redirect to 'welcome'`);
      }
      this.router.navigate(['welcome']);
      return false;
    }
  }
}

@Injectable()
export class NotAuthenticatedGuard implements CanActivate {
  constructor(private router: Router, private auth: AuthService) { }

  async canActivate(route: ActivatedRouteSnapshot): Promise<boolean> {
    const loggedIn = await this.auth.currentlyAuthenticated;
    if (!loggedIn) {
      if (!environment.production) {
        console.info(`not logged in. can activate '${route.routeConfig.path}'`);
      }
      return true;
    } else {
      if (!environment.production) {
        console.info(`logged in but '${route.routeConfig.path}' can only activate when not logged in! redirect to 'loggedin'`);
      }
      this.router.navigate(['loggedin']);
      return false;
    }
  }
}
