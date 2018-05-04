import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppComponent } from './components/app';
import {BrowserAnimationsModule} from "@angular/platform-browser/animations";
import {MaterialsModule} from "../materials";
import {RouterModule} from "@angular/router";
import {HealthComponent} from "./components/health";
import {NotFoundComponent} from "./components/not-found";

@NgModule({
  declarations: [
    AppComponent,
    HealthComponent,
    NotFoundComponent,
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    MaterialsModule,
    RouterModule.forRoot(
      [
        { path: '',
          pathMatch: 'full',
          component: HealthComponent
        },
        { path: '**', component: NotFoundComponent }
      ],
      { enableTracing: true }
    )
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
