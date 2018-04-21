import { Component } from '@angular/core';
import { map, mergeMap, filter, scan } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';
import { error } from 'protractor';
import { Api } from '../../services/api.service';

@Component({
  templateUrl: 'dashboard.component.html',
  styleUrls: [ 'dashboard.component.css' ]
})
export class DashboardComponent {

  public api_result: any;

  constructor(private auth: AuthService, private api: Api) { }

  public getIdToken() {
    return this.auth.idToken;
  }

  public getAccessToken() {
    return this.auth.accessToken;
  }

  public callApi() {
    this.api.get().subscribe(
      result => {
        this.api_result = result;
      },
      err => {
        this.api_result = err;
      }
    );
  }

}
