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
import {
    GameRequest,
    FriendRequest,
    FriendEntity,
    GameEntity,
    QuestionResponse,
    AskQuestionRequest,
    AnswerQuestionRequest
} from '@models';

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
    QuestionResponse = QuestionResponse;

    id: string;
    userId: string;
    game: GameEntity;
    form: FormGroup;
    friends: Array<FriendEntity>;
    explanationVisible: boolean = false;

    get gameMode(): GameMode {
        if (!this.id)
            return GameMode.Creating;
        else {
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
        private friendService: FriendService,
        private authenticationService: AuthenticationService,
        private route: ActivatedRoute,
        private router: Router
    ) { }

    async ngOnInit(): Promise<void> {
        this.userId = this.authenticationService.getAccessToken().userId;

        await this.loadFriends();

        this.route.paramMap.subscribe(async params => {
            this.id = params.get('id');

            if (this.id) {
                if (!this.game)
                    await this.loadGame();
            }

            this.buildForm();
        });
    }

    buildForm() {
        this.form = null;

        if (!this.id) {
            this.form = new FormGroup({
                subject: new FormControl('', [Validators.required]),
                opponentId: new FormControl('', [Validators.required])
            });
        }
        else if (this.gameMode == GameMode.Guessing) {
            this.form = new FormGroup({
                question: new FormControl('', [Validators.required])
            });
        }
        else if (this.gameMode == GameMode.Responding) {
            this.form = new FormGroup({
                response: new FormControl('', [Validators.required]),
                responseExplanation: new FormControl('')
            });
        }
    }

    async loadFriends(): Promise<void> {
        const request = new FriendRequest();
        const response = await this.friendService.query(request).toPromise();

        this.friends = response.items;
    }

    async loadGame(): Promise<void> {
        this.game = await this.gameService.get(this.id).toPromise();
    }

    async createGame() {
        if (this.form.valid) {
            let game: GameEntity = new GameEntity();

            game.maxQuestions = 20;

            Object.assign(game, this.form.value);

            this.game = await this.gameService.upsert(game).toPromise();

            this.router.navigate(['/game', game.id]);
        }
    }

    async askQuestion() {
        if (this.form.valid) {
            const request = new AskQuestionRequest(this.game.id);

            Object.assign(request, this.form.value);

            this.game = await this.gameService.askQuestion(request).toPromise();
        }
    }

    async answerQuestion() {
        if (this.form.valid) {
            const question = this.game.questions[this.game.questions.length - 1];
            const request = new AnswerQuestionRequest(this.game.id, question.id);

            Object.assign(request, this.form.value);

            this.game = await this.gameService.answerQuestion(request).toPromise();
        }
    }

    showExplanation(){
        this.explanationVisible = true;
    }
}

@NgModule({
    imports: [CommonModule, ReactiveFormsModule],
    declarations: [GameComponent],
    exports: [GameComponent]
})
export class GameComponentModule { }
