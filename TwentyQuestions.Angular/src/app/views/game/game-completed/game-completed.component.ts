import { NgModule, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameEntity } from '@models';
import { GameQuestionsComponentModule } from '../game-questions/game-questions.component';

@Component({
    selector: 'app-game-completed',
    templateUrl: './game-completed.component.html'
})
export class GameCompletedComponent {
    @Input() game: GameEntity;

    constructor() {}
}

@NgModule({
    imports: [CommonModule, GameQuestionsComponentModule],
    declarations: [GameCompletedComponent],
    exports: [GameCompletedComponent]
})
export class GameCompletedComponentModule { }