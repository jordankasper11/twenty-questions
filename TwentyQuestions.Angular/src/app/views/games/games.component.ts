import { NgModule, Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DurationPipeModule } from '@pipes';
import { GameService, AuthenticationService } from '@services';
import { NotificationProvider } from '@providers';
import { GameRequest, GameEntity, EntityStatus } from '@models';
import { environment } from '@environments';

@Component({
    selector: 'app-games',
    templateUrl: './games.component.html'
})
export class GamesComponent implements OnInit, OnDestroy {
    defaultAvatarUrl = environment.defaultAvatarUrl;
    invitations: Array<GameEntity>;
    games: Array<GameEntity>;

    private componentDestroyed = new Subject<void>();

    constructor(
        private gameService: GameService,
        private authenticationService: AuthenticationService,
        private notificationProvider: NotificationProvider,
        private router: Router
    ) { }

    async ngOnInit(): Promise<void> {
        this.notificationProvider.notificationsUpdated
            .pipe(
                takeUntil(this.componentDestroyed)
            )
            .subscribe(async () => await this.loadGames());

        await this.loadGames();
    }

    async ngOnDestroy(): Promise<void> {
        this.componentDestroyed.next();
        this.componentDestroyed.complete();
    }

    async loadGames(): Promise<void> {
        const request = new GameRequest();

        request.status = EntityStatus.Active | EntityStatus.Pending;

        const response = await this.gameService.query(request).toPromise();
        const userId = this.authenticationService.getAccessToken().userId;

        if (response) {
            this.invitations = response.items.filter(g => g.status == EntityStatus.Pending && g.opponentId == userId);
            this.games = response.items.filter(g => g.status == EntityStatus.Active);
        }
    }

    getStatus(game: GameEntity): string {
        if (game.completed)
            return 'Completed';
        else if (game.modifiedBy == this.authenticationService.getAccessToken().userId)
            return 'Waiting';
        else
            return 'Your turn'
    }

    async acceptInvitation(game: GameEntity): Promise<void> {
        await this.gameService.acceptInvitation(game.id).toPromise();
        
        await this.router.navigate(['/games', game.id]);
    }

    async declineInvitation(game: GameEntity): Promise<void> {
        await this.gameService.declineInvitation(game.id).toPromise();
        await this.loadGames();
    }
}

@NgModule({
    imports: [CommonModule, RouterModule, DurationPipeModule],
    declarations: [GamesComponent],
    exports: [GamesComponent],
    providers: [GameService, AuthenticationService]
})
export class GamesComponentModule { }
