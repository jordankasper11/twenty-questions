import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserEntity, UserRequest, RegistrationRequest } from '@models';
import { BaseEntityService } from './base.service';

@Injectable({
    providedIn: 'root'
})
export class UserService extends BaseEntityService<UserEntity, UserRequest> {
    constructor(http: HttpClient) {
        super(http, '/User');
    }

    public register(registrationRequest: RegistrationRequest): Observable<UserEntity> {
        return this.httpPost<UserEntity>('/User/Register', registrationRequest);
    }
}