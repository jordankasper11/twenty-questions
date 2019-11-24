import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { GameEntity, GameRequest, AskQuestionRequest, AnswerQuestionRequest } from '@models';
import { BaseEntityService } from './base.service';

@Injectable({
    providedIn: 'root'
})
export class GameService extends BaseEntityService<GameEntity, GameRequest> {
    constructor(http: HttpClient) {
        super(http, '/Game');
    }

    acceptInvitation(id: string): Observable<void> {
        return this.httpGet(`${this.endPoint}/AcceptInvitation?id=${id}`);
    }

    declineInvitation(id: string): Observable<void> {
        return this.httpGet(`${this.endPoint}/DeclineInvitation?id=${id}`);
    }

    askQuestion(request: AskQuestionRequest): Observable<GameEntity> {
        return this.httpPost(`${this.endPoint}/AskQuestion`, request);
    }

    answerQuestion(request: AnswerQuestionRequest): Observable<GameEntity> {
        return this.httpPost(`${this.endPoint}/AnswerQuestion`, request);
    }
}
