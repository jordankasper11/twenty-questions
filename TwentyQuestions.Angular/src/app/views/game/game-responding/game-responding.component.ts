import { NgModule, Component, OnInit, Output, Input, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { GameService } from '@services';
import { GameEntity, QuestionEntity, QuestionResponse, AnswerQuestionRequest } from '@models';
import { GameQuestionsComponentModule } from '../game-questions/game-questions.component';

@Component({
    selector: 'app-game-responding',
    templateUrl: './game-responding.component.html'
})
export class GameRespondingComponent implements OnInit {
    QuestionResponse = QuestionResponse;

    @Output() gameSaved = new EventEmitter<GameEntity>();

    @Input() game: GameEntity;

    form: FormGroup;
    question: QuestionEntity;

    constructor(private gameService: GameService) { }

    ngOnInit(): void {
        this.question = this.game.questions[this.game.questions.length - 1];

        this.buildForm();
    }

    buildForm() {
        this.form = new FormGroup({
            response: new FormControl('', [Validators.required]),
            responseExplanation: new FormControl('')
        });
    }

    async submit(): Promise<void> {
        if (this.form.valid) {
            const question = this.game.questions[this.game.questions.length - 1];
            const request = new AnswerQuestionRequest(this.game.id, question.id);

            Object.assign(request, this.form.value);

            const game = await this.gameService.answerQuestion(request).toPromise();

            this.gameSaved.emit(game);
        }
    }
}

@NgModule({
    imports: [CommonModule, ReactiveFormsModule, GameQuestionsComponentModule],
    declarations: [GameRespondingComponent],
    exports: [GameRespondingComponent],
    providers: [GameService]
})
export class GameRespondingComponentModule { }