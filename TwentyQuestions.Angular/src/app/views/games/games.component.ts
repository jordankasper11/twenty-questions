import { NgModule, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DurationPipeModule } from '@pipes';
import { GameService, AuthenticationService } from '@services';
import { GameRequest, GameEntity, EntityStatus } from '@models';

@Component({
    selector: 'app-games',
    templateUrl: './games.component.html'
})
export class GamesComponent implements OnInit {
    invitations: Array<GameEntity>;
    games: Array<GameEntity>;

    constructor(private gameService: GameService, private authenticationService: AuthenticationService) { }

    async ngOnInit(): Promise<void> {
        await this.load();
    }

    async load(): Promise<void> {
        const request = new GameRequest();

        request.completed = false;

        const response = await this.gameService.query(request).toPromise();

        if (response) {
            this.invitations = response.items.filter(g => g.status == EntityStatus.Pending);
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
}

@NgModule({
    imports: [CommonModule, RouterModule, DurationPipeModule],
    declarations: [GamesComponent],
    exports: [GamesComponent],
    providers: [GameService, AuthenticationService]
})
export class GamesComponentModule { }
