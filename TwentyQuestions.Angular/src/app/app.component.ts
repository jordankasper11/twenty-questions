import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styles: []
})
export class AppComponent {
    menuExpanded: boolean = false;

    toggleMenu() :void{
        this.menuExpanded = !this.menuExpanded;
    }
}
