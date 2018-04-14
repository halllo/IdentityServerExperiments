import { Injectable } from '@angular/core';
import { fromPromise } from 'rxjs/observable/fromPromise';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class AuthService {

  constructor() { }

  public initAndHandleRedirects() {
    const authority = `https://login.microsoftonline.com/tfp/${environment.msalConfig.tenant}/${environment.msalConfig.signUpSignInPolicy}`;
    this.clientApplication = new UserAgentApplication(environment.msalConfig.clientId, authority,
      function (errorDesc, token, error, tokenType) {
        if (error) {
          console.error(error);
        } else {
          console.warn(`got new ${tokenType} token: ${token}`);
        }
      },
      {
        navigateToLoginRequestUrl: false,
        redirectUri: environment.msalConfig.redirectUri,
      }
    );

    if (this.authenticated) {
      console.warn(`User: ${this.idToken['extension_Nickname']} (${this.idToken['sub']})`);
    }
  }

  public login() {
    this.clientApplication.loginRedirect(environment.msalConfig.scopes);
  }

  public logout() {
    this.clientApplication.logout();
  }

  public get authenticated(): boolean {
    return this.clientApplication.getUser() != null;
  }

  public get username(): string {
    return this.idToken['extension_Nickname'];
  }

  public get idToken(): Object {
    return this.clientApplication.getUser().idToken;
  }

  public getAccessToken(): Observable<string> {
    const that = this;
    const acquireTokenPromise = this.clientApplication.acquireTokenSilent(environment.msalConfig.scopes);
    return fromPromise(acquireTokenPromise);
  }

}
