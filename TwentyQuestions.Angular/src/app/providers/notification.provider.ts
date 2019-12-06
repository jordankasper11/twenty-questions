import { Injectable, EventEmitter } from '@angular/core';
import { HubConnectionBuilder, HubConnection } from '@microsoft/signalr';
import { NotificationsService, AuthenticationService } from '@services';
import { environment } from '@environments';
import { NotificationsEntity } from '@models';

@Injectable()
export class NotificationProvider {
    notificationsUpdated = new EventEmitter<NotificationsEntity>();
    gameUpdated = new EventEmitter<string>();

    get gamesLastChecked(): Date {
        const value = localStorage['gamesLastChecked'];

        if (value)
            return new Date(value);

        return null;
    }

    set gamesLastChecked(value: Date) {
        const key = 'gamesLastChecked';

        if (value)
            localStorage[key] = value.toISOString();
        else
            localStorage.removeItem(key);
    }

    private connection: HubConnection;
    private notifications: NotificationsEntity;

    constructor(private notificationsService: NotificationsService, private authenticationService: AuthenticationService) {
        this.createConnection();
    }

    async createConnection(): Promise<void> {
        const accessToken = this.authenticationService.accessToken;

        if (accessToken) {
            this.connection = new HubConnectionBuilder()
                .withUrl(`${environment.requestUrlPrefix}/hubs/notifications`, { accessTokenFactory: () => accessToken })
                .withAutomaticReconnect({
                    nextRetryDelayInMilliseconds: retryContext => {
                        if (retryContext.elapsedMilliseconds < 10 * 60 * 1000)
                            return 10 * 1000;
                        else if (retryContext.elapsedMilliseconds < 60 * 60 * 24 * 1000)
                            return 60 * 1000;

                        return null;
                    }
                })
                .build();

            this.connection.onreconnected(async () => {
                this.notifications = await this.getNotifications(true);
            });

            this.connection.on('UpdateGame', async (gameId: string) => {
                this.gameUpdated.emit(gameId);

                await this.updateNotifications();
            });

            this.connection.on('UpdateFriendsList', async () => {
                this.notifications = await this.getNotifications(true);
            });
            this.connection.on('UpdateGamesList', async () => {
                this.notifications = await this.getNotifications(true);
            });

            await this.connect();
        }
    }

    async connect(): Promise<void> {
        try {
            await this.connection.start();
        }
        catch{
            setTimeout(async () => await this.connect(), 60 * 1000);
        }
    }

    async getNotifications(forceUpdate?: boolean): Promise<NotificationsEntity> {
        if (!this.notifications || forceUpdate)
            await this.updateNotifications();

        return this.notifications
    }

    async updateNotifications(): Promise<void> {
        if (this.authenticationService.isLoggedIn()) {
            this.notifications = await this.notificationsService.get(this.gamesLastChecked).toPromise();

            this.notificationsUpdated.emit(this.notifications);
        }
        else
            this.notifications = null;
    }
}