import { Injectable } from '@angular/core';
import {
    CanActivate,
    ActivatedRouteSnapshot,
    RouterStateSnapshot,
    Router
} from '@angular/router';
import { AuthenticationService } from '@services';

@Injectable()
export class AuthenticationGuard implements CanActivate {
    constructor(private authenticationService: AuthenticationService, private router: Router) { }

    async canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {
        if (this.authenticationService.isLoggedIn())
            return true;

        try {
            await this.authenticationService.refreshToken().toPromise();
        }
        catch { }

        if (this.authenticationService.isLoggedIn())
            return true;

        await this.router.navigate(['/login'], { queryParams: { returnUrl: state.url } });

        return false;
    }
}
