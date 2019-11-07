import { Injectable } from '@angular/core';
import {
    CanActivate,
    ActivatedRouteSnapshot,
    RouterStateSnapshot,
    Router
} from '@angular/router';
import { AuthenticationService } from '@services';

@Injectable({
    providedIn: 'root'
})
export class AuthenticationGuard implements CanActivate {
    constructor(private authenticationService: AuthenticationService, private router: Router) {}

    canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot ): boolean {
        if (this.authenticationService.isLoggedIn())
            return true;
        else {
            this.router.navigate(['/login'], { queryParams: { returnUrl: state.url } });

            return false;
        }
    }
}
