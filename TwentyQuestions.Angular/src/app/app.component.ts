import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthenticationService } from '@services';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styles: []
})
export class AppComponent {
    menuExpanded: boolean = false;

    get signedIn(): boolean {
        return this.authenticationService.isLoggedIn();
    }

    constructor(private authenticationService: AuthenticationService, private router: Router) {
    }

    toggleMenu(): void {
        this.menuExpanded = !this.menuExpanded;
    }

    navigate(routerLink:string):void{
        this.menuExpanded = false;

        this.router.navigate([routerLink]);
    }

    async signOut(): Promise<void> {
        await this.authenticationService.logout();

        this.navigate('/login');
    }
}
