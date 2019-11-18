import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import {
    GameComponentModule, GameComponent,
    GamesComponentModule, GamesComponent,
    LoginComponentModule, LoginComponent
} from '@views';
import { AuthenticationGuard } from './authentication.guard';

const routes: Routes = [
    { path: '', component: GamesComponent, canActivate: [AuthenticationGuard] },
    { path: 'game', component: GameComponent, canActivate: [AuthenticationGuard] },
    { path: 'game/:id', component: GameComponent, canActivate: [AuthenticationGuard] },
    { path: 'games', component: GamesComponent, canActivate: [AuthenticationGuard] },
    { path: 'login', component: LoginComponent },
];

@NgModule({
    imports: [
        GameComponentModule,
        GamesComponentModule,
        LoginComponentModule,
        RouterModule.forRoot(routes)
    ],
    exports: [RouterModule]
})
export class AppRoutingModule { }
