import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError, empty } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { AccessToken, LoginRequest, LoginResponse, RefreshTokenRequest } from '@models';
import { BaseService } from './base.service';

@Injectable({
    providedIn: 'root'
})
export class AuthenticationService extends BaseService {
    constructor(http: HttpClient) {
        super(http);
    }

    get accessToken(): string {
        return localStorage['accessToken'];
    }

    set accessToken(value) {
        const key = 'accessToken';

        if (value)
            localStorage[key] = value;
        else
            localStorage.removeItem(key);
    }

    private get _refreshToken(): string {
        return localStorage['refreshToken'];
    }

    private set _refreshToken(value) {
        const key = 'refreshToken';

        if (value)
            localStorage[key] = value;
        else
            localStorage.removeItem(key);
    }

    public getAccessToken(): AccessToken {
        if (this.accessToken) {
            const encodedToken = /\.([^.]+)\./.exec(this.accessToken)[1];
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
        return this
            .httpPost<LoginResponse>('/Authentication/Login', loginRequest)
            .pipe(
                map(loginResponse => {
                    this.accessToken = loginResponse.accessToken;
                    this._refreshToken = loginResponse.refreshToken;

                    return this.getAccessToken();
                }),
                catchError(error => throwError(error))
            );
    }

    public refreshToken(): Observable<AccessToken> {
        if (!this._refreshToken)
            return empty();

        const request = new RefreshTokenRequest();

        request.refreshToken = this._refreshToken;

        return super
            .httpPost<LoginResponse>('/Authentication/RefreshToken', request)
            .pipe(
                map(loginResponse => {
                    this.accessToken = loginResponse.accessToken;
                    this._refreshToken = loginResponse.refreshToken;

                    return this.getAccessToken();
                }),
                catchError(error => throwError(error))
            );
    }

    public async logout(): Promise<void> {
        this.accessToken = null;
        this.refreshToken = null;
    }
}