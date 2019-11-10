import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import {
    GameComponentModule, GameComponent,
    GamesComponentModule, GamesComponent,
    HomeComponentModule, HomeComponent,
    LoginComponentModule, LoginComponent
} from '@views';
import { AuthenticationGuard } from './authentication.guard';

const routes: Routes = [
    { path: '', component: HomeComponent, canActivate: [AuthenticationGuard] },
    { path: 'game', component: GameComponent, canActivate: [AuthenticationGuard] },
    { path: 'game/:id', component: GameComponent, canActivate: [AuthenticationGuard] },
    { path: 'games', component: GamesComponent, canActivate: [AuthenticationGuard] },
    { path: 'home', component: HomeComponent, canActivate: [AuthenticationGuard] },
    { path: 'login', component: LoginComponent },
];

@NgModule({
    imports: [
        GameComponentModule,
        GamesComponentModule,
        HomeComponentModule,
        LoginComponentModule,
        RouterModule.forRoot(routes)
    ],
    exports: [RouterModule]
})
export class AppRoutingModule { }
