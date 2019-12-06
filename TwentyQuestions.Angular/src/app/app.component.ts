import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { AuthenticationService, NotificationsService } from '@services';
import { NotificationProvider } from '@providers';
import { NotificationsEntity } from '@models';
import { Subscription } from 'rxjs';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styles: []
})
export class AppComponent implements OnInit, OnDestroy {
    url: string;
    menuExpanded: boolean = false;
    notifications: NotificationsEntity;

    private notificationsSubscription: Subscription;

    get signedIn(): boolean {
        return this.authenticationService.isLoggedIn();
    }

    constructor(
        private authenticationService: AuthenticationService,
        private notificationProvider: NotificationProvider,
        private router: Router
    ) {
    }

    async ngOnInit(): Promise<void> {
        this.router.events.subscribe(event => {
            if (event instanceof NavigationEnd)
                this.url = event.url;
        });

        this.notificationsSubscription = this.notificationProvider.notificationsUpdated.subscribe(notifications => this.notifications = notifications);
        this.notifications = await this.notificationProvider.getNotifications();
    }

    ngOnDestroy(): void {
        if (this.notificationsSubscription)
            this.notificationsSubscription.unsubscribe();
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
