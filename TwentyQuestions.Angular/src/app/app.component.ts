import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { Subject } from 'rxjs';
import { AuthenticationService } from '@services';
import { NotificationProvider } from '@providers';
import { NotificationEntity, NotificationType } from '@models';
import { takeUntil } from 'rxjs/operators';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styles: []
})
export class AppComponent implements OnInit, OnDestroy {
    url: string;
    menuExpanded: boolean = false;
    friendNotifications = false;
    gameNotifications = false;

    private componentDestroyed = new Subject();

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

        this.notificationProvider.notificationsUpdated
            .pipe(
                takeUntil(this.componentDestroyed)
            )
            .subscribe(notifications => {
                this.friendNotifications = notifications.some(n => n.type == NotificationType.Friend);
                this.gameNotifications = notifications.some(n => n.type == NotificationType.Game);
            });
    }

    ngOnDestroy(): void {
        this.componentDestroyed.next();
        this.componentDestroyed.complete();
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
