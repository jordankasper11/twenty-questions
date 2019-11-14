import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

import { AccessToken, LoginRequest } from '@models';
import { BaseService } from './base.service';

@Injectable({
    providedIn: 'root'
})
export class AuthenticationService extends BaseService {
    constructor(http: HttpClient) {
        super(http);
    }

    get token(): string {
        return localStorage['token'];
    }

    set token(value) {
        localStorage['token'] = value;
    }

    public getAccessToken(): AccessToken {
        if (this.token) {
            const encodedToken = /\.([^.]+)\./.exec(this.token)[1];
            const decodedToken = <AccessToken>JSON.parse(atob(encodedToken));

            return decodedToken;
        }

        return null;
    }

    public isLoggedIn(): boolean {
        const accessToken = this.getAccessToken();

        if (accessToken) {
            const expirationDate = new Date(accessToken.exp * 1000);

            return expirationDate.getTime() > new Date().getTime();
        }

        return false;
    }

    public login(loginRequest: LoginRequest): Observable<AccessToken> {
        return super
            .httpPost<string>('/Authentication/Login', loginRequest)
            .pipe(
                map(token => {
                    this.token = token;

                    const accessToken = this.getAccessToken();

                    return accessToken;
                }),
                catchError(error => throwError(error))
            );
    }

    public async logout(): Promise<void> {
        localStorage.removeItem('token');
    }
}
