import { NgModule, Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DurationPipeModule } from '@pipes';
import { GameService, AuthenticationService } from '@services';
import { GameRequest, GameEntity, EntityStatus } from '@models';

@Component({
    selector: 'app-games',
    templateUrl: './games.component.html'
})
export class GamesComponent implements OnInit, OnDestroy {
    invitations: Array<GameEntity>;
    games: Array<GameEntity>;
    gamesTimer: NodeJS.Timer;

    constructor(private gameService: GameService, private authenticationService: AuthenticationService) { }

    async ngOnInit(): Promise<void> {
        await this.loadGames();

        this.gamesTimer = setInterval(() => this.loadGames(), 300 * 1000);
    }

    async ngOnDestroy(): Promise<void> {
        clearInterval(this.gamesTimer);
    }

    async loadGames(): Promise<void> {
        const request = new GameRequest();

        request.status = EntityStatus.Active | EntityStatus.Pending;
        request.completed = false;

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
        await this.loadGames();
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
