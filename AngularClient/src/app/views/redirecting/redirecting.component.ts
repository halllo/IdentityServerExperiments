import { Component, AfterViewInit, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../services/auth.service';

@Component({
  templateUrl: 'redirecting.component.html'
})
export class RedirectingComponent implements AfterViewInit {

  constructor(private router: Router, private auth: AuthService) {
  }

  async ngAfterViewInit() {
    const authenticated = await this.auth.currentlyAuthenticated;
    if (!environment.production) {
      console.log('redirecting, authenticated?', authenticated);
    }

    setTimeout(() => {
      this.router.navigate(['']);
    });
  }

}
