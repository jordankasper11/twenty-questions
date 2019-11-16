import { NgModule, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameEntity, QuestionEntity, QuestionResponse } from '@models';

@Component({
    selector: 'app-game-questions',
    templateUrl: './game-questions.component.html'
})
export class GameQuestionsComponent {
    QuestionResponse = QuestionResponse;

    @Input() game: GameEntity;

    constructor() { }

    toggleResponseExplanation(question: QuestionEntity): void {
        question['responseExplanationVisible'] = !question['responseExplanationVisible'];
    }
}

@NgModule({
    imports: [CommonModule],
    declarations: [GameQuestionsComponent],
    exports: [GameQuestionsComponent]
})
export class GameQuestionsComponentModule { }