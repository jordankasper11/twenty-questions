import { NgModule, Component, OnInit, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { GameService, FriendService } from '@services';
import { FriendRequest, FriendEntity, GameEntity, QuestionResponse } from '@models';

@Component({
    selector: 'app-create-game',
    templateUrl: './create-game.component.html'
})
export class CreateGameComponent implements OnInit {
    QuestionResponse = QuestionResponse;

    @Input() game: GameEntity;

    form: FormGroup;
    friends: Array<FriendEntity>;

    constructor(
        private gameService: GameService,
        private friendService: FriendService,
        private router: Router
    ) { }

    async ngOnInit(): Promise<void> {
        await this.loadFriends();

        this.buildForm();
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

    async submit(): Promise<void> {
        if (this.form.valid) {
            let game: GameEntity = new GameEntity();

            game.maxQuestions = 20;

            Object.assign(game, this.form.value);

            game = await this.gameService.upsert(game).toPromise();

            this.router.navigate(['/game', game.id]);
        }
    }
}

@NgModule({
    imports: [CommonModule, ReactiveFormsModule],
    declarations: [CreateGameComponent],
    exports: [CreateGameComponent]
})
export class CreateGameComponentModule { }