import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  templateUrl: './full-layout.component.html',
  styleUrls: ['./full-layout.component.css']
})
export class FullLayoutComponent {

  navbarCollapsed = true;

  constructor(public auth: AuthService, private router: Router) { }

  continueToSignin() {
    this.router.navigate(['']);
  }
}
