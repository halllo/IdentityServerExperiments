import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { NgModule, APP_INITIALIZER } from '@angular/core';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { AppRoutingModule } from './app.routing';
import { AppComponent } from './app.component';
import { SimpleLayoutComponent, FullLayoutComponent } from './container';
import { P404Component } from './views/404/404.component';
import { RedirectingComponent } from './views/redirecting/redirecting.component';
import { AppConfigService, appInitializerFn } from './services/app-config.service';
import { AuthService } from './services/auth.service';
import { AuthenticatedGuard, NotAuthenticatedGuard } from './services/auth-guard.service';
import { Api } from './services/api.service';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { P403Component } from './views/403/403.component';


@NgModule({
  imports: [
    CommonModule,
    BrowserModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule,
    AppRoutingModule,
    NgbModule,
  ],
  declarations: [
    AppComponent,
    SimpleLayoutComponent,
    FullLayoutComponent,
    P404Component,
    P403Component,
    RedirectingComponent,
  ],
  providers: [
    AppConfigService,
    {
      provide: APP_INITIALIZER,
      useFactory: appInitializerFn,
      multi: true,
      deps: [AppConfigService]
    },
    AuthService,
    AuthenticatedGuard,
    NotAuthenticatedGuard,
    Api
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
