import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Api } from '../../services/api.service';

@Component({
  templateUrl: 'dashboard.component.html',
  styleUrls: [ 'dashboard.component.css' ]
})
export class DashboardComponent {

  public api_get_result: any;
  public api_post_result: any;

  constructor(private auth: AuthService, private api: Api) { }

  public getIdToken() {
    return this.auth.idToken;
  }

  public getAccessToken() {
    return this.auth.accessToken;
  }

  public async invokeApiGet() {
    try {
      const result = await this.api.get().toPromise();
      this.api_get_result = result;
    } catch (err) {
      console.error(err);
      this.api_get_result = err;
      alert(err);
    }
  }
  
  public async invokeApiPost() {
    try {
      const result = await this.api.post().toPromise();
      this.api_post_result = result;
    } catch (err) {
      console.error(err);
      this.api_post_result = err;
      alert(err);
    }
  }

}
