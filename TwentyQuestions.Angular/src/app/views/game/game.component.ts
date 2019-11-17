import { NgModule, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { GameService, AuthenticationService, FriendService } from '@services';
import { GameEntity, FriendRequest, FriendEntity } from '@models';
import { DurationPipeModule } from '@pipes';
import { CreateGameComponentModule } from './create-game/create-game.component';
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
export class GameComponent implements OnInit {
    GameMode = GameMode;

    id: string;
    userId: string;
    game: GameEntity;
    gameTimer: NodeJS.Timer;
    friend: FriendEntity;

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
        private friendService: FriendService,
        private route: ActivatedRoute
    ) { }

    ngOnInit(): void {
        this.userId = this.authenticationService.getAccessToken().userId;

        this.route.paramMap.subscribe(async params => {
            this.id = params.get('id');

            if (this.id) {
                if (!this.game) {
                    await this.loadGame();
                    await this.loadFriend();
                }
            }
        });
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

    async loadFriend(): Promise<void> {
        const request = new FriendRequest();

        request.friendId = this.game.createdBy == this.userId ? this.game.opponentId : this.game.createdBy;

        const response = await this.friendService.query(request).toPromise();

        if (response != null && response.items != null && response.items.length == 1)
            this.friend = response.items[0];
    }
}

@NgModule({
    imports: [
        CommonModule,
        DurationPipeModule,
        CreateGameComponentModule,
        GameCompletedComponentModule,
        GameGuessingComponentModule,
        GameRespondingComponentModule,
        GameWaitingComponentModule
    ],
    declarations: [GameComponent],
    exports: [GameComponent],
    providers: [GameService, AuthenticationService, FriendService]
})
export class GameComponentModule { }