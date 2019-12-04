import { Injectable, EventEmitter } from '@angular/core';
import { HubConnectionBuilder, HubConnection } from '@microsoft/signalr';
import { environment } from '@environments';

@Injectable()
export class NotificationProvider {
    gameUpdated = new EventEmitter<string>();

    connection: HubConnection;

    constructor() {
        this.connect();
    }

    async connect(): Promise<void> {
        const accessToken = localStorage['accessToken'];

        this.connection = new HubConnectionBuilder().withUrl(`${environment.requestUrlPrefix}/hubs/notifications`, { accessTokenFactory: () => accessToken }).build();

        await this.connection.start();

        this.connection.on('UpdateGame', (gameId: string) => {
            this.gameUpdated.emit(gameId);
        });
    }
}
