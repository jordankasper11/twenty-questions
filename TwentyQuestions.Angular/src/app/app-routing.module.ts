import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { HomeComponentModule, HomeComponent, LoginComponentModule, LoginComponent } from '@views';
import { AuthenticationGuard } from './authentication.guard';

const routes: Routes = [
    { path: '', component: HomeComponent, canActivate: [AuthenticationGuard] },
    { path: 'home', component: HomeComponent, canActivate: [AuthenticationGuard] },
    { path: 'login', component: LoginComponent },
];

@NgModule({
    imports: [
        HomeComponentModule,
        LoginComponentModule,
        RouterModule.forRoot(routes)
    ],
    exports: [RouterModule]
})
export class AppRoutingModule { }
