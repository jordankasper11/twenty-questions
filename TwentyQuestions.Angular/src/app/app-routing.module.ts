import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import {
    FriendsComponentModule, FriendsComponent,
    FriendsSearchComponentModule, FriendsSearchComponent,
    GameComponentModule, GameComponent,
    GamesComponentModule, GamesComponent,
    LoginComponentModule, LoginComponent,
    NewGameComponentModule, NewGameComponent,
    RegistrationComponentModule, RegistrationComponent,
    SettingsComponentModule, SettingsComponent
} from '@views';
import { AuthenticationGuard } from './authentication.guard';

const routes: Routes = [
    { path: '', component: GamesComponent, canActivate: [AuthenticationGuard] },
    { path: 'friends', component: FriendsComponent, canActivate: [AuthenticationGuard] },
    { path: 'friends/search', component: FriendsSearchComponent, canActivate: [AuthenticationGuard] },
    { path: 'games', component: GamesComponent, canActivate: [AuthenticationGuard] },
    { path: 'games/new', component: NewGameComponent, canActivate: [AuthenticationGuard] },
    { path: 'games/:id', component: GameComponent, canActivate: [AuthenticationGuard] },
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegistrationComponent },
    { path: 'settings', component: SettingsComponent, canActivate: [AuthenticationGuard] },
];

@NgModule({
    imports: [
        FriendsComponentModule,
        FriendsSearchComponentModule,
        GameComponentModule,
        GamesComponentModule,
        LoginComponentModule,
        NewGameComponentModule,
        RegistrationComponentModule,
        SettingsComponentModule,
        RouterModule.forRoot(routes)
    ],
    exports: [RouterModule]
})
export class AppRoutingModule { }
