import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { NotificationsEntity } from '@models';
import { BaseService } from './base.service';

@Injectable()
export class NotificationsService extends BaseService {
    constructor(http: HttpClient) {
        super(http);
    }

    public get(gamesLastChecked?: Date): Observable<NotificationsEntity> {
        let url = '/Notifications';

        if (gamesLastChecked)
            url += `?GamesLastChecked=${encodeURIComponent(gamesLastChecked.toISOString())}`;

        return this.httpGet<NotificationsEntity>(url);
    }
}