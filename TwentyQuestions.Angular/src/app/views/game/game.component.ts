import { NgModule, Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { GameService, AuthenticationService } from '@services';
import { GameEntity } from '@models';
import { DurationPipeModule } from '@pipes';
import { GameCompletedComponentModule } from './game-completed/game-completed.component';
import { GameGuessingComponentModule } from './game-guessing/game-guessing.component';
import { GameRespondingComponentModule } from './game-responding/game-responding.component';
import { GameWaitingComponentModule } from './game-waiting/game-waiting.component';

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
export class GameComponent implements OnInit, OnDestroy {
    GameMode = GameMode;

    id: string;
    userId: string;
    game: GameEntity;
    gameTimer: NodeJS.Timer;

    get gameMode(): GameMode {
        if (this.game) {
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

            if (this.id && !this.game)
                await this.loadGame();
        });
    }

    async ngOnDestroy(): Promise<void> {
        clearInterval(this.gameTimer);
    }

    async loadGame(game?: GameEntity): Promise<void> {
        clearTimeout(this.gameTimer);

        if (game)
            this.game = game;
        else
            this.game = await this.gameService.get(this.id).toPromise();

        if (this.game && this.gameMode == GameMode.Waiting)
            this.gameTimer = setTimeout(() => this.loadGame(), 30000);
    }
}

@NgModule({
    imports: [
        CommonModule,
        DurationPipeModule,
        GameCompletedComponentModule,
        GameGuessingComponentModule,
        GameRespondingComponentModule,
        GameWaitingComponentModule
    ],
    declarations: [GameComponent],
    exports: [GameComponent],
    providers: [GameService, AuthenticationService]
})
export class GameComponentModule { }