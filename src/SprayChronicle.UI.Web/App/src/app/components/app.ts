import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  template: `
    <section class="application">
      <nav>
        <mat-toolbar color="primary">
          <a mat-button routerLink="/" routerLinkActive="active">
            <h2>SprayChronicle</h2>
          </a>
          <a mat-button routerLink="/processes" routerLinkActive="active" >
            Processes
          </a>
        </mat-toolbar>
      </nav>
      <router-outlet></router-outlet>
    </section>
  `
})
export class AppComponent {

}
