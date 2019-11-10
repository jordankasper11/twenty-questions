import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { GameEntity, GameRequest } from '@models';
import { BaseEntityService } from './base.service';

@Injectable({
    providedIn: 'root'
})
export class GameService extends BaseEntityService<GameEntity, GameRequest> {
    constructor(http: HttpClient) {
        super(http, '/game');
    }
}
