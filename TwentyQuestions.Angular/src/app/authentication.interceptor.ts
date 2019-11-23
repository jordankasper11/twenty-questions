import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpErrorResponse } from '@angular/common/http';
import { Subject, Observable, throwError } from 'rxjs';
import { catchError, switchMap, filter, take } from 'rxjs/operators';
import { AuthenticationService } from '@services';

@Injectable()
export class AuthenticationInterceptor implements HttpInterceptor {
    private refreshingToken = false;
    private refreshTokenSubject = new Subject<void>();

    constructor(private authenticationService: AuthenticationService) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        request = this.addRequestHeaders(request);

        const url = request.url.toLowerCase().replace(/https?:\/\/[^/]+/, '');

        return next.handle(request).pipe(
            catchError(error => {
                if (error instanceof HttpErrorResponse && error.status == 401 && url != '/api/authentication/login')
                    return this.handleUnauthorizedRequest(request, next, error);

                return throwError(error);
            })
        );
    }

    private addRequestHeaders(request: HttpRequest<any>) {
        if (this.authenticationService.isLoggedIn()) {
            request = request.clone({
                setHeaders: {
                    Authorization: `Bearer ${this.authenticationService.accessToken}`
                }
            });
        }

        return request;
    }

    private handleUnauthorizedRequest(request: HttpRequest<any>, next: HttpHandler, error: any) {
        if (!this.refreshingToken) {
            this.refreshingToken = true;
            this.refreshTokenSubject.next();

            return this.authenticationService.refreshToken().pipe(
                switchMap(() => {
                    this.refreshingToken = false;
                    this.refreshTokenSubject.next();

                    return next.handle(this.addRequestHeaders(request));
                })
            );

        } else {
            return this.refreshTokenSubject.pipe(
                take(1),
                switchMap(() => {
                    if (this.authenticationService.isLoggedIn())
                        return next.handle(this.addRequestHeaders(request));

                    return throwError(error);
                })
            );
        }
    }
}
