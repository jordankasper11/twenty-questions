import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { AuthenticationService } from '@services';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styles: []
})
export class AppComponent implements OnInit {
    url: string;
    menuExpanded: boolean = false;

    get signedIn(): boolean {
        return this.authenticationService.isLoggedIn();
    }

    constructor(private authenticationService: AuthenticationService, private router: Router) {
    }

    ngOnInit(): void {
        this.router.events.subscribe(event => {
            if (event instanceof NavigationEnd)
                this.url = event.url;
        });
    }

    toggleMenu(): void {
        this.menuExpanded = !this.menuExpanded;
    }

    navigate(routerLink: string): void {
        this.menuExpanded = false;

        this.router.navigate([routerLink]);
    }

    async signOut(): Promise<void> {
        await this.authenticationService.logout();

        this.navigate('/login');
    }
}
