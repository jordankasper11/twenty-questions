import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { FriendEntity, FriendRequest } from '@models';
import { BaseEntityService } from './base.service';

@Injectable()
export class FriendService extends BaseEntityService<FriendEntity, FriendRequest> {
    constructor(http: HttpClient) {
        super(http, '/Friend');
    }

    acceptInvitation(id: string): Observable<void> {
        return this.httpGet(`${this.endPoint}/AcceptInvitation?id=${id}`);
    }

    declineInvitation(id: string): Observable<void> {
        return this.httpGet(`${this.endPoint}/DeclineInvitation?id=${id}`);
    }
}
