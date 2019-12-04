import { NgModule, Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { GameService, AuthenticationService } from '@services';
import { GameEntity } from '@models';
import { DurationPipeModule } from '@pipes';
import { GameCompletedComponentModule } from './game-completed/game-completed.component';
import { GameGuessingComponentModule } from './game-guessing/game-guessing.component';
import { GameRespondingComponentModule } from './game-responding/game-responding.component';
import { GameWaitingComponentModule } from './game-waiting/game-waiting.component';
import { environment } from '@environments';
import { NotificationProvider } from '@providers';

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
    defaultAvatarUrl = environment.defaultAvatarUrl;
    userId: string;
    game: GameEntity;

    private notificationSubscription: Subscription;

    get gameMode(): GameMode {
        if (this.game) {
            if (this.game.completed)
                return GameMode.Completed;
            else if (this.game.opponentId == this.userId && (this.game.questions == null || this.game.questions.length == 0 || this.game.questions[this.game.questions.length - 1].response != null))
                return GameMode.Guessing;
            else if (this.game.createdBy == this.userId && (this.game.questions != null && this.game.questions.length && this.game.questions[this.game.questions.length - 1].response == null))
                return GameMode.Responding;
            else
                return GameMode.Waiting;
        }

        return null;
    }

    constructor(
        private gameService: GameService,
        private authenticationService: AuthenticationService,
        private notificationProvider: NotificationProvider,
        private route: ActivatedRoute
    ) { }

    ngOnInit(): void {
        this.userId = this.authenticationService.getAccessToken().userId;

        this.route.paramMap.subscribe(async params => {
            this.id = params.get('id');

            if (this.id && !this.game)
                await this.loadGame();
        });

        this.notificationSubscription = this.notificationProvider.gameUpdated.subscribe(async gameId => {
            if (gameId == this.id)
                await this.loadGame();
        })
    }

    async ngOnDestroy(): Promise<void> {
        if (this.notificationSubscription)
            this.notificationSubscription.unsubscribe();
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