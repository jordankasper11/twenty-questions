import { NgModule, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { GameService, AuthenticationService } from '@services';
import { GameEntity } from '@models';
import { CreateGameComponentModule } from './create-game/create-game.component';
import { GameCompletedComponentModule } from './game-completed/game-completed.component';
import { GameGuessingComponentModule } from './game-guessing/game-guessing.component';
import { GameRespondingComponentModule } from './game-responding/game-responding.component';

enum GameMode {
    Creating,
    Guessing,
    Responding,
    Waiting,
    Completed
}

@Component({
    selector: 'app-game',
    templateUrl: './game.component.html'
})
export class GameComponent implements OnInit {
    GameMode = GameMode;

    id: string;
    userId: string;
    game: GameEntity;

    get gameMode(): GameMode {
        if (!this.id)
            return GameMode.Creating;
        else if (this.game) {
            if (this.game.completed)
                return GameMode.Completed;
            else if (this.game.modifiedBy == this.userId)
                return GameMode.Waiting;
            else if (this.game.createdBy == this.userId)
                return GameMode.Responding;
            else
                return GameMode.Guessing;
        }

        return null;
    }

    constructor(
        private gameService: GameService,
        private authenticationService: AuthenticationService,
        private route: ActivatedRoute
    ) { }

    ngOnInit(): void {
        this.userId = this.authenticationService.getAccessToken().userId;

        this.route.paramMap.subscribe(async params => {
            this.id = params.get('id');

            if (this.id) {
                if (!this.game)
                    await this.loadGame();
            }
        });
    }

    async loadGame(game?: GameEntity): Promise<void> {
        if (game)
            this.game = game;
        else
            this.game = await this.gameService.get(this.id).toPromise();
    }

}

@NgModule({
    imports: [
        CommonModule,
        CreateGameComponentModule,
        GameCompletedComponentModule,
        GameGuessingComponentModule,
        GameRespondingComponentModule
    ],
    declarations: [GameComponent],
    exports: [GameComponent],
    providers: [GameService]
})
export class GameComponentModule { }