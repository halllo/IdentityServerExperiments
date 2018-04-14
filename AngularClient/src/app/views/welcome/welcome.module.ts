import { NgModule } from '@angular/core';

import { WelcomeComponent } from './welcome.component';
import { WelcomeRoutingModule } from './welcome-routing.module';
import { CommonModule } from '@angular/common';

@NgModule({
  imports: [
    WelcomeRoutingModule,
    CommonModule
  ],
  declarations: [ WelcomeComponent ]
})
export class WelcomeModule { }
