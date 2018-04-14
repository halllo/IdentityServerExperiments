import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot } from '@angular/router';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';

@Injectable()
export class AuthenticatedGuard implements CanActivate {
  constructor(private router: Router, private auth: AuthService) { }

  canActivate(route: ActivatedRouteSnapshot) {
    return this.auth.authenticated;
  }
}

@Injectable()
export class NotAuthenticatedGuard implements CanActivate {
  constructor(private router: Router, private auth: AuthService) { }

  canActivate(route: ActivatedRouteSnapshot) {
    if (this.auth.authenticated) {
      this.router.navigate(['dashboard']);
      return false;
    } else {
      return true;
    }
  }
}
