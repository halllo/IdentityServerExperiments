import { Injectable, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { map, mergeMap, filter, scan } from 'rxjs/operators';
import { fromPromise } from 'rxjs/observable/fromPromise';
import { Observable } from 'rxjs/Observable';
import { UserManager, Log, MetadataService, User, UserManagerSettings } from 'oidc-client';
import { environment } from '../../environments/environment';

const settings: UserManagerSettings = {
  authority: environment.oidc.authority,
  client_id: environment.oidc.clientId,
  redirect_uri: environment.oidc.redirectUri,
  post_logout_redirect_uri: environment.oidc.redirectUri,
  response_type: 'id_token token',
  scope: 'openid profile api',

  silent_redirect_uri: environment.oidc.redirectUri + 'silent-renew.html',
  automaticSilentRenew: true,
  // silentRequestTimeout:10000,
  filterProtocolClaims: true,
  loadUserInfo: true
};

@Injectable()
export class AuthService {

  private mgr: UserManager;;
  private currentUser: User;
  private loggedIn = false;

  constructor(private router: Router) { }

  public initAndHandleRedirects() {
    this.mgr = new UserManager(settings);

    if (window.location.hash.indexOf('id_token') > -1 || window.location.hash.indexOf('access_token') > -1) {
      this.completeLogin();
    } else {
      this.mgr.getUser()
        .then((user) => {
          if (user) {
            this.loggedIn = true;
            this.currentUser = user;
          } else {
            this.loggedIn = false;
          }
        })
        .catch((err) => {
          this.loggedIn = false;
        }
      );
    }

    this.mgr.events.addUserLoaded(user => {
      this.currentUser = user;
      this.loggedIn = true;
      if (!environment.production) {
        console.log('authService addUserLoaded', user);
      }
    });

    this.mgr.events.addUserUnloaded((e) => {
      if (!environment.production) {
        console.log('user unloaded');
      }
      this.loggedIn = false;
    });
  }

  public login() {
    this.mgr.signinRedirect({ data: 'some data' }).then(function () {
      console.log('signinRedirect done');
    }).catch(function (err) {
      console.log(err);
    });
  }
  private completeLogin() {
    const that = this;
    this.mgr.signinRedirectCallback().then(function (user) {
      console.log('signed in', user);
      that.router.navigate(['']);
    }).catch(function (err) {
      console.log(err);
    });
  }

  public logout() {
    this.mgr.signoutRedirect().then(function (resp) {
      console.log('signed out', resp);
      setTimeout(5000, () => {
        console.log('testing to see if fired...');

      });
    }).catch(function (err) {
      console.log(err);
    });
  }

  public get authenticated(): boolean {
    return this.loggedIn;
  }

  public get username(): string {
    return this.currentUser.profile.name;
  }

  public get idToken(): string {
    return this.currentUser.id_token;
  }

  public get accessToken(): string {
    return this.currentUser.access_token;
  }

}
