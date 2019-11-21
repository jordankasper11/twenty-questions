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

    public getUsernameAvailability(username: string): Observable<boolean> {
        return this.httpGet<boolean>(`/User/GetUsernameAvailability?username=${username}`);
    }

    public register(registrationRequest: RegistrationRequest): Observable<UserEntity> {
        return this.httpPost<UserEntity>('/User/Register', registrationRequest);
    }

    public saveAvatar(userId: string, avatar: File) {
        const formData = new FormData();

        formData.set('avatar', avatar);

        return this.httpPost<UserEntity>(`/User/SaveAvatar?userId=${userId}`, formData);
    }

    public removeAvatar(userId: string) {
        return this.httpDelete<UserEntity>(`/User/RemoveAvatar?userId=${userId}`);
    }
}