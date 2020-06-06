import { Injectable } from '@angular/core';
import { BehaviorSubject, Subject } from 'rxjs';
import { HubConnectionBuilder, HubConnection, HubConnectionState } from '@microsoft/signalr';
import { AuthenticationService } from '@services';
import { environment } from '@environments';
import { NotificationEntity, NotificationType } from '@models';

@Injectable()
export class NotificationProvider {
    notificationsUpdated = new BehaviorSubject<Array<NotificationEntity>>([]);
    refreshFriendsList = new Subject<void>();
    refreshGame = new Subject<string>();

    private connection: HubConnection;
    private notifications: Array<NotificationEntity> = [];

    constructor(private authenticationService: AuthenticationService) {
    }

    async connect(): Promise<void> {
        try {
            this.connection = await this.createConnection();

            await this.connection.start();
        }
        catch {
            setTimeout(async () => await this.connect(), 60 * 1000);
        }
    }

    async createConnection(): Promise<HubConnection> {
        const accessToken = this.authenticationService.accessToken;

        if (accessToken) {
            const connection = new HubConnectionBuilder()
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

            connection.on('NotificationsReceived', async (notifications: Array<NotificationEntity>) => {
                const newNotifications = notifications.filter(n => !this.notifications.some(en => en.id == n.id));

                this.notifications = notifications.concat(newNotifications);

                this.notificationsUpdated.next(this.notifications);

                notifications.filter(n => n.type == NotificationType.Game).forEach(n => this.refreshGame.next(n.recordId))
            });

            connection.on('NotificationsRemoved', async (notifications: Array<NotificationEntity>) => {
                this.notifications = this.notifications.filter(en => !notifications.some(n => n.id == en.id));

                this.notificationsUpdated.next(this.notifications);
            });

            connection.on('FriendsListUpdated', async (notifications: Array<NotificationEntity>) => this.refreshFriendsList.next());

            return connection;
        }
    }

    async removeNotification(options: {
        id?: string,
        type?: NotificationType,
        recordId?: string
    }): Promise<void> {
        if(options && this.connection.state == HubConnectionState.Connected)
            await this.connection.send('RemoveNotification', options.id, options.type, options.recordId);
    }
}