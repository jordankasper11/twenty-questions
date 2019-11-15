import { NgModule, Component, OnInit, Output, Input, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { GameService } from '@services';
import { GameEntity, QuestionResponse, AskQuestionRequest } from '@models';

@Component({
    selector: 'app-game-guessing',
    templateUrl: './game-guessing.component.html'
})
export class GameGuessingComponent implements OnInit {
    QuestionResponse = QuestionResponse;

    @Output() gameSaved = new EventEmitter<GameEntity>();

    @Input() game: GameEntity;

    form: FormGroup;

    constructor(private gameService: GameService) { }

    ngOnInit(): void {
        this.buildForm();
    }

    buildForm(): void {
        this.form = new FormGroup({
            question: new FormControl('', [Validators.required])
        });
    }

    async submit(): Promise<void> {
        if (this.form.valid) {
            const request = new AskQuestionRequest(this.game.id);

            Object.assign(request, this.form.value);

            const game = await this.gameService.askQuestion(request).toPromise();

            this.gameSaved.emit(game);
        }
    }
}

@NgModule({
    imports: [CommonModule, ReactiveFormsModule],
    declarations: [GameGuessingComponent],
    exports: [GameGuessingComponent],
    providers: [GameService]
})
export class GameGuessingComponentModule { }