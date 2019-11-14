import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FriendEntity, FriendRequest } from '@models';
import { BaseEntityService } from './base.service';

@Injectable({
    providedIn: 'root'
})
export class FriendService extends BaseEntityService<FriendEntity, FriendRequest> {
    constructor(http: HttpClient) {
        super(http, '/Friend');
    }
}
