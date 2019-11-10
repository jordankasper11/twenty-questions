import { NgModule, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import {
    ReactiveFormsModule,
    FormControl,
    FormGroup,
    Validators
} from '@angular/forms';
import { GameService, FriendService, AuthenticationService } from '@services';
import { GameRequest, FriendRequest, FriendEntity, GameEntity } from '@models';

@Component({
    selector: 'app-game',
    templateUrl: './game.component.html'
})
export class GameComponent implements OnInit {
    id: string;
    game: GameEntity;
    form: FormGroup;
    friends: Array<FriendEntity>;

    constructor(
        private gameService: GameService,
        private friendService: FriendService,
        private authenticationService: AuthenticationService,
        private route: ActivatedRoute,
        private router: Router
    ) { }

    async ngOnInit(): Promise<void> {
        this.buildForm();

        await this.loadFriends();

        this.route.paramMap.subscribe(async params => {
            this.id = params.get('id');

            if (!this.game)
                await this.loadGame();
        });
    }

    buildForm() {
        this.form = new FormGroup({
            subject: new FormControl('', [Validators.required]),
            opponentId: new FormControl('', [Validators.required])
        });
    }

    async loadFriends(): Promise<void> {
        const request = new FriendRequest();
        const response = await this.friendService.query(request).toPromise();

        this.friends = response.items;
    }

    async loadGame(): Promise<void> {
        this.game = await this.gameService.get(this.id).toPromise();
    }

    async submit() {
        if (this.form.valid) {
            let game: GameEntity = new GameEntity();

            game.maxQuestions = 20;

            Object.assign(game, this.form.value);

            this.game = await this.gameService.upsert(game).toPromise();

            this.router.navigate(['/game', game.id]);
        }
    }
}

@NgModule({
    imports: [CommonModule, ReactiveFormsModule],
    declarations: [GameComponent],
    exports: [GameComponent]
})
export class GameComponentModule { }
