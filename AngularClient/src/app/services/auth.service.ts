import { Injectable } from '@angular/core';
import { UserManager, User, UserManagerSettings, WebStorageStateStore, Log } from 'oidc-client';
import { Observable, BehaviorSubject, Subject } from 'rxjs';
import { map, mergeMap, filter, scan, tap, take } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { AppConfigService } from './app-config.service';

@Injectable()
export class AuthService {
  private mgr: UserManager;
  private currentUser: User;

  private loggedIn = new BehaviorSubject<boolean>(undefined);

  constructor(private runtimeConfig: AppConfigService) {
    if (!environment.production) {
      this.loggedIn.subscribe(n => console.info('loggedIn?', n));
    }
  }

  public initAndHandleRedirects() {
    const settings: UserManagerSettings = {
      authority: environment.oidc.authority,
      client_id: environment.oidc.clientId,
      redirect_uri: this.runtimeConfig.basePath,
      post_logout_redirect_uri: this.runtimeConfig.basePath,

      response_type: 'id_token token',
      scope: environment.oidc.scope,

      silent_redirect_uri: this.runtimeConfig.basePath + 'assets/pages/silent-token-refresh.html',
      automaticSilentRenew: true,
      // silentRequestTimeout: 10000,
      filterProtocolClaims: true,
      loadUserInfo: true,
      monitorSession: true,
      checkSessionInterval: 2000,
      userStore: new WebStorageStateStore({ store: window.localStorage })
    };

    Log.logger = console;
    this.mgr = new UserManager(settings);

    if (
      window.location.hash.indexOf('id_token') > -1 ||
      window.location.hash.indexOf('access_token') > -1
    ) {
      this.completeLogin();
    } else {
      this.mgr
        .getUser()
        .then(user => {
          if (user) {
            if (user.expires_in > 0) {
              this.currentUser = user;
              this.loggedIn.next(true);
            } else {
              console.info('Access token expired. Trying to silenty acquire new access token...');
              this.renewToken();
            }
          } else {
            this.loggedIn.next(false);
          }
        })
        .catch(err => {
          this.loggedIn.next(false);
        });
    }

    this.mgr.events.addUserLoaded(user => {
      this.currentUser = user;
      this.loggedIn.next(true);
      if (!environment.production) {
        console.log('authService: user loaded', user);
      }
    });

    this.mgr.events.addUserUnloaded(e => {
      if (!environment.production) {
        console.log('authService: user unloaded', e);
      }
      this.loggedIn.next(false);
    });

    this.mgr.events.addSilentRenewError(e => {
      if (!environment.production) {
        console.log('authService: silent renew error', e);
      }
    });

    this.mgr.events.addUserSessionChanged(e => {
      if (!environment.production) {
        console.log('authService: user session changed', e);
      }
    });

    this.mgr.events.addUserSignedOut(e => {
      if (!environment.production) {
        console.log('authService: user signed out', e);
      }
      this.mgr.removeUser();
    });
  }

  public login() {
    this.mgr
      .signinRedirect({ data: 'some data' })
      .then(function() {
        if (!environment.production) {
          console.log('authService: signinRedirect done');
        }
      })
      .catch(function(err) {
        console.log(err);
      });
  }
  private completeLogin() {
    this.mgr
      .signinRedirectCallback()
      .then(function(user) {
        if (!environment.production) {
          console.log('authService: signed in', user);
        }
      })
      .catch(function(err) {
        console.log(err);
      });
  }

  public renewToken() {
    if (!environment.production) {
      console.log('authService: signinSilent...');
    }
    this.mgr.signinSilent()
      .then(function() {
        if (!environment.production) {
          console.log('authService: signinSilent done');
        }
      })
      .catch(function(err) {
        console.log('Silent token refresh failed.', err);
      });
  }

  public logout() {
    this.mgr
      .signoutRedirect()
      .then(function(resp) {
        if (!environment.production) {
          console.log('authService: signed out', resp);
        }
      })
      .catch(function(err) {
        console.log(err);
      });
  }

  public get currentlyAuthenticated(): Promise<boolean> {
    return this.loggedIn.pipe(
      filter(l => l !== undefined),
      take(1)
    ).toPromise();
  }

  public get authenticated(): Observable<boolean> {
    return this.loggedIn.pipe(
      filter(l => l !== undefined)
    );
  }

  public get username(): string {
    return this.currentUser.profile.given_name || this.currentUser.profile.name;
  }

  public get subject(): string {
    return this.currentUser.profile.sub;
  }

  public get idToken(): string {
    return this.currentUser.id_token;
  }

  public get accessToken(): string {
    return this.currentUser.access_token;
  }

  public get scopes(): string[] {
    return this.currentUser.scopes;
  }

  public hasScope(scope: string): boolean {
    return !scope || this.scopes.includes(scope);
  }
}
