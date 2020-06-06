import { NgModule, Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NotificationProvider } from '@providers';
import { GameService, AuthenticationService } from '@services';
import { GameEntity, NotificationType } from '@models';
import { DurationPipeModule } from '@pipes';
import { GameCompletedComponentModule } from './game-completed/game-completed.component';
import { GameGuessingComponentModule } from './game-guessing/game-guessing.component';
import { GameRespondingComponentModule } from './game-responding/game-responding.component';
import { GameWaitingComponentModule } from './game-waiting/game-waiting.component';
import { environment } from '@environments';

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

    private componentDestroyed = new Subject();

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

        this.route.paramMap
            .pipe(
                takeUntil(this.componentDestroyed)
            )
            .subscribe(async params => {
                this.id = params.get('id');

                if (this.id && !this.game)
                    await this.loadGame();
            });

        this.notificationProvider.notificationsUpdated
            .pipe(
                takeUntil(this.componentDestroyed)
            )
            .subscribe(async notifications => {
                if (notifications.some(n => n.type == NotificationType.Game && n.recordId == this.id))
                    await this.loadGame();
            });
    }

    async ngOnDestroy(): Promise<void> {
        this.componentDestroyed.next();
        this.componentDestroyed.complete();
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
    exports: [GameComponent]
})
export class GameComponentModule { }