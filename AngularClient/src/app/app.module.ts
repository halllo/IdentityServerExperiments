import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { AppRoutingModule } from './app.routing';
import { AppComponent } from './app.component';
import { SimpleLayoutComponent, FullLayoutComponent } from './container';
import { P404Component } from './views/404/404.component';
import { AuthService } from './services/auth.service';
import { AuthenticatedGuard, NotAuthenticatedGuard } from './services/auth-guard.service';
import { Api } from './services/api.service';


@NgModule({
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule,
    NgbModule.forRoot(),
  ],
  declarations: [
    AppComponent,
    SimpleLayoutComponent,
    FullLayoutComponent,
    P404Component,
  ],
  providers: [
    AuthService,
    AuthenticatedGuard,
    NotAuthenticatedGuard,
    Api
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
