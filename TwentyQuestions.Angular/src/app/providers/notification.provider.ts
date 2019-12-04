import { Injectable, EventEmitter } from '@angular/core';
import { HubConnectionBuilder, HubConnection } from '@microsoft/signalr';
import { environment } from '@environments';

@Injectable()
export class NotificationProvider {
    friendsUpdated = new EventEmitter();
    gamesListUpdated = new EventEmitter();
    gameUpdated = new EventEmitter<string>();

    connection: HubConnection;

    constructor() {
        this.createConnection();
    }

    async createConnection(): Promise<void> {
        const accessToken = localStorage['accessToken'];

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

            this.connection.on('UpdateGame', (gameId: string) => {
                this.gameUpdated.emit(gameId);
            });

            await this.connect();
        }
    }

    async connect(): Promise<void> {
        try {
            await this.connection.start();
        }
        catch{
            setTimeout(async () => await this.connect(), 10 * 1000);
        }
    }
}